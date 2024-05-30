using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.Networking;
using System.Collections;
using SimpleJSON;

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
    public int latestRewardClaimedDay; // button index day claimed

    [Header("---- Firebase -----")]
    public string firebasBaseURL = "https://police-sentri-airasia-default-rtdb.asia-southeast1.firebasedatabase.app/";
    public GameObject dailyRewardGO;
    
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
            if(memberID != ""){
                LoadFromFirebase();
            }else{
                memberID = "0000000000";
                SaveToFirebase();
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
    public void SaveToFirebase(){
        C_UserData userDat = new C_UserData();
        userDat.currentKupang = currentKupang;
        userDat.policeList = policeList;
        userDat.robberList = robberList;
        userDat.petList = petList;
        userDat.startDailyRewardDate = startDailyRewardDate;
        userDat.latestRewardClaimedDay = latestRewardClaimedDay;

        RestClient.Put(firebasBaseURL + memberID + "/.json", userDat);
        print("SAVED");
        Invoke("StartLoadReward", 1f);
    } // end SaveToFirebase

    public void UpdateFirebaseData(string fieldName, object fieldValue)
    {
        StartCoroutine(FetchAndUpdateField(fieldName, fieldValue));
    }

    private IEnumerator FetchAndUpdateField(string fieldName, object fieldValue)
    {
        string url = firebasBaseURL + memberID + ".json";

        using (UnityWebRequest getRequest = UnityWebRequest.Get(url))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data from Firebase: " + getRequest.error);
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
                    Debug.LogError("Error saving " + fieldName + " value to Firebase: " + patchRequest.error);
                }
                else
                {
                    Debug.Log(fieldName + " value saved to Firebase: " + fieldValue);
                }
            }
        }
    }

    [System.Obsolete]
    public void LoadFromFirebase(){
        RestClient.Get<C_UserData>(firebasBaseURL + memberID + "/.json").Then(response =>
            {
                currentKupang = response.currentKupang;
                policeList = response.policeList;
                robberList = response.robberList;
                petList = response.petList;
                startDailyRewardDate = response.startDailyRewardDate;
                latestRewardClaimedDay = response.latestRewardClaimedDay;

                print("Loaded");
                Invoke("StartLoadReward", 1f);
            }
        );
    } // end LoadFromFirebase

    
}