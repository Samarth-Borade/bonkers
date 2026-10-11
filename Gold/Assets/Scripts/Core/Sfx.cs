using System.Collections.Generic;
using UnityEngine;

// Tiny sound effects made from math (no audio files). Sfx.Play(Sfx.Bonk);
public static class Sfx
{
    static AudioSource source;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static AudioClip Bonk => Get("bonk", () => Sweep("bonk", 750, 110, 0.25f, 0.55f, 0.05f, 0.35f));
    public static AudioClip Jump => Get("jump", () => Sweep("jump", 280, 620, 0.12f, 0.25f, 0f, 0.6f));
    public static AudioClip Swim => Get("swim", () => Sweep("swim", 180, 260, 0.15f, 0.2f, 0.25f, 0f));
    public static AudioClip Pickup => Get("pickup", () => Notes("pickup", new[] { 660f, 990f }, 0.07f, 0.3f));
    public static AudioClip Use => Get("use", () => Sweep("use", 900, 300, 0.12f, 0.25f, 0.3f, 0.2f));
    public static AudioClip Die => Get("die", () => Sweep("die", 420, 50, 0.4f, 0.4f, 0.35f, 0.5f));
    public static AudioClip Shove => Get("shove", () => Sweep("shove", 200, 90, 0.08f, 0.3f, 0.6f, 0f));
    public static AudioClip Block => Get("block", () => Notes("block", new[] { 1200f, 800f }, 0.06f, 0.3f));
    public static AudioClip Teleport => Get("teleport", () => Sweep("teleport", 300, 1800, 0.3f, 0.25f, 0f, 0.4f));
    public static AudioClip Beep => Get("beep", () => Notes("beep", new[] { 440f }, 0.15f, 0.3f));
    public static AudioClip Go => Get("go", () => Notes("go", new[] { 880f }, 0.35f, 0.35f));
    public static AudioClip Win => Get("win", () => Notes("win", new[] { 523f, 659f, 784f, 1047f, 784f, 1047f }, 0.11f, 0.35f));

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (!source)
        {
            var go = new GameObject("Sfx");
            source = go.AddComponent<AudioSource>();
        }
        source.PlayOneShot(clip, volume);
    }

    static AudioClip Get(string key, System.Func<AudioClip> make)
    {
        if (!cache.TryGetValue(key, out var clip) || !clip) cache[key] = clip = make();
        return clip;
    }

    const int Rate = 44100;

    // A pitch slide from f0 to f1. noise adds hiss, square adds a buzzy retro edge.
    static AudioClip Sweep(string name, float f0, float f1, float duration, float volume, float noise, float square)
    {
        int n = (int)(Rate * duration);
        var data = new float[n];
        var rng = new System.Random(name.GetHashCode());
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            phase += Mathf.Lerp(f0, f1, t) / Rate;
            float s = Mathf.Sin((float)(phase * 2 * Mathf.PI));
            float v = Mathf.Lerp(s, s >= 0 ? 1f : -1f, square) + (float)(rng.NextDouble() * 2 - 1) * noise;
            float env = Mathf.Min(1f, i / (Rate * 0.004f)) * (1 - t) * (1 - t);
            data[i] = v * env * volume;
        }
        return Clip(name, data);
    }

    static AudioClip Notes(string name, float[] freqs, float noteLength, float volume)
    {
        int per = (int)(Rate * noteLength);
        var data = new float[per * freqs.Length];
        for (int k = 0; k < freqs.Length; k++)
            for (int i = 0; i < per; i++)
            {
                float t = i / (float)per;
                float s = Mathf.Sin(2 * Mathf.PI * freqs[k] * i / Rate);
                data[k * per + i] = (s >= 0 ? 0.6f : -0.6f) * volume * (1 - t) * Mathf.Min(1f, i / (Rate * 0.004f));
            }
        return Clip(name, data);
    }

    static AudioClip Clip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return clip;
    }
}
