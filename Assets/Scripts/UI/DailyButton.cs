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

    void OnEnable(){
        /* PayloadSender.instance.OnPayloadSentWebview -= OnPayloadSentWebview;
        PayloadSender.instance.OnPayloadSentWebview += OnPayloadSentWebview; */
    }

    void OnDisable(){
        //PayloadSender.instance.OnPayloadSentWebview -= OnPayloadSentWebview;
    }

    /* void OnPayloadSentWebview(){
        if(PayloadSender.instance.payloadSent){
            if(UserDataManager.instance.latestRewardClaimedDay != dayIndex){
                tickGO.SetActive(true);
                shinyGO.SetActive(false);
                claimBtn.interactable = false;
                btnText.SetText("Claimed");

                var color = claimBtn.GetComponent<Image>().color;
                color.a = 0;
                claimBtn.GetComponent<Image>().color = color;
                btnText.color = claimedColor;

                PlayerPrefs.SetInt("DayRewardClaimed", dayIndex);
                
                UserDataManager.instance.UpdateFirebaseData("latestRewardClaimedDay", dayIndex);
                UserDataManager.instance.latestRewardClaimedDay = dayIndex;

                Invoke(nameof(DelayUpdateKupang), 1f);

                PlayerPrefs.Save();
            }
        }else{
            print("Cannot Claim Because Server Not Responding");
            NotificationManager.instance.PopupNotification("Cannot Claim Because Server Not Responding");
        }
    } */

    [Obsolete]
    void Start()
    {
        claimBtn.onClick.AddListener( delegate{ Claimed(); } ); //
        DisableButton();
        PlayerPrefs.DeleteKey("DayRewardClaimed");
    }

    public void EnableButton(){
        tickGO.SetActive(false);
        if(shinyGO != null){
            shinyGO.SetActive(true);
            shinyGO.GetComponent<SpinningUI>().StartSpinning();
        }
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

    [Obsolete]
    public void Claimed()
    {
        if (int.TryParse(airAsiaPointText.text, out int points) && points > 0)
        {
            PayloadData payload = new PayloadData
            {
                memberId = UserDataManager.instance.memberID,
                partnerCode = "AAB",
                transactionDate = GetCurrentDateTimeFormatted(),
                points = points.ToString(),
                description = $"TAG Police Sentri : Daily Reward For {dayText.text}",
                referenceNumber = GenerateReferenceNumber(),
                partnerMID = "4582088801"
            };

            PayloadSender.instance.SendPayloadParams(payload);
        }

        SaveClaimData();
        Invoke(nameof(DelayUpdateKupang), .2f);
        UpdateUIForClaimed();
    }

    private void UpdateUIForClaimed()
    {
        if(shinyGO != null){
            shinyGO.SetActive(false);
        }
        claimBtn.interactable = false;
        btnText.text = "Processing";

        Image btnImage = claimBtn.GetComponent<Image>();
        Color btnColor = btnImage.color;
        btnColor.a = 0;
        btnImage.color = btnColor;

        btnText.color = Color.gray;
        Invoke(nameof(DelayClaimedText), 1f);
    }

    [Obsolete]
    void DelayClaimedText(){
        if(UserDataManager.instance.TryLoadFromFirebase()){
            if(UserDataManager.instance.latestRewardClaimedDay == dayIndex){    
                btnText.text = "Claimed";
            btnText.color = claimedColor;
            tickGO.SetActive(true);
        }else{
                EnableButton();
            }
        }
    }

    private void SaveClaimData()
    {
        PlayerPrefs.SetInt("DayRewardClaimed", dayIndex);
        UserDataManager.instance.UpdateFirebaseData("latestRewardClaimedDay", dayIndex);
       // Invoke(nameof(DelayUpdateClaimedDailyRewardTime), .3f);
        UserDataManager.instance.latestRewardClaimedDay = dayIndex;
        PlayerPrefs.Save();
    }
    void DelayUpdateClaimedDailyRewardTime()
    {
        UserDataManager.instance.UpdateFirebaseData("claimedDailyRewardTime", DailyRewardManager.instance.todayDateTime.ToString());
    }

    void DelayUpdateKupang(){
            UserDataManager.instance.currentKupang += int.Parse(kupangText.text);
            print("Added Kupang " + (int)UserDataManager.instance.currentKupang);
            UserDataManager.instance.UpdateFirebaseData("currentKupang", (int)UserDataManager.instance.currentKupang);
    }

    public void ClaimedVisual(){
        // Save playerprefs to date today without time
        tickGO.SetActive(true);
        if(shinyGO != null){
            shinyGO.SetActive(false);
        }
        claimBtn.interactable = false;
        btnText.SetText("Claimed");

        var color = claimBtn.GetComponent<Image>().color;
        color.a = 0;
        claimBtn.GetComponent<Image>().color = color;
        btnText.color = claimedColor;
    }

    public string GetCurrentDateTimeFormatted()
    {
        DateTime now = DateTime.Now;
        string formattedDateTime = now.ToString("yyyyMMddHHmmss");
        return formattedDateTime;
    }

    public string GenerateReferenceNumber()
    {
        string prefix = "TAGPS";
        string randomValue = GenerateRandomString(6); // Generate an 6-character random string
        string referenceNumber = prefix + randomValue;
        return referenceNumber;
    }

    // Function to generate a random string of a specified length
    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        System.Random random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}
