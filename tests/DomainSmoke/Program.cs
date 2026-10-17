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
        var w = new GameWorld("GKU_KID");
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

        var apeWorld=new GameWorld("GKU_KID");
        var kid=apeWorld.Player;
        apeWorld.GrantExperience(kid,99999);
        Check(kid.Level==18 && kid.FormId=="kid_oozaru" &&
            apeWorld.BodyRadius(kid)>.9 && kid.MoveSpeed<6,"Oozaru is larger and slower");

        var combat=new GameWorld("GKU_KID");
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

        Console.WriteLine("DOMAIN SMOKE PASSED | checks=" + passed);
    }
}
