#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    // Opt-in presentation measurements only: never submits commands or changes domain state.
    public sealed class VisualAudioValidationProbe : MonoBehaviour
    {
        readonly List<float> frameMs = new List<float>(18000);
        string directory, stage = "startup";
        float nextSnapshot, recordingEnd;
        ValidationAudioCapture audioCapture;
        bool overview;
        float savedZoom;
        static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Application.isBatchMode || !Environment.GetCommandLineArgs().Contains("-art002-validation")) return;
            var probe = new GameObject("ART-002 opt-in presentation measurements").AddComponent<VisualAudioValidationProbe>();
            probe.directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/ART002"));
            Directory.CreateDirectory(probe.directory);
        }

        void LateUpdate()
        {
            if (directory == null) return;
            var listener = FindAnyObjectByType<AudioListener>();
            if (listener != null && audioCapture == null)
                audioCapture = listener.gameObject.AddComponent<ValidationAudioCapture>();
            if (frameMs.Count < 18000) frameMs.Add(Time.unscaledDeltaTime * 1000f);
            var game = FindAnyObjectByType<PrototypeGame>();
            var world = game == null ? null : (GameWorld)typeof(PrototypeGame).GetField("world", Flags).GetValue(game);
            var commandPath = Path.Combine(directory, "command.txt");
            if (File.Exists(commandPath))
            {
                var command = File.ReadAllText(commandPath).Trim();
                File.Delete(commandPath);
                var fields = command.Split(':');
                if (fields[0] == "mark" && fields.Length == 2 && SafeName(fields[1]))
                { stage = fields[1]; frameMs.Clear(); }
                else if (fields[0] == "record" && fields.Length == 3 && SafeName(fields[1]) && audioCapture != null)
                {
                    var seconds = Mathf.Clamp(float.Parse(fields[2], CultureInfo.InvariantCulture), 1, 30);
                    audioCapture.Begin(Path.Combine(directory, fields[1] + ".wav"), seconds, AudioSettings.outputSampleRate);
                    recordingEnd = Time.realtimeSinceStartup + seconds + .2f;
                }
                else if (fields[0] == "sound" && fields.Length == 2 && world != null && game != null &&
                    Enum.TryParse(fields[1], out EventKind kind))
                {
                    // Audition the real presentation route, without simulating a victory/transform.
                    game.GetComponent<BattleSoundscape>().React(
                        new GameEvent(kind, world.PlayerId, world.Player.Position, "ART-002 presentation audition"), world);
                }
                else if (fields[0] == "focus" && fields.Length == 4 && Camera.main != null)
                {
                    var camera = Camera.main.GetComponent<ArenaCameraController>();
                    if (camera != null)
                    {
                        var zoom = float.Parse(fields[3], CultureInfo.InvariantCulture);
                        camera.Zoom(Camera.main.orthographicSize - zoom);
                        camera.FocusAt(new Vector3(float.Parse(fields[1], CultureInfo.InvariantCulture), 0,
                            float.Parse(fields[2], CultureInfo.InvariantCulture)), true);
                    }
                }
                else if(fields[0]=="overview"&&fields.Length==2&&Camera.main!=null)
                {
                    if(fields[1]=="1"&&!overview){savedZoom=Camera.main.orthographicSize;overview=true;}
                    else if(fields[1]=="0"){overview=false;Camera.main.orthographicSize=savedZoom;RenderSettings.fog=true;}
                }
                Debug.Log("ART002 PRESENTATION COMMAND | " + command);
                nextSnapshot = 0;
            }
            if (recordingEnd > 0 && Time.realtimeSinceStartup >= recordingEnd && audioCapture != null)
            { audioCapture.Finish(); recordingEnd = 0; }
            if(overview&&Camera.main!=null)
            {
                Camera.main.orthographic=true;Camera.main.orthographicSize=81;
                Camera.main.transform.SetPositionAndRotation(new Vector3(0,180,0),Quaternion.Euler(90,0,0));
                RenderSettings.fog=false;
            }
            if (Time.realtimeSinceStartup < nextSnapshot) return;
            nextSnapshot = Time.realtimeSinceStartup + 1;
            var sorted = frameMs.OrderBy(v => v).ToArray();
            var terrain = GameObject.Find("OrganicTerrain");
            var material = terrain == null ? null : terrain.GetComponent<Renderer>().sharedMaterial;
            var texture = material == null ? null : material.mainTexture;
            var sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            var result = new Snapshot {
                stage = stage, frames = sorted.Length,
                seconds = sorted.Sum(v => (double)v) / 1000,
                averageFps = sorted.Length == 0 ? 0 : (float)(sorted.Length * 1000 / sorted.Sum(v => (double)v)),
                medianFrameMs = Percentile(sorted, .5f), p95FrameMs = Percentile(sorted, .95f), p99FrameMs = Percentile(sorted, .99f),
                minions = world == null ? 0 : world.Entities.Count(e => e.Kind == EntityKind.Minion && e.Alive),
                renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length,
                width = Screen.width, height = Screen.height,
                groundShader = material == null ? "missing" : material.shader.name,
                groundTexture = texture == null ? "missing" : texture.name,
                textureWidth = texture == null ? 0 : texture.width,
                textureEnabled = material != null && material.HasProperty("_UseTexture") ? material.GetFloat("_UseTexture") : -1,
                galleryHeights = GalleryHeights(),
                audioSources = sources.Length, playingSources = sources.Count(s => s.isPlaying), mutedSources = sources.Count(s => s.mute),
                audioSampleRate = AudioSettings.outputSampleRate,
                cameraPerspective=Camera.main!=null&&!Camera.main.orthographic,
                mapHalfWidth=world==null?0:world.Layout.HalfWidth,
                mapHalfHeight=world==null?0:world.Layout.HalfHeight,
                lastAudio = audioCapture == null ? null : audioCapture.LastResult
            };
            File.WriteAllText(Path.Combine(directory, "state.json"), JsonUtility.ToJson(result, true));
            File.WriteAllText(Path.Combine(directory, stage + "-metrics.json"), JsonUtility.ToJson(result, true));
        }
        static bool SafeName(string value) { return Regex.IsMatch(value, "^[a-z0-9-]+$"); }
        static float Percentile(float[] values, float p)
        { return values.Length == 0 ? 0 : values[Mathf.Min(values.Length - 1, Mathf.FloorToInt((values.Length - 1) * p))]; }
        static float[] GalleryHeights()
        {
            var values = new float[3];
            if (Camera.main == null) return values;
            for (var i = 0; i < values.Length; i++)
            {
                var anchor = GameObject.Find("Hero showcase " + i);
                if (anchor == null) continue;
                var renderers = anchor.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                float low = float.MaxValue, high = float.MinValue;
                for (var corner = 0; corner < 8; corner++)
                {
                    var offset = Vector3.Scale(bounds.extents, new Vector3((corner & 1) == 0 ? -1 : 1,
                        (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    var y = Camera.main.WorldToScreenPoint(bounds.center + offset).y;
                    low = Mathf.Min(low, y); high = Mathf.Max(high, y);
                }
                values[i] = high - low;
            }
            return values;
        }
        [Serializable] class Snapshot
        {
            public string stage, groundShader, groundTexture;
            public int frames, minions, renderers, width, height, textureWidth, audioSources, playingSources, mutedSources, audioSampleRate;
            public double seconds;
            public float averageFps, medianFrameMs, p95FrameMs, p99FrameMs, textureEnabled;
            public float[] galleryHeights;
            public bool cameraPerspective;
            public float mapHalfWidth,mapHalfHeight;
            public ValidationAudioCapture.Result lastAudio;
        }
    }

    // Listener-side diagnostic tap: copies the actual mixed signal, leaving output untouched.
    public sealed class ValidationAudioCapture : MonoBehaviour
    {
        readonly object gate = new object();
        float[] samples;
        int count, sampleRate;
        string output;
        bool recording;
        public Result LastResult { get; private set; }
        public void Begin(string path, float seconds, int rate)
        {
            lock (gate) { output = path; sampleRate = rate; samples = new float[Mathf.CeilToInt(seconds * rate)]; count = 0; recording = true; }
        }
        void OnAudioFilterRead(float[] data, int channels)
        {
            lock (gate)
            {
                if (!recording || samples == null) return;
                for (var i = 0; i < data.Length && count < samples.Length; i += channels)
                {
                    float mono = 0;
                    for (var channel = 0; channel < channels; channel++) mono += data[i + channel];
                    samples[count++] = mono / channels;
                }
            }
        }
        public void Finish()
        {
            float[] captured; int length;
            lock (gate) { recording = false; captured = samples; length = count; samples = null; }
            if (captured == null) return;
            double squares = 0; float peak = 0; int clipped = 0;
            using (var writer = new BinaryWriter(File.Create(output)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(sampleRate); writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(length * 2);
                for (var i = 0; i < length; i++)
                {
                    var value = captured[i]; squares += value * (double)value;
                    peak = Mathf.Max(peak, Mathf.Abs(value)); if (Mathf.Abs(value) >= .999f) clipped++;
                    writer.Write((short)(Mathf.Clamp(value, -1, 1) * 32767));
                }
            }
            LastResult = new Result { file = Path.GetFileName(output), samples = length, seconds = length / (float)sampleRate,
                peak = peak, rms = length == 0 ? 0 : (float)Math.Sqrt(squares / length), clippedSamples = clipped };
            Debug.Log("ART002 MIX CAPTURE | " + JsonUtility.ToJson(LastResult));
        }
        [Serializable] public class Result
        { public string file; public int samples, clippedSamples; public float seconds, peak, rms; }
    }
}
#endif
