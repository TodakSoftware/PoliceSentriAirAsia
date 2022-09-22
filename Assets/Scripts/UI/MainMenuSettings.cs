using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    public Toggle muteToggle;
    bool firstRun;
    public Slider sfxSlider, musicSlider;
    private AudioMixer audioMix;
    public TextMeshProUGUI versionText;

    void Awake(){
        audioMix = GameSettingsManager.instance.audioMix;
        versionText.text = "V"+ Application.version;
    }

    void Start(){
        // Assign as setted in GameSettingManager
        sfxSlider.value = GameSettingsManager.instance.sfxVolume;
        audioMix.SetFloat("SFXVol", Mathf.Log10(sfxSlider.value) * 20);

        musicSlider.value = GameSettingsManager.instance.musicVolume;
        audioMix.SetFloat("BGMVol", Mathf.Log10(musicSlider.value) * 20);

        muteToggle.isOn = GameSettingsManager.instance.mute;

        sfxSlider.onValueChanged.AddListener(delegate {SetSfxLvl(sfxSlider.value);});
        musicSlider.onValueChanged.AddListener(delegate {SetBgmLvl(musicSlider.value);});
        muteToggle.onValueChanged.AddListener(delegate { GameSettingsManager.instance.Mute();});
    }

    public void MuteUnmute(){
        print("Running");
        GameSettingsManager.instance.Mute();
    }

    public void SetSfxLvl(float sfxLvl){
        audioMix.SetFloat("SFXVol", Mathf.Log10(sfxLvl) * 20);
        GameSettingsManager.instance.sfxVolume = sfxLvl;
        PlayerPrefs.SetFloat("SFXVol", sfxLvl);
        PlayerPrefs.Save();
    }

    public void SetBgmLvl(float bgmLvl){
        audioMix.SetFloat("BGMVol", Mathf.Log10(bgmLvl) * 20);
        GameSettingsManager.instance.musicVolume = bgmLvl;
        PlayerPrefs.SetFloat("BGMVol", bgmLvl);
        PlayerPrefs.Save();
    }

    public void ClearVolume(){
        audioMix.ClearFloat("BGMVol");
        audioMix.ClearFloat("SFXVol");
    }
}
