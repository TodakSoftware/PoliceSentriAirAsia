using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_GameplayFeeds : MonoBehaviour
{
    public void SetText(string teks){
        GetComponent<TextMeshProUGUI>().text = teks;
        GetComponent<CanvasGroup>().DOFade(0f, .5f).SetDelay(3f).OnComplete(() => { Destroy(gameObject); });
    }

    public void SetTextPoliceCapture(string policeName, string robberName){
        GetComponent<TextMeshProUGUI>().text = policeName + " has capture " + robberName;
        GetComponent<CanvasGroup>().DOFade(0f, .5f).SetDelay(3f).OnComplete(() => { Destroy(gameObject); });
    }

    public void SetTextPickupMoneybag(string robberName){
        GetComponent<TextMeshProUGUI>().text = robberName + " has pickup the moneybag!";
        GetComponent<CanvasGroup>().DOFade(0f, .5f).SetDelay(3f).OnComplete(() => { Destroy(gameObject); });
    }

    public void SetTextReleasedBy(string robberName, string teammateName){
        GetComponent<TextMeshProUGUI>().text = robberName + " has been released by " + teammateName;
        GetComponent<CanvasGroup>().DOFade(0f, .5f).SetDelay(3f).OnComplete(() => { Destroy(gameObject); });
    }
}
