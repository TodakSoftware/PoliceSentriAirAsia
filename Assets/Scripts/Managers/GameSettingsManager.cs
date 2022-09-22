using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager instance;
    public bool mute;

    public AudioMixer audioMix;
    [Range(0.0001f, 1)] public float sfxVolume = 0f, musicVolume = 0f;
    // Language

    void Awake(){
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable(){
        if(PlayerPrefs.HasKey("Mute")){
            if(PlayerPrefs.GetInt("Mute") == 0){
                AudioListener.volume = 0;
                mute = true;
            }else{
                AudioListener.volume = 1;
                mute = false;
            }
        }else{
            PlayerPrefs.SetInt("Mute", 1);
            AudioListener.volume = 1;
            mute = false;
        }

        if(PlayerPrefs.HasKey("SFXVol")){
            if(PlayerPrefs.GetFloat("SFXVol") == 0){
                sfxVolume = 0.0001f;
            }else{
                sfxVolume = PlayerPrefs.GetFloat("SFXVol");
            }
            audioMix.SetFloat("SFXVol", Mathf.Log10(sfxVolume) * 20);
        }else{
            PlayerPrefs.SetFloat("SFXVol", 1f);
            sfxVolume = 1f;
            audioMix.SetFloat("SFXVol", Mathf.Log10(sfxVolume) * 20);
        }

        if(PlayerPrefs.HasKey("BGMVol")){
            if(PlayerPrefs.GetFloat("BGMVol") == 0){
                musicVolume = 0.0001f;
            }else{
                musicVolume = PlayerPrefs.GetFloat("BGMVol");
            }
            audioMix.SetFloat("BGMVol", Mathf.Log10(musicVolume) * 20);
        }else{
            PlayerPrefs.SetFloat("BGMVol", 1f);
            musicVolume = 1;
            audioMix.SetFloat("BGMVol", Mathf.Log10(musicVolume) * 20);
        }
    }

    public void Mute(){
        if(mute){
            mute = false;

            PlayerPrefs.SetInt("Mute", 1);
            AudioListener.volume = 1;
            
        }else{
            mute = true;

            PlayerPrefs.SetInt("Mute", 0);
            AudioListener.volume = 0;
        }

        PlayerPrefs.Save();
    }

    public void SetSfxLvl(float sfxLvl){
        audioMix.SetFloat("SFXVol", Mathf.Log10(sfxLvl) * 20);
        PlayerPrefs.SetFloat("SFXVol", sfxLvl);
        PlayerPrefs.Save();
    }

    public void SetBgmLvl(float bgmLvl){
        audioMix.SetFloat("BGMVol", Mathf.Log10(bgmLvl) * 20);
        PlayerPrefs.SetFloat("BGMVol", bgmLvl);
        PlayerPrefs.Save();
    }

    public void ClearVolume(){
        audioMix.ClearFloat("BGMVol");
        audioMix.ClearFloat("SFXVol");
        PlayerPrefs.SetFloat("SFXVol", 0.0001f);
        PlayerPrefs.SetFloat("BGMVol", 0.0001f);
    }

    

    public void RetriggerAudioSetting(){
        if(PlayerPrefs.HasKey("SFXVol")){
            audioMix.SetFloat("SFXVol",  Mathf.Log10(PlayerPrefs.GetFloat("SFXVol")) * 20);
        }else{
            PlayerPrefs.SetFloat("SFXVol", 0.0001f);
        }

        if(PlayerPrefs.HasKey("BGMVol")){
            audioMix.SetFloat("BGMVol", Mathf.Log10(PlayerPrefs.GetFloat("BGMVol")) * 20);
        }else{
            PlayerPrefs.SetFloat("BGMVol", 0.0001f);
        }
    }

    void OnApplicationQuit() {
        // Force stop music play when quit game
        if(AudioListener.volume != 0){
            AudioListener.volume = 0;
        }
    }
}
