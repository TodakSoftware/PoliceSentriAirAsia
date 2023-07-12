using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyButton : MonoBehaviour
{
    public int dayIndex;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI kupangText;
    public TextMeshProUGUI airAsiaPointText;
    public TextMeshProUGUI btnText;
    public GameObject tickGO;
    public Button claimBtn;

    void Start()
    {
        claimBtn.onClick.AddListener( delegate{ Claimed(); } );
        DisableButton();
        PlayerPrefs.DeleteKey("DayRewardClaimed");
    }

    public void EnableButton(){
        tickGO.SetActive(false);
        claimBtn.interactable = true;
        btnText.SetText("Claim");
        btnText.color = Color.black;
    } // end EnableButton

    public void DisableButton(){
        tickGO.SetActive(false);
        claimBtn.interactable = false;
        btnText.SetText("Not Yet");
        //btnText.color = Color.red;
    } // end DisableButton

    public void Claimed(){
        // Save playerprefs to date today without time
        tickGO.SetActive(true);
        claimBtn.interactable = false;
        btnText.SetText("Claimed");
        claimBtn.GetComponent<Image>().color = Color.green;
        btnText.color = Color.white;

        // Save to userdata
        UserDataManager.instance.currentKupang += int.Parse(kupangText.text);
        // API Airasia to give point

        PlayerPrefs.SetInt("DayRewardClaimed", dayIndex);
        print("Save " + dayIndex);
        PlayerPrefs.Save();
    } // end Claimed
}
