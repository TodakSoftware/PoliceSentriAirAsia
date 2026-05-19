using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;
using UnityEngine.UI;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance;
    public string memberID = "";
    public float currentKupang = 0;
    public List<string> policeList = new List<string>();
    public List<string> robberList = new List<string>();
    public List<string> petList = new List<string>();

    [Header("---- Daily Rewards -----")]
    public string startDailyRewardDate;
    public string claimedDailyRewardTime;
    public int latestRewardClaimedDay; // button index day claimed

    [Header("---- Firebase -----")]
    public string firebaseBaseURL = "https://police-sentri-airasia-default-rtdb.asia-southeast1.firebasedatabase.app/";
    public GameObject dailyRewardGO;

    public TextMeshProUGUI memberIDText;
    
    public void Awake()
    {
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    } // end Awake

    [System.Obsolete]
    void Start()
    {
        if(!PhotonNetworkManager.instance.offlineMode){
            if(memberID == "" || string.IsNullOrEmpty(memberID)){
                //LoadFromFirebase();
                //memberID = "9999990005545751";
                memberID = "0";
                NotificationManager.instance.PopupNotification("Member ID not defined");
                LoadFromFirebase();
            }else{
                LoadFromFirebase();
            }
        }
    }

    [System.Obsolete]
    public void StartLoadReward(){
        // Load 1st
        print("Rewarding");
        dailyRewardGO.SetActive(true);
        DailyRewardManager.instance.Start2();
    }

    [System.Obsolete]
    public void SaveToFirebase()
    {
        memberIDText.text = memberID;

        C_UserData userDat = new C_UserData
        {
            currentKupang = currentKupang,
            policeList = policeList,
            robberList = robberList,
            petList = petList,
            startDailyRewardDate = startDailyRewardDate,
            claimedDailyRewardTime = claimedDailyRewardTime,
            latestRewardClaimedDay = latestRewardClaimedDay
        };

        RestClient.Put(firebaseBaseURL + memberID + "/.json", userDat).Then(response =>
        {
            //Debug.Log("Data saved successfully.");
            Invoke("StartLoadReward", 1f);
        }).Catch(error =>
        {
            Debug.LogError("Failed to save data to Database: " + error.Message);
        });
    }

    public void UpdateFirebaseData(string fieldName, object fieldValue)
    {
        StartCoroutine(FetchAndUpdateField(fieldName, fieldValue));
    }

    private IEnumerator FetchAndUpdateField(string fieldName, object fieldValue)
    {
        string url = firebaseBaseURL + memberID + ".json";

        using (UnityWebRequest getRequest = UnityWebRequest.Get(url))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data from Database: " + getRequest.error);
                yield break;
            }

            // Parse the existing data from Firebase
            string json = getRequest.downloadHandler.text;
            C_UserData existingData = JsonUtility.FromJson<C_UserData>(json);

            // Update the specific field locally
            var fieldInfo = existingData.GetType().GetField(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(existingData, fieldValue);
            }
            else
            {
                Debug.LogError("Field not found: " + fieldName);
                yield break;
            }

            // Convert the updated data back to JSON
            string updatedJson = JsonUtility.ToJson(existingData);

            // Send the patched data to Firebase
            using (UnityWebRequest patchRequest = new UnityWebRequest(url, "PATCH"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(updatedJson);
                patchRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                patchRequest.downloadHandler = new DownloadHandlerBuffer();
                patchRequest.SetRequestHeader("Content-Type", "application/json");

                yield return patchRequest.SendWebRequest();

                if (patchRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error saving " + fieldName + " value to Database: " + patchRequest.error);
                    //NotificationManager.instance.PopupNotification("Error Saving firebase : " + fieldName);
                }
                else
                {
                    //Debug.Log(fieldName + " value saved to Firebase: " + fieldValue);
                    //NotificationManager.instance.PopupNotification(fieldName + " = " + fieldValue);
                }
            }
        }
    }

    [System.Obsolete]
    public void LoadFromFirebase()
    {
        if (string.IsNullOrEmpty(memberID))
        {
            Debug.LogWarning("memberID not defined");
            return;
        }

        memberIDText.text = memberID;

        RestClient.Get(firebaseBaseURL + memberID + "/.json").Then(response =>
        {
            if (string.IsNullOrEmpty(response.Text) || response.Text == "null")
            {
                //Debug.LogWarning("No data found at the specified Firebase path.");
                SaveToFirebase();
                return;
            }

            try
            {
                C_UserData userData = JsonUtility.FromJson<C_UserData>(response.Text);

                currentKupang = userData.currentKupang;
                policeList = userData.policeList;
                robberList = userData.robberList;
                petList = userData.petList;
                startDailyRewardDate = userData.startDailyRewardDate;
                claimedDailyRewardTime = userData.claimedDailyRewardTime;
                latestRewardClaimedDay = userData.latestRewardClaimedDay;

                Invoke("StartLoadReward", 1f);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse Firebase data: " + e.Message);
                SaveToFirebase();
            }
        }).Catch(error =>
        {
            Debug.LogError("Failed to load data from Firebase: " + error.Message);
            NotificationManager.instance.PopupNotification("Failed to load data from Database: " + error.Message);
            //SaveToFirebase();
        });
    }

    public void DeleteFromFirebase(Button _deleteButton)
    {
        if (string.IsNullOrEmpty(memberID))
        {
            Debug.LogWarning("memberID not defined");
            return;
        }

        RestClient.Delete(firebaseBaseURL + memberID + "/.json").Then(response =>
        {
            Debug.Log("Successfully deleted data for memberID: " + memberID);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save(); // Ensure changes are saved
            Debug.Log("All PlayerPrefs have been cleared. Reload the game.");
            NotificationManager.instance.PopupNotification("Data Deleted! Please Refresh.");
            _deleteButton.gameObject.SetActive(false);
        }).Catch(error =>
        {
            Debug.LogError("Failed to delete data from Firebase: " + error.Message);
        });
    }

    [Obsolete]
    public bool TryLoadFromFirebase()
    {
        if (string.IsNullOrEmpty(memberID))
        {
            Debug.LogWarning("memberID not defined");
            return false; // Return false if memberID is not defined
        }

        memberIDText.text = memberID;

        RestClient.Get(firebaseBaseURL + memberID + "/.json").Then(response =>
        {
            if (string.IsNullOrEmpty(response.Text) || response.Text == "null")
            {
                SaveToFirebase();
                return;
            }

            try
            {
                C_UserData userData = JsonUtility.FromJson<C_UserData>(response.Text);

                currentKupang = userData.currentKupang;
                policeList = userData.policeList;
                robberList = userData.robberList;
                petList = userData.petList;
                startDailyRewardDate = userData.startDailyRewardDate;
                claimedDailyRewardTime = userData.claimedDailyRewardTime;
                latestRewardClaimedDay = userData.latestRewardClaimedDay;

                Invoke("StartLoadReward", 1f);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse Firebase data: " + e.Message);
                SaveToFirebase();
            }
        }).Catch(error =>
        {
            Debug.LogError("Failed to load data from Firebase: " + error.Message);
            NotificationManager.instance.PopupNotification("Failed to load data from Database: " + error.Message);
        });

        return true; // Return true if the request was initiated
    }
}