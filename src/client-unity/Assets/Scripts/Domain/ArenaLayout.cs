// Original three-lane diagonal arena data. Combat/progression values are not part of map geometry.
using System;

namespace DragonMoba.Domain
{
    public sealed class ArenaLayout
    {
        public static readonly ArenaLayout Training = new ArenaLayout(false);
        public static readonly ArenaLayout Classic = new ArenaLayout(true);
        public readonly bool Diagonal;
        public readonly float HalfWidth, HalfHeight;
        public readonly V2[][] Lanes;
        public int TowerTiers { get { return Diagonal ? 3 : 1; } }

        ArenaLayout(bool diagonal)
        {
            Diagonal = diagonal; HalfWidth = diagonal ? 72 : 32; HalfHeight = diagonal ? 72 : 41;
            if (diagonal)
            {
                Lanes = new[] {
                    new[]{new V2(-58,-58),new V2(-63,-39),new V2(-63,32),new V2(-56,49),new V2(-40,61),new V2(36,63),new V2(58,58)},
                    new[]{new V2(-58,-58),new V2(-32,-32),new V2(0,0),new V2(32,32),new V2(58,58)},
                    new[]{new V2(-58,-58),new V2(-39,-63),new V2(32,-63),new V2(49,-56),new V2(61,-40),new V2(63,36),new V2(58,58)}
                };
            }
            else Lanes = new[] {new[]{new V2(-17,-32),new V2(-17,32)},new[]{new V2(0,-32),new V2(0,32)},new[]{new V2(17,-32),new V2(17,32)}};
        }
        public V2 Base(Team team) { var sign=team==Team.Blue?-1:1;return Diagonal?new V2(sign*64,sign*64):new V2(0,sign*34); }
        public V2 Core(Team team) { var sign=team==Team.Blue?-1:1;return Diagonal?new V2(sign*59,sign*59):new V2(0,sign*38); }
        public V2 HeroSpawn(Team team) { var sign=team==Team.Blue?-1:1;return Diagonal?new V2(sign*61,sign*61):new V2(0,sign*29); }
        public V2 Tower(Team team,int lane,int tier)
        {
            if(!Diagonal)return new V2((lane-1)*17,team==Team.Blue?-19:19);
            var progress=new[]{.40f,.24f,.10f}[tier];
            return At(lane,team==Team.Blue?progress:1-progress);
        }
        public V2 MinionSpawn(Team team,int lane,int index)
        {
            if(!Diagonal)return new V2((lane-1)*17+(index-1)*1.3f,team==Team.Blue?-32:32);
            var progress=team==Team.Blue?.015f:.985f;
            var point=At(lane,progress);var next=At(lane,progress+(team==Team.Blue?.02f:-.02f));
            var dx=next.X-point.X;var dy=next.Y-point.Y;var length=(float)Math.Sqrt(dx*dx+dy*dy);
            var offset=(index-1)*1.15f;
            return new V2(point.X-dy/length*offset,point.Y+dx/length*offset);
        }
        public V2 Clamp(V2 p) { return new V2(Math.Max(-HalfWidth,Math.Min(HalfWidth,p.X)),Math.Max(-HalfHeight,Math.Min(HalfHeight,p.Y))); }
        public V2 At(int lane,float progress)
        {
            var path=Lanes[lane];var remaining=Length(lane)*Math.Max(0,Math.Min(1,progress));
            for(var i=0;i<path.Length-1;i++)
            {
                var segment=V2.Distance(path[i],path[i+1]);
                if(remaining<=segment)return V2.MoveTowards(path[i],path[i+1],remaining);
                remaining-=segment;
            }
            return path[path.Length-1];
        }
        public float Length(int lane)
        {var length=0f;var path=Lanes[lane];for(var i=0;i<path.Length-1;i++)length+=V2.Distance(path[i],path[i+1]);return length;}
        public float Progress(int lane,V2 position)
        {
            var path=Lanes[lane];var nearest=float.MaxValue;var traveled=0f;var result=0f;
            for(var i=0;i<path.Length-1;i++)
            {
                var a=path[i];var b=path[i+1];var dx=b.X-a.X;var dy=b.Y-a.Y;var squared=dx*dx+dy*dy;
                var t=Math.Max(0,Math.Min(1,((position.X-a.X)*dx+(position.Y-a.Y)*dy)/squared));
                var point=new V2(a.X+dx*t,a.Y+dy*t);var distance=V2.Distance(position,point);var segment=(float)Math.Sqrt(squared);
                if(distance<nearest){nearest=distance;result=traveled+t*segment;}
                traveled+=segment;
            }
            return result/Math.Max(.001f,traveled);
        }
        public V2 RouteDestination(EntityState unit,EntityState structure)
        {
            if(!Diagonal)return structure.Position;
            var progress=Progress(unit.Lane,unit.Position);var end=Progress(unit.Lane,structure.Position);
            var step=3.5f/Length(unit.Lane);
            if(Math.Abs(progress-end)<step)return structure.Position;
            return At(unit.Lane,unit.Team==Team.Blue?Math.Min(end,progress+step):Math.Max(end,progress-step));
        }
        public float RoadDistance(V2 point)
        {
            var squared=float.MaxValue;
            foreach(var path in Lanes)for(var i=0;i<path.Length-1;i++)
            {
                var dx=path[i+1].X-path[i].X;var dy=path[i+1].Y-path[i].Y;
                var t=Math.Max(0,Math.Min(1,((point.X-path[i].X)*dx+(point.Y-path[i].Y)*dy)/(dx*dx+dy*dy)));
                var x=point.X-path[i].X-t*dx;var y=point.Y-path[i].Y-t*dy;
                squared=Math.Min(squared,x*x+y*y);
            }
            var result=(float)Math.Sqrt(squared)-3.4f;
            result=Math.Min(result,V2.Distance(point,Base(Team.Blue))-9.5f);
            return Math.Min(result,V2.Distance(point,Base(Team.Red))-9.5f);
        }
    }
}
