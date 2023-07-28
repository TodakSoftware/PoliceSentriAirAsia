using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyButton : MonoBehaviour
{
    public int dayIndex;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI kupangText;
    public TextMeshProUGUI airAsiaPointText;
    public TextMeshProUGUI btnText;
    public GameObject tickGO, shinyGO;
    public Button claimBtn;
    public Color claimedColor;


    void Start()
    {
        claimBtn.onClick.AddListener( delegate{ Claimed(); } );
        DisableButton();
        PlayerPrefs.DeleteKey("DayRewardClaimed");
    }

    public void EnableButton(){
        tickGO.SetActive(false);
        shinyGO.SetActive(true);
        claimBtn.interactable = true;
        claimBtn.GetComponent<Image>().color = Color.white;
        btnText.SetText("Claim");
        btnText.color = Color.black;
    } // end EnableButton

    public void DisableButton(){
        tickGO.SetActive(false);
        claimBtn.interactable = false;
        claimBtn.GetComponent<Image>().color = Color.gray;
        btnText.SetText("Not Yet");
        btnText.color = Color.black;
    } // end DisableButton

    public void Claimed(){
        // Save playerprefs to date today without time
        tickGO.SetActive(true);
        shinyGO.SetActive(false);
        claimBtn.interactable = false;
        btnText.SetText("Claimed");

        var color = claimBtn.GetComponent<Image>().color;
        color.a = 0;
        claimBtn.GetComponent<Image>().color = color;
        btnText.color = claimedColor;

        // Save to userdata
        UserDataManager.instance.currentKupang += int.Parse(kupangText.text);
        // API Airasia to give point

        PlayerPrefs.SetInt("DayRewardClaimed", dayIndex);
        UserDataManager.instance.latestRewardClaimedDay = dayIndex;
        PlayerPrefs.Save();
    } // end Claimed
}
