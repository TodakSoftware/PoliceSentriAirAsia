using UnityEngine;

public class HideShowAdsEnabler : MonoBehaviour
{
    public bool showOnEnable;
    public bool hideOnEnable;
    public bool hideOnDisable;

    private void OnEnable() {
        #if UNITY_WEBGL && !UNITY_EDITOR
            if(showOnEnable){
                GPTIntegration.instance.TriggerAd();
            }

            if(hideOnEnable){
                GPTIntegration.instance.RemoveAd();
            }
        #endif
    }

     private void OnDisable() {
        #if UNITY_WEBGL && !UNITY_EDITOR
            if(hideOnDisable){
                GPTIntegration.instance.RemoveAd();
            }
        #endif
    }
}
