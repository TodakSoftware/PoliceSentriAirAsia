﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;     //Store the name of our music/effect
    public AudioMixerGroup audioMixerGroup; //Reference to audio mixer group
    public AudioClip clip;  //Store the actual music/effect
    [Range(0f, 1f)]         //limit the range in the Unity editor
    public float volume;    //Store our volume
    [Range(0.1f, 3f)]       //Limit the Range again
    public float pitch;     // set the picth for our music/effect
    [HideInInspector]       //Hide this variable from the Editor
    public AudioSource source;// the source that will play the sound
    public bool loop = false;// should this sound loop
}


[CreateAssetMenu(fileName = "New Audio Preset", menuName = "Database/Audio/Audio Preset")]
public class SO_AudioPreset : ScriptableObject
{
    
    public Sound[] sounds;      // store all our sounds
}
