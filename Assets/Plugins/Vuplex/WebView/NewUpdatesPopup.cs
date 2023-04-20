using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
struct SalesData{
    public bool Enable;
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

    [Space(20f)]

    [TextArea(1,5)] string jsonDataURL = "https://api.npoint.io/62786692354ad23a6f8a";
    public bool isAlreadyCheckedForUpdates = false;
    SalesData latestSalesData;

    [System.Obsolete]
    void Awake(){
        instance = this;
        if(!skipUpdate){
            StartCoroutine(CheckForUpdates());
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
                    ShowPopup(latestSalesData.Title, latestSalesData.Message, latestSalesData.LinkURL);
                }
            }else{
                //Debug.Log(request.error);
                print("Cannot Received JSON from NEWSUPDATES");
            }
        }

        request.Dispose();
    }

    void ShowPopup(string _title, string _message, string _link){ // forceUpdate = Yes / No

        titleText.SetText(_title);
        messageText.SetText(_message);

        okBtn.onClick.AddListener(() => {
            HidePopup();
        });

        if(_link != ""){
            openLinkBtn.gameObject.SetActive(true);
            openLinkBtn.onClick.AddListener(() => {
                OpenLinkURL(_link);
            });
        }else{
            openLinkBtn.gameObject.SetActive(false);
        }

        updatePanelGO.SetActive(true);
    }

    void HidePopup(){
        updatePanelGO.SetActive(false);

        okBtn.onClick.RemoveAllListeners();
    }

    void OpenLinkURL(string _linkURL){
        Application.OpenURL(_linkURL);
    }

    void OnDestroy(){
        StopAllCoroutines();
    }
}
