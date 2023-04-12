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
        public string avatar;
    }

    public static ConfigReceiver instance;
    public Config configData;

    void Awake(){
        instance = this;
    }

    public void SetConfig(string configJson)
    {
        Config config = JsonUtility.FromJson<Config>(configJson);
        configData = config;

        if(config.name != ""){
            NotificationManager.instance.PopupNotification("Welcome, " + config.name);
        }
    }
}
