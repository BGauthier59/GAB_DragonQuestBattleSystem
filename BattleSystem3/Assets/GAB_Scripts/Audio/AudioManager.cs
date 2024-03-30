using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoSingleton<AudioManager>
{
    
    public Sound[] soundArray;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        foreach(Sound s in soundArray)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(soundArray, sound => sound.name == name);
        s.source.Play();
    }
    public void Stop(string name)
    {
        Sound s = Array.Find(soundArray, sound => sound.name == name);
        s.source.Stop();
    }
}
