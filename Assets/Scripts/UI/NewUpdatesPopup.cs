using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Runtime.InteropServices;

[System.Serializable]
struct SalesData{
    public bool Enable;
    public bool ForceUpdate;
    public bool ShowUpdateButton;
    public string Version;
    public string Title;
    public string Message;
    public string LinkURL;
}

public class NewUpdatesPopup : MonoBehaviour
{
    public static NewUpdatesPopup instance;
    public bool skipUpdate;
    [SerializeField] GameObject updatePanelGO;
    [SerializeField] Button okBtn;
    [SerializeField] Button openLinkBtn;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI messageText;
    public AudioManager audioMgr;

    [Space(20f)]

    [TextArea(1,5)] string jsonDataURL = "https://api.npoint.io/161e3b4659c7aa9a08c0";
    public bool isAlreadyCheckedForUpdates = false;
    SalesData latestSalesData;

    [System.Obsolete]
    void Awake(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
            if(!skipUpdate && !isAlreadyCheckedForUpdates){
            StartCoroutine(CheckForUpdates());
        }
        }else{
            Destroy(gameObject);
        }
        
    }

    [System.Obsolete]
    IEnumerator CheckForUpdates(){
        yield return new WaitForSeconds(1f);
        UnityWebRequest request = UnityWebRequest.Get(jsonDataURL);
        request.timeout = 60;

        yield return request.SendWebRequest();

        if(request.isDone){
            //isAlreadyCheckedForUpdates = true;

            if(!request.isNetworkError){
                latestSalesData = JsonUtility.FromJson<SalesData>(request.downloadHandler.text);

                if(latestSalesData.Enable){
                    if(latestSalesData.Version != Application.version){
                        ShowPopup(latestSalesData.Title, latestSalesData.Message, latestSalesData.LinkURL);
                    }

                    if(latestSalesData.ForceUpdate){
                        okBtn.gameObject.SetActive(false);
                    }else{
                        okBtn.gameObject.SetActive(true);
                    }

                    if(latestSalesData.ShowUpdateButton){
                        openLinkBtn.gameObject.SetActive(true);
                    }else{
                        openLinkBtn.gameObject.SetActive(false);
                    }
                }

                isAlreadyCheckedForUpdates = true;
            }else{
                //Debug.Log(request.error);
                print("Cannot Received JSON from NEWSUPDATES");
            }
        }

        request.Dispose();
    }

    [System.Obsolete]
    void ShowPopup(string _title, string _message, string _link){ // forceUpdate = Yes / No

        titleText.SetText(_title);
        messageText.SetText(_message);

        okBtn.onClick.AddListener(() => {
            HidePopup();
        });

        /* if(_link != ""){
            openLinkBtn.gameObject.SetActive(true);
            openLinkBtn.onClick.AddListener(() => {
                OpenLinkURL(_link);
            });
        }else{
            openLinkBtn.gameObject.SetActive(false);
        } */
        
        openLinkBtn.onClick.AddListener(() => {
            OpenAirasiaLink();
        });

        updatePanelGO.SetActive(true);
        //AudioManager.instance.PlaySound("PS_UI_Popup_Valid");
    }

    void HidePopup(){
        updatePanelGO.SetActive(false);

        okBtn.onClick.RemoveAllListeners();
    }

    void OpenLinkURL(string _linkURL){
        //Application.OpenURL(_linkURL);
    }

    [System.Obsolete]
    void OpenAirasiaLink(){
        // Tell index .html
        Application.ExternalCall("goToUpdatePage");
    }

    void OnDestroy(){
        StopAllCoroutines();
    }
}
