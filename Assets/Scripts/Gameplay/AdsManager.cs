using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdsManager : MonoBehaviour
{
    public Button testAdsBtn;

    void Awake(){
        testAdsBtn.onClick.AddListener(delegate{ ShowAd(); });
    }
/* 
    // Trigger the ad popup
    [System.Obsolete]
    public void TriggerAdPopup()
    {
        string javascriptCode = @"
            googletag.cmd.push(function() { 
                googletag.display('Game-Police-Sentri'); 
                Unity.call('AdTriggered'); // Call Unity function when ad is displayed
            });";
        Application.ExternalEval(javascriptCode);
    }

    // Unity C# function to be called from JavaScript
    public void AdTriggered()
    {
        Debug.Log("Ad Triggered!");
        NotificationManager.instance.PopupNotification("Ads Triggered!");
        // Add any additional handling here
    }

    // Example method to show the ad popup directly
    [System.Obsolete]
    public void ShowAdPopup()
    {
        TriggerAdPopup();
    } */

    // Method to call the showAd() function in the HTML page
    public void ShowAd()
    {
        string url = "javascript:showAd();";
        NotificationManager.instance.PopupNotification("Showing Ads");
        Application.OpenURL(url);
    }
}
