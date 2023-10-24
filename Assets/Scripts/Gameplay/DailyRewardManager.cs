using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


[System.Serializable]
public class WorldTimeResponse
{
    public string datetime;
}

[System.Serializable]
public struct S_DailyRewards{
    public string name;
    public int day;
    public int kupangValue; // CHange to float/int if reward type is Kupang/ Big Points
    public int airAsiaPointValue; // CHange to float/int if reward type is Kupang/ Big Points
}

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager instance;
    public GameObject dailyRewardPanel;
    public List<S_DailyRewards> dailyRewardData = new List<S_DailyRewards>(); // Data only
    public List<DailyButton> dailyRewardBtnLists = new List<DailyButton>(); // Data only

    public DateTime startDateTime; // set by firebase
    public DateTime todayDateTime; // set by system
    Coroutine cacheDateTime;
    [HideInInspector]public WebRequest webRequestScript;
    [HideInInspector]public AwsRequest awsRequest;

    /* void OnEnable()
    {
        if(UserDataManager.instance.latestRewardClaimedDay == PlayerPrefs.GetInt("DayRewardClaimed")){
            dailyRewardPanel.SetActive(false);
        }
    } */
    
    void Awake()
    {
        webRequestScript = GetComponent<WebRequest>();
        awsRequest = GetComponent<AwsRequest>();
        instance = this;
    }

    [Obsolete]
    public void Start2()
    {
        cacheDateTime = StartCoroutine(GetRealtimeDate());
        dailyRewardPanel.SetActive(true);
    }

    public void SetDailyRewardStartDate(DateTime _date){ // Set to firebase 1 time. Will reset after 
        startDateTime = _date;
        // FIREBASE SAVE START DATE

        UserDataManager.instance.startDailyRewardDate = startDateTime.ToString();
        UserDataManager.instance.UpdateFirebaseData("startDailyRewardDate", UserDataManager.instance.startDailyRewardDate);
        //UserDataManager.instance.latestRewardClaimedDay = 0;
        //DateTime oDate = DateTime.Parse(DailyRewardManager.instance.todayDateTime.ToString());
    } // end SetDailyRewardStartDate

    [Obsolete]
    public void CheckPlayerStartDailyLogin(){ // Check player start date
        // Get from firebase
        // If null
        for(int i = 0; i < dailyRewardBtnLists.Count; i++){
            dailyRewardBtnLists[i].DisableButton();

            dailyRewardBtnLists[i].kupangText.text = dailyRewardData[i].kupangValue.ToString();
            dailyRewardBtnLists[i].airAsiaPointText.text = dailyRewardData[i].airAsiaPointValue.ToString();
        }
        // Set to current
        if(UserDataManager.instance.startDailyRewardDate == ""){ // startDateTime.Year == 0001 || 
            SetDailyRewardStartDate(todayDateTime);
            print("NULL DATE, so we set new " + startDateTime);
        }
        
        if(CalculateDaysPassed(startDateTime, todayDateTime) >= 7){ // After more than a week, reset 0 - 6, full week
            print("Passed 7 days, reset new start date " + startDateTime);
            SetDailyRewardStartDate(todayDateTime); // of use fire
        }

        if((CalculateDaysPassed(startDateTime, todayDateTime) - UserDataManager.instance.latestRewardClaimedDay) > 1){ // IF SKIP 1 day, reset
            print("Skip daily reward > 1 day, RESET");
            SetDailyRewardStartDate(todayDateTime);
        }

        if(UserDataManager.instance.latestRewardClaimedDay >= 0 && (UserDataManager.instance.latestRewardClaimedDay - 1) == CalculateDaysPassed(startDateTime, todayDateTime)){
            dailyRewardBtnLists[CalculateDaysPassed(startDateTime, todayDateTime)].Claimed();
            print("Claimed Day");
        }else{
            dailyRewardBtnLists[CalculateDaysPassed(startDateTime, todayDateTime)].EnableButton();
        }
        
    } // end CheckPlayerStartDailyLogin

    [Obsolete]
    private IEnumerator GetRealtimeDate()
    {
        string url = "https://worldtimeapi.org/api/ip";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;

                // Parse the JSON response
                WorldTimeResponse worldTimeResponse = JsonUtility.FromJson<WorldTimeResponse>(response);

                // Extract the date information
                DateTime currentDateTime = DateTime.Parse(worldTimeResponse.datetime);
                todayDateTime = currentDateTime;

                CheckPlayerStartDailyLogin();

                StopCoroutine(cacheDateTime);
                print("Stop Coroutine");
            }
            else
            {
                Debug.LogError("Failed to retrieve real-time date. Error: " + webRequest.error);
                print("Retrying...");
                StartCoroutine(GetRealtimeDate());
            }
        }

        
    } // end GetRealtimeDate

    private int CalculateDaysPassed(DateTime startDate, DateTime endDate)
    {
        TimeSpan duration = endDate - startDate;
        int daysPassed = duration.Days;
        return daysPassed;
    } // end CalculateDaysPassed
}
