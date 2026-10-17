// Original procedural audio: ambient soundtrack + energy blasts + impacts + feedback.
// No copied Dragon Ball/League audio and no network/download requirement.
// Never influences simulated combat, XP or input; all clips are generated once.
using System;
using System.Collections.Generic;
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    public sealed class BattleSoundscape : MonoBehaviour
    {
        public float musicVolume = .12f;
        public float soundVolume = .26f;
        private const int SampleRate = 22050;
        private AudioSource music, effects;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private float nextAttack, nextHit, nextCharge;
        private bool muted;
        private AudioClip licensedBlast, licensedHit, licensedCharging;
        // Layer vetted audio assets already bundled with the game with the
        // original synthesized foundation, instead of removing good samples.
        public void SetExistingClips(AudioClip blast,AudioClip hit,AudioClip charge)
        {
            licensedBlast=blast;
            licensedHit=hit;
            licensedCharging=charge;
        }
        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.spatialBlend = 0;
            music.priority = 180;
            music.clip = Create("Ambient", 48f, 0);
            music.volume = musicVolume;
            music.Play();
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.loop = false;
            effects.spatialBlend = 0;
            effects.priority = 70;
            Create("Blast", .27f, 1);
            Create("Impact", .24f, 2);
            Create("Purchase", .55f, 3);
            Create("Charge", .72f, 4);
            Create("Transform", 1.4f, 5);
            Create("Death", 1f, 6);
            Create("Victory", 2.0f, 7);
            Create("Warning", .30f, 8);
        }
        private static float Sin(float phase) { return Mathf.Sin(phase * Mathf.PI * 2f); }
        static readonly float[] roots={130.81f,103.83f,155.56f,116.54f};
        static readonly float[] thirds={155.56f,130.81f,196f,146.83f};
        static readonly float[] fifths={196f,155.56f,233.08f,174.61f};
        static readonly float[] melody={392f,0,311.13f,261.63f,0,233.08f,261.63f,0,311.13f,0,392f,466.16f,392f,0,311.13f,0};
        private static float Ambient(float t)
        {
            // Original 16-bar cue at 80 BPM; soft pad, bass and sparse plucked melody.
            // Crossfade harmony rather than restarting the old eight-second drone.
            var bar=Mathf.FloorToInt(t/3f);var chord=bar%4;var previous=(chord+3)%4;
            var blend=Mathf.SmoothStep(0,1,(t%3f)/.25f);
            var pad=.25f*Mathf.Lerp(Sin(t*roots[previous]),Sin(t*roots[chord]),blend)+
                .17f*Mathf.Lerp(Sin(t*thirds[previous]),Sin(t*thirds[chord]),blend)+
                .12f*Mathf.Lerp(Sin(t*fifths[previous]),Sin(t*fifths[chord]),blend);
            var beat=t/.75f;var local=t%.75f;var note=melody[Mathf.FloorToInt(beat)%melody.Length];
            var pluck=note<=0?0:.13f*Mathf.Exp(-local*7f)*(Sin(t*note)+.18f*Sin(t*note*2));
            var bass=.10f*Sin(t*roots[chord]*.5f)*Mathf.Exp(-local*3f);
            return pad+pluck+bass;
        }
        private AudioClip Create(string name, float duration, int style)
        {
            var samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            var phase = 0f;
            for(var i=0; i<samples; i++)
            {
                var t=i/(float)SampleRate;
                var u=i/(float)(samples-1);
                float f=0f, raw=0f, env=0f;
                switch(style)
                {
                    case 0:
                        env=Mathf.Min(1f,Mathf.Min(t/.08f,(duration-t)/.08f));
                        raw=Ambient(t);
                        break;
                    case 1: // Ki shot with accelerating edge and short bass tail.
                        f=420f*Mathf.Pow(1f-u,1.15f)+80f;
                        env=Mathf.Exp(-u*8f)*(1f-Mathf.Exp(-u*38f));
                        raw=.68f*Sin(phase)+.14f*Sin(phase*2.1f)+.26f*Sin(t*90f)*(1f-u);
                        break;
                    case 2: // Impact, low thud and a short crunchy transient.
                        f=100f-58f*u; env=Mathf.Exp(-u*12f);
                        raw=.74f*Sin(phase)+.24f*Sin(t*380f)*Mathf.Exp(-u*27f);
                        raw+=.15f*Sin(t*1700f+Sin(t*43f))*(1f-u);
                        break;
                    case 3: // Purchase/confirm: clean original ascending double bell.
                        env=Mathf.Exp(-u*5f);
                        raw=.65f*Sin(t*783.99f)+.37f*Sin(t*1046.50f);
                        raw+=.12f*Sin(t*1567.98f);
                        break;
                    case 4: // Ki charging: soft accelerating oscillator.
                        f=150f+250f*u;env=(1f-Mathf.Exp(-u*25f))*Mathf.Pow(1f-u,.95f);
                        raw=.56f*Sin(phase)+.23f*Sin(phase*2f)+.12f*Sin(t*12f)*Sin(phase);
                        break;
                    case 5: // Transformation sweep and chord, with space for an animation.
                        f=180f+670f*u*u;env=Mathf.Sin(Mathf.PI*u)*.8f;
                        raw=.37f*Sin(phase)+.17f*Sin(phase*1.97f);
                        raw+=.22f*Sin(t*329.63f)+.15f*Sin(t*493.88f);
                        break;
                    case 6: // Death descending signal.
                        f=230f-145f*u;env=Mathf.Pow(1f-u,1.4f);
                        raw=.65f*Sin(phase)+.22f*Sin(t*97.99f);
                        break;
                    case 7: // Victory arpeggio and held major chord.
                        env=Mathf.Min(1f,t/.06f)*Mathf.Min(1f,(duration-t)/.28f);
                        raw=.22f*Sin(t*261.63f)+.22f*Sin(t*329.63f)+
                            .19f*Sin(t*392f)+.17f*Sin(t*523.25f);
                        raw+=.24f*Sin(t*(u<.25f?392f:u<.5f?493.88f:523.25f));
                        break;
                    default: // Tower warning / alert
                        f=380f+85f*Sin(t*13f);env=Mathf.Exp(-u*5f);
                        raw=.55f*Sin(phase)+.16f*Sin(phase*2f);
                        break;
                }
                phase+=f/SampleRate;
                // Smooth boundaries prevent clicks and preserve headroom.
                var fade=Mathf.Min(1f,Mathf.Min(i/(SampleRate*.008f),
                    (samples-1-i)/(SampleRate*.012f)));
                data[i]=Mathf.Clamp(raw*env*fade*.55f,-.65f,.65f);
            }
            var clip=AudioClip.Create("Original "+name,samples,1,SampleRate,false);
            clip.SetData(data,0);
            clips[name]=clip;
            return clip;
        }
        private void Play(string key, float gain)
        {
            if(muted || effects==null || !clips.TryGetValue(key,out var clip))return;
            effects.PlayOneShot(clip,Mathf.Clamp01(soundVolume*gain));
            var sample=key=="Blast"?licensedBlast:
                key=="Impact"?licensedHit:key=="Charge"?licensedCharging:null;
            if(sample!=null)
                effects.PlayOneShot(sample,Mathf.Clamp01(soundVolume*gain*.62f));
        }
        public void Confirm() { Play("Purchase",.75f); }
        public void React(GameEvent ev, GameWorld world)
        {
            if(world==null || ev==null)return;
            bool relevant=ev.ActorId==world.PlayerId || ev.TargetId==world.PlayerId ||
                ev.Kind==EventKind.Victory;
            if(!relevant)return;
            switch(ev.Kind)
            {
                case EventKind.Purchase:
                case EventKind.SkillUp: Confirm(); break;
                case EventKind.Attack:
                    if(Time.time>=nextAttack){Play("Blast",.67f);nextAttack=Time.time+.10f;}
                    break;
                case EventKind.Ability: Play("Blast",1.0f);break;
                case EventKind.Damage:
                    if(Time.time>=nextHit){Play("Impact",.6f);nextHit=Time.time+.12f;}
                    break;
                case EventKind.Transform: Play("Transform",.85f);break;
                case EventKind.Charge:
                    if(Time.time>=nextCharge){Play("Charge",.28f);nextCharge=Time.time+.50f;}
                    break;
                case EventKind.Death: Play("Death",.9f);break;
                case EventKind.Victory: Play("Victory",1f);break;
                case EventKind.Warning: Play("Warning",.75f);break;
            }
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.M))
            {
                muted=!muted;
                if(music!=null)music.mute=muted;
                if(effects!=null)
                {
                    effects.mute=muted;
                    if(muted)effects.Stop();
                }
            }
            if(music!=null)music.volume=muted?0:musicVolume;
        }
        private void OnDestroy()
        {
            foreach(var clip in clips.Values) if(clip!=null)Destroy(clip);
            clips.Clear();
        }
    }
}
