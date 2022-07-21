using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Modal_Username : MonoBehaviour
{
    public Button confirmBtn;
    public TMP_InputField usernameInput;

    void OnEnable(){
        if(PlayerPrefs.HasKey("Username")){
            usernameInput.text = PlayerPrefs.GetString("Username");
        }
    }

    void Update(){
        if(usernameInput.text.Length > 3){
            confirmBtn.interactable = true;
        }else{
            confirmBtn.interactable = false;
        }
    }

    public void ConfirmUsername(){
        PlayerPrefs.SetString("Username", usernameInput.text);
        UIManager.instance.p_MainMenu.usernameValue = usernameInput.text;
        UIManager.instance.p_MainMenu.usernameText.text = usernameInput.text;
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
