using System;
using System.Collections.Generic;
using UnityEngine;


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
    public string latestVersion;
    public GameObject dailyRewardPanel;
    public List<S_DailyRewards> dailyRewardData = new List<S_DailyRewards>(); // Data only
    public List<DailyButton> dailyRewardBtnLists = new List<DailyButton>(); // Data only

    public DateTime startDateTime; // set by firebase
    public DateTime todayDateTime; // set by system
    public bool timeHasInit = false;
    //Coroutine cacheDateTime;
    
    void Awake()
    {
       /*  if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        } */
        instance = this;
        latestVersion = Application.version;
        Invoke("Start2", 1f);
    }

    [Obsolete]
    public void Start2()
    {
        //cacheDateTime = StartCoroutine(GetRealtimeDate());
        GetLocalDate();
        //CheckPlayerStartDailyLoginClaimed();
        dailyRewardPanel.SetActive(true);
    }

    public void SetDailyRewardStartDate(DateTime _date)
    {
        // Set to firebase 1 time. Will reset after
        startDateTime = _date;
        
        // Format the date in the desired format
        string formattedDate = startDateTime.ToString("d/M/yyyy h:mm:ss tt");
        
        // FIREBASE SAVE START DATE
        UserDataManager.instance.startDailyRewardDate = formattedDate;
        //UserDataManager.instance.claimedDailyRewardTime = formattedDate;
        
        // Print the formatted date
        //print("Hai " + formattedDate);
        
        // Update Firebase if the formatted date string is not empty
        if(UserDataManager.instance.startDailyRewardDate != "")
        {
            UserDataManager.instance.UpdateFirebaseData("startDailyRewardDate", UserDataManager.instance.startDailyRewardDate);
        }

        //Invoke(nameof(DelayUpdateClaimedDailyRewardTime), .5f);
    } // end SetDailyRewardStartDate

    /* public void CheckPlayerStartDailyLoginClaimed(){ // Check player start date
        // 1st: Disable all button
        for(int i = 0; i < dailyRewardBtnLists.Count; i++){
            dailyRewardBtnLists[i].DisableButton();
        }

        // 2nd: Enable button ahead of current day
        if(UserDataManager.instance.latestRewardClaimedDay < 7){
            dailyRewardBtnLists[UserDataManager.instance.latestRewardClaimedDay].EnableButton();
        }
        

        // 3: If claimed more than 0, show claimed visual
        if(UserDataManager.instance.latestRewardClaimedDay > 0){
            for(int i = 0; i < UserDataManager.instance.latestRewardClaimedDay; i++){
                dailyRewardBtnLists[i].ClaimedVisual();
            }
        }
    } // end CheckPlayerStartDailyLoginClaimed */

    void DelayUpdateClaimedDailyRewardTime()
    {
        if(UserDataManager.instance.claimedDailyRewardTime != "")
        {
            UserDataManager.instance.UpdateFirebaseData("claimedDailyRewardTime", UserDataManager.instance.claimedDailyRewardTime);
        }
    }

    [Obsolete]
    public void CheckPlayerStartDailyLogin()
    { 
        // Disable all buttons and set reward values
        for (int i = 0; i < dailyRewardBtnLists.Count; i++)
        {
            dailyRewardBtnLists[i].DisableButton();
            dailyRewardBtnLists[i].kupangText.text = dailyRewardData[i].kupangValue.ToString();
            dailyRewardBtnLists[i].airAsiaPointText.text = dailyRewardData[i].airAsiaPointValue.ToString();
        }

        // Handle different scenarios
        if (string.IsNullOrEmpty(UserDataManager.instance.startDailyRewardDate))
        {
            SetDailyRewardStartDate(todayDateTime);
            Debug.Log($"NULL DATE, so we set new {todayDateTime}");
        }
        else
        {
            if(CalculateDaysPassed(startDateTime, todayDateTime) <= 6){
                if(UserDataManager.instance.latestRewardClaimedDay <= 7){
                    //int minutesPassed = CalculateMinutesPassed(startDateTime, todayDateTime);
                    int daysPassed = CalculateDaysPassed(startDateTime, todayDateTime);
                    int daysSkipped = daysPassed - UserDataManager.instance.latestRewardClaimedDay; 
                    

                    /* if (daysPassed > 7)
                    {
                        Debug.Log($"Passed 7 days, reset new start date {todayDateTime}");
                        SetDailyRewardStartDate(todayDateTime);
                    } */
                    
                    //if (daysSkipped > 1){
                    if (daysPassed > 1 && UserDataManager.instance.latestRewardClaimedDay < daysPassed){
                        PlayerPrefs.SetInt("DayRewardClaimed", 0);
                        UserDataManager.instance.UpdateFirebaseData("latestRewardClaimedDay", 0);
                        UserDataManager.instance.latestRewardClaimedDay = 0;
                        PlayerPrefs.Save();
                        NotificationManager.instance.PopupNotification($"Skip day {daysPassed}, RESET");
                        //Invoke(nameof(DelayUpdateClaimedDailyRewardTime), .3f);
                        SetDailyRewardStartDate(todayDateTime);
                    }
                    /* else if (minutesPassed >= 1 && minutesPassed < 2)
                    {
                        //Debug.Log($"Skipped {daysSkipped} days, RESET");
                        print("Enable button for next today");
                        NotificationManager.instance.PopupNotification(minutesPassed + " minutes has passed, enable day " + (UserDataManager.instance.latestRewardClaimedDay + 1));
                        dailyRewardBtnLists[UserDataManager.instance.latestRewardClaimedDay].EnableButton();
                    }
                    //else if (daysSkipped > 1)
                    else if (minutesPassed >= 2)
                    {
                        //Debug.Log($"Skipped {daysSkipped} days, RESET");
                        Debug.Log($"Skipped {minutesPassed} minutes, RESET");
                        PlayerPrefs.SetInt("DayRewardClaimed", 0);
                        UserDataManager.instance.UpdateFirebaseData("latestRewardClaimedDay", 0);
                        UserDataManager.instance.latestRewardClaimedDay = 0;
                        PlayerPrefs.Save();
                        NotificationManager.instance.PopupNotification("You have skipped " + minutesPassed + " minutes. Resetting.");
                        Invoke(nameof(DelayUpdateClaimedDailyRewardTime), .3f);
                        SetDailyRewardStartDate(todayDateTime);
                    } */

                    // Show claimed rewards
                    for (int i = 0; i < UserDataManager.instance.latestRewardClaimedDay; i++)
                    {
                        dailyRewardBtnLists[i].ClaimedVisual();
                    }

                    // Enable button for current day if not claimed
                    int currentDay = CalculateDaysPassed(startDateTime, todayDateTime);
                    if (currentDay < dailyRewardBtnLists.Count && currentDay >= UserDataManager.instance.latestRewardClaimedDay && timeHasInit){
                        dailyRewardBtnLists[currentDay].EnableButton();
                    }

                }
            }
            else{
                Debug.Log($"New Week Refreshed!");

                PlayerPrefs.SetInt("DayRewardClaimed", 0);
                UserDataManager.instance.UpdateFirebaseData("latestRewardClaimedDay", 0);
                UserDataManager.instance.latestRewardClaimedDay = 0;
                PlayerPrefs.Save();
                //Invoke(nameof(DelayUpdateClaimedDailyRewardTime), .3f);
                SetDailyRewardStartDate(todayDateTime);

                // Enable button for current day if not claimed
                int currentDay = 0;
                
                if (currentDay < dailyRewardBtnLists.Count && currentDay >= UserDataManager.instance.latestRewardClaimedDay && timeHasInit){
                    dailyRewardBtnLists[currentDay].EnableButton();
                }
            }

            
        }

        
    }

    /* [Obsolete]
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
            }
            else
            {
                Debug.LogError("Failed to retrieve real-time date. Error: " + webRequest.error);
                print("Retrying...");
                StartCoroutine(GetRealtimeDate());
            }
        }

        
    } // end GetRealtimeDate */

    [Obsolete]
    private void GetLocalDate()
    {
        // Attempt to get the local date and time
        DateTime currentDateTime = DateTime.Now;

        // Check if the retrieved date is valid
        if (currentDateTime == DateTime.MinValue)
        {
            Debug.LogWarning("Failed to retrieve local date, retrying...");
            NotificationManager.instance.PopupNotification("Failed to retrieve local date, retrying...");
            Invoke(nameof(GetLocalDate), 1f); // Retry after 1 second
            return;
        }

        todayDateTime = currentDateTime;
        timeHasInit = true;

        // Check if todayDateTime is valid before calling CheckPlayerStartDailyLogin
        if (todayDateTime != DateTime.MinValue)
        {
            CheckPlayerStartDailyLogin();
        }
    }

    private int CalculateDaysPassed(DateTime startDate, DateTime endDate)
    {
        // Check if startDate is the default DateTime value
        if (startDate == DateTime.MinValue)
        {
            // Define the date format used in startDailyRewardDate
            string dateFormat = "d/M/yyyy h:mm:ss tt";
            
            // Convert the string to DateTime using DateTime.ParseExact
            if (DateTime.TryParseExact(UserDataManager.instance.startDailyRewardDate, dateFormat, 
                                       System.Globalization.CultureInfo.InvariantCulture, 
                                       System.Globalization.DateTimeStyles.None, 
                                       out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }
            else
            {
                Debug.LogError("Invalid start date format in UserDataManager.");
                return -1; // or any other error handling
            }
        }
        
        // Calculate the duration
        TimeSpan duration = endDate - startDate;
        
        // Log the calculated duration in days
        int daysPassed = duration.Days;
        
        // Return the days passed
        print("Days passed: " + daysPassed);
        return daysPassed;
    }

    private int CalculateMinutesPassed(DateTime startDate, DateTime endDate)
    {
        // Check if startDate is the default DateTime value
        if (startDate == DateTime.MinValue)
        {
            string dateFormat = "d/M/yyyy h:mm:ss tt";
            
            if (DateTime.TryParseExact(UserDataManager.instance.claimedDailyRewardTime, dateFormat, 
                                       System.Globalization.CultureInfo.InvariantCulture, 
                                       System.Globalization.DateTimeStyles.None, 
                                       out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }
            else
            {
                Debug.LogError("Invalid start date format in UserDataManager.");
                return -1; // or any other error handling
            }
        }
        
        // Calculate the duration
        TimeSpan duration = endDate - startDate;
        
        // Calculate total minutes passed
        int minutesPassed = (int)duration.TotalMinutes;
        
        return minutesPassed;
    }
}
