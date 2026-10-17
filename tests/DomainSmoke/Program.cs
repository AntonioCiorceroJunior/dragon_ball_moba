// Build-and-run smoke checks for the Unity-independent domain. No external NuGet packages.
using System;
using DragonMoba.Domain;

static class Program
{
    private static int passed;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("DOMAIN SMOKE FAILED: " + message);
        passed++;
    }
    private static bool Cmd(GameWorld w, EntityState hero, CommandKind kind,
        string item = null, V2 pos = default, AbilitySlot slot = AbilitySlot.Q)
    {
        return w.Submit(new GameCommand { ActorId=hero.Id, Kind=kind, ItemId=item,
            Position=pos, Slot=slot });
    }
    public static void Main()
    {
        var w = new GameWorld("GKU_KID",ArenaLayout.Training);
        var hero = w.Player;
        Check(hero.MaxHp == 570 && hero.HpRegenPer5 >= 6,
            "base Kid stats and regeneration");
        hero.Position=hero.Destination=new V2(30,-36);
        hero.Hp=200;
        for(var i=0;i<30;i++)w.Tick(GameWorld.FixedStep);
        Check(hero.Hp>201 && hero.Hp<202, "HP regen away from fountain");
        hero.Position=hero.Destination=new V2(0,-34);
        hero.Zeni=20000;
        Check(Cmd(w,hero,CommandKind.Buy,"GLOVES") &&
            Cmd(w,hero,CommandKind.Buy,"GLOVES") &&
            Cmd(w,hero,CommandKind.Buy,"HEALTH_GEM"),"buy components");
        Check(w.RemainingItemCost(hero,"DRAGON_GLOVES")==1700,
            "composite recipe deducts owned ingredients");
        Check(Cmd(w,hero,CommandKind.Buy,"DRAGON_GLOVES") &&
            hero.Inventory.Count==1 && hero.Zeni==17200 &&
            hero.PhysicalAttack==100 && hero.MaxHp==770,
            "recipe replaces all components with one final modifier");
        Check(Cmd(w,hero,CommandKind.Sell,"DRAGON_GLOVES") &&
            hero.MaxHp==570 && hero.PhysicalAttack==55 && hero.Zeni==19160,
            "sell removes modifiers, returns 70 percent");
        Check(Cmd(w,hero,CommandKind.Buy,"VAMP_ESSENCE") &&
            Cmd(w,hero,CommandKind.Buy,"HASTE_SHARD"),"lifesteal and haste items");
        EntityState enemy=null;
        foreach(var e in w.Entities)
            if(e.Kind==EntityKind.Hero && e.Team==Team.Red)enemy=e;
        Check(enemy!=null,"enemy hero exists");
        hero.Hp=100;
        Check(w.DealDamage(hero,enemy,100,false,true) && hero.Hp>104,
            "lifesteal heals only actual HP loss");
        hero.Hp=100;enemy.Shield=1000;
        Check(w.DealDamage(hero,enemy,100,false,true) && hero.Hp==100,
            "shield prevents lifesteal on blocked hit");
        Check(Cmd(w,hero,CommandKind.UpgradeAbility) &&
            Cmd(w,hero,CommandKind.Cast,pos:hero.Position) &&
            hero.SkillCooldown[0]>3.6 && hero.SkillCooldown[0]<3.7,
            "haste reduces cooldown");

        var apeWorld=new GameWorld("GKU_KID",ArenaLayout.Training);
        var kid=apeWorld.Player;
        kid.Position=kid.Destination=new V2(0,-17.2f);
        apeWorld.GrantExperience(kid,99999);
        Check(kid.Level==18 && kid.FormId=="kid_oozaru" &&
            apeWorld.BodyRadius(kid)>.9 && kid.MoveSpeed<6 &&
            !apeWorld.IsStructureCollision(kid,kid.Position),
            "Oozaru grows, slows and never transforms inside a tower");

        var combat=new GameWorld("GKU_KID",ArenaLayout.Training);
        var actor=combat.Player;
        EntityState victim=null;
        foreach(var e in combat.Entities)
            if(e.Kind==EntityKind.Hero && e.Team==Team.Red)victim=e;
        Check(victim!=null,"attack-move fixture enemy");
        actor.Position=actor.Destination=new V2(30,-36);
        actor.Hp=actor.MaxHp=5000;
        victim.Position=victim.Destination=new V2(30,-26);
        victim.MoveSpeed=victim.PhysicalAttack=victim.AttackRange=0;
        victim.MaxHp=victim.Hp=5000;
        Check(Cmd(combat,actor,CommandKind.AttackMove,pos:new V2(30,-23)),
            "attack-move command accepted");
        int attacks=0;
        for(int i=0;i<260 && attacks<2;i++)
        {
            combat.Tick(GameWorld.FixedStep);
            foreach(var ev in combat.DrainEvents())
                if(ev.Kind==EventKind.Attack && ev.ActorId==actor.Id &&
                    ev.TargetId==victim.Id)attacks++;
        }
        Check(attacks>=2 && actor.AttackMoveActive,
            "attack-move acquires and repeats attacks");
        Check(Cmd(combat,actor,CommandKind.Stop) && !actor.AttackMoveActive &&
            actor.AttackTargetId==0,"stop clears attack-move");

        var anti=new GameWorld("GKU_KID",ArenaLayout.Training);
        var buyer=anti.Player;
        buyer.Zeni=5000;
        Check(Cmd(anti,buyer,CommandKind.Buy,"ANTI_HEAL_BLADE") &&
            Math.Abs(buyer.AntiHealOnHit-.35f)<.001f,
            "anti-heal item grants debuff on damage");
        EntityState affected=null;
        foreach(var e in anti.Entities)
            if(e.Kind==EntityKind.Hero && e.Team==Team.Red)affected=e;
        Check(affected!=null,"enemy for anti-heal exists");
        affected.Position=affected.Destination=new V2(30,-36);
        affected.MoveSpeed=affected.PhysicalAttack=affected.AttackRange=0;
        affected.Hp=250;
        Check(anti.DealDamage(buyer,affected,40,false,true) &&
            Math.Abs(affected.HealingReductionRate-.35f)<.001f &&
            affected.HealingReductionSeconds==3f,"anti-heal debuff applied for 3s");
        for(int i=0;i<115;i++)anti.Tick(GameWorld.FixedStep);
        Check(affected.HealingReductionSeconds==0f &&
            affected.HealingReductionRate==0f,"anti-heal debuff expires");

        var towers=new GameWorld("GKU_SUPER",ArenaLayout.Training);
        EntityState redTower=null, firstMinion=null, closerMinion=null;
        foreach(var entity in towers.Entities)
        {
            if(entity.Kind==EntityKind.Tower && entity.Team==Team.Red && entity.Lane==1)
                redTower=entity;
            if(entity.Kind==EntityKind.Minion && entity.Team==Team.Blue && entity.Lane==1)
            {
                if(firstMinion==null) firstMinion=entity;
                else if(closerMinion==null) closerMinion=entity;
            }
        }
        Check(redTower!=null && firstMinion!=null && closerMinion!=null,
            "tower target retention fixtures exist");
        firstMinion.Position=firstMinion.Destination=new V2(0,12);
        closerMinion.Position=closerMinion.Destination=new V2(10,0);
        towers.Tick(GameWorld.FixedStep);
        Check(redTower.AttackTargetId==firstMinion.Id,
            "tower initially acquires minion in range");
        closerMinion.Position=closerMinion.Destination=new V2(0,16);
        towers.Tick(GameWorld.FixedStep);
        Check(redTower.AttackTargetId==firstMinion.Id,
            "tower does not retarget when closer minion arrives");

        var classic=new GameWorld("GKU_KID");var map=classic.Layout;
        Check(map==ArenaLayout.Classic&&map.HalfWidth==72&&map.HalfHeight==72,"actual default diagonal map bounds");
        var towerCount=0;EntityState routeMinion=null;
        foreach(var entity in classic.Entities){if(entity.Kind==EntityKind.Tower)towerCount++;if(entity.Kind==EntityKind.Minion&&entity.Team==Team.Blue&&entity.Lane==0)routeMinion=entity;}
        Check(towerCount==18&&map.Core(Team.Blue).X<0&&map.Core(Team.Red).X>0,"corner bases and eighteen towers");
        Check(Cmd(classic,classic.Player,CommandKind.Buy,"KI_CELL"),"classic spawn fountain purchase");
        Check(map.At(0,.5f).X<0&&map.At(0,.5f).Y>0&&map.At(2,.5f).X>0&&map.At(2,.5f).Y<0,"distinct top and bottom routes");
        var progress=map.Progress(0,routeMinion.Position);
        for(var tick=0;tick<600;tick++)classic.Tick(GameWorld.FixedStep);
        Check(map.Progress(0,routeMinion.Position)>progress+.15f&&map.RoadDistance(routeMinion.Position)<0,"minion follows side lane without cutting jungle");
        classic.Player.Position=classic.Player.Destination=new V2(5,-30);
        Cmd(classic,classic.Player,CommandKind.Move,pos:new V2(13.2f,-17.8f));
        for(var tick=0;tick<30;tick++)classic.Tick(GameWorld.FixedStep);
        Check(Math.Abs((classic.Player.Position.X-5)/(classic.Player.Position.Y+30)-8.2f/12.2f)<.002f,"movement supports arbitrary angles");
        Cmd(classic,classic.Player,CommandKind.Move,pos:new V2(1000,-1000));
        Check(classic.Player.Destination.X==72&&classic.Player.Destination.Y==-72,"classic map command clamp");
        Console.WriteLine("DOMAIN SMOKE PASSED | checks=" + passed);
    }
}
