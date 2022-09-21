using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SoundFX : MonoBehaviourPunCallbacks
{
    public List<string> footstepsLists = new List<string>();
    public List<string> soundLists = new List<string>();
    public bool playOnAwake;

    [Header("Play Single Sound")]
    public int soundIndex;

    [Header("Play All Sound")]
    public bool playAll;

    [Header("Play Random List")]
    public bool enableRandom;
    public int startIndex, endIndex;

    void Awake(){
        // Predefined Footsteps

        if(playOnAwake){
            if(playAll){
                EffectAllSFX();
            }else{
                if(enableRandom){
                    EffectSFXRandom();
                }else{
                    EffectSFX(soundIndex);
                }
            }
        }
        
    }

    public void Footstep(){
        if(footstepsLists.Count > 0 && AudioManager.instance != null){
            if(photonView.IsMine){
                AudioManager.instance.PlaySound(footstepsLists[Random.Range(0,footstepsLists.Count)]);
            }
        }else{
            //print("AudioSource Not Found. Add Prefabs 'AddSceneManager' in the scene");
        }
    } // end footstep

    public void EffectSFX(int indexSound){
        if(soundLists.Count > 0 && AudioManager.instance != null){
            AudioManager.instance.PlaySound(soundLists[indexSound]);
        }else{
            //print("AudioSource Not Found. Add Prefabs 'AddSceneManager' in the scene");
        }
    }

    public void EffectAllSFX(){
        if(soundLists.Count > 0 && AudioManager.instance != null){
            foreach(string s in soundLists){
                AudioManager.instance.PlaySound(s);
            }
        }else{
            //print("AudioSource Not Found. Add Prefabs 'AddSceneManager' in the scene");
        }
    }

    public void EffectSFXRandom(){
        if(soundLists.Count > 0 && AudioManager.instance != null){
            AudioManager.instance.PlaySound(soundLists[Random.Range(startIndex,endIndex + 1)]);
        }else{
            //print("AudioSource Not Found. Add Prefabs 'AddSceneManager' in the scene");
        }
    }
}
