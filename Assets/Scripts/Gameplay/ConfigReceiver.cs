using TMPro;
using UnityEngine;

public class ConfigReceiver : MonoBehaviour
{
    [System.Serializable]
    public class Config
    {
        public string dataUrl;
        public string frameworkUrl;
        public string codeUrl;
        public string memoryUrl;
        public string symbolsUrl;
        public string streamingAssetsUrl;
        public string companyName;
        public string productName;
        public string productVersion;
        public string name;
        public string memberId;
        public string avatar;
    }

    public static ConfigReceiver instance;
    public Config configData;

    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    [System.Obsolete]
    public void SetConfig(string configJson)
    {
        if(configData.name == ""){
            Config config = JsonUtility.FromJson<Config>(configJson);
            configData = config;

            if(config.name != ""){
                NotificationManager.instance.PopupNotification("Welcome, " + config.name);
            }

            if(config.memberId != ""){
                UserDataManager.instance.memberID = config.memberId;
            }else{
                NotificationManager.instance.PopupNotification("Member ID not defined");
                //UserDataManager.instance.memberID = "9999990005545751"; // Test if NUll
                UserDataManager.instance.memberID = ""; // Test if NUll
            }
        }
    }
}
