using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Modal_Username : MonoBehaviour
{
    public Button confirmBtn;
    public TMP_InputField usernameInput;
    public TextMeshProUGUI profileAirasiaText;
    public AirAsiaProfile aaProfile;
    //public Button closeBtn;

    void OnEnable(){
        if(PlayerPrefs.HasKey("Username")){
            if(PlayerPrefs.GetString("Username") != ConfigReceiver.instance.configData.name && ConfigReceiver.instance.configData.name != ""){
                usernameInput.text = ConfigReceiver.instance.configData.name;
                PlayerPrefs.SetString("Username", ConfigReceiver.instance.configData.name);
                PlayerPrefs.Save();
            }else{
                usernameInput.text = PlayerPrefs.GetString("Username");
            }
        }

        if(ConfigReceiver.instance.configData.name != ""){
            usernameInput.text = ConfigReceiver.instance.configData.name;
        }
    }

    void Update(){
        if(usernameInput.text.Length >= 3){
            confirmBtn.interactable = true;
            //closeBtn.interactable = true;
        }else{
            confirmBtn.interactable = false;
            //closeBtn.interactable = false;
        }
    }

    public void ConfirmUsername(){
        string userText = usernameInput.text.ToLower();
        userText = userText.Replace(" ", "_");
        PlayerPrefs.SetString("Username", userText);
        UIManager.instance.p_MainMenu.usernameValue = userText;
        UIManager.instance.p_MainMenu.usernameText.text = userText;
        profileAirasiaText.text = userText;
        //aaProfile.FetchAirasiaData();
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
