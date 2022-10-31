using UnityEngine;
using TMPro;
using System.Collections;

public class AutoDestroyGO : MonoBehaviour
{
    public float duration;
    public TextMeshProUGUI textGO;

    void OnEnable(){
        StartCoroutine(DestroyGO());
    }

    IEnumerator DestroyGO(){
        var remaining = duration;
        while(remaining > 0){
            textGO.text = "Auto Close in (" + remaining +")";
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        if(remaining <= 0){
            Destroy(gameObject);
        }
    }
}
