using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class P_EndScreen : MonoBehaviour
{
    public Button endScreenleaveBtn, playAgainBtn;

    void Start(){
        endScreenleaveBtn.onClick.AddListener(delegate{ StartCoroutine(NetworkManager.instance.InGameLeaveRoom());});
        playAgainBtn.onClick.AddListener(delegate{ PlayAgainButton();});
    }

    void PlayAgainButton(){
        //GameManager.instance.endGamePlayAgain = true;
        //GameManager.instance.AskHostToPlayAgain();
        PlayerPrefs.SetInt("PlayAgain", 1);
        PlayerPrefs.Save();
        
        HideButton();
    }

    void HideButton(){
        playAgainBtn.gameObject.SetActive(false);
        endScreenleaveBtn.gameObject.SetActive(false);
    }
}
