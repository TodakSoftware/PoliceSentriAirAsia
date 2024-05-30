using UnityEngine;
using UnityEngine.UI;

public class GPTIntegration : MonoBehaviour
{
    public static GPTIntegration instance;
    /* public Button showButton; // Reference to the UI button
    public Button hideButton; // Reference to the UI button */

    void Awake(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }
    }

   /*  void Start()
    {
        if (showButton != null)
        {
            showButton.onClick.AddListener(TriggerAd);
        }

        if (hideButton != null)
        {
            hideButton.onClick.AddListener(RemoveAd);
        }
    } */

    public void TriggerAd()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("showDiv");
            /* showButton.gameObject.SetActive(false);
            hideButton.gameObject.SetActive(true); */
        #endif
    }
    
    public void RemoveAd()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("hideDiv");
            /* showButton.gameObject.SetActive(true);
            hideButton.gameObject.SetActive(false); */
        #endif
    }
}