using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;
    public GameObject notificationGO;
    public TextMeshProUGUI notiText;

    void Awake()
    {
        if(instance == null){ instance = this; }
    }

    public void PopupNotification(string _text){
        DOTween.KillAll();
        notiText.SetText(_text);
        notificationGO.GetComponent<CanvasGroup>().alpha = 0;
        notificationGO.SetActive(true);
        notificationGO.GetComponent<CanvasGroup>().DOFade(1f, .2f).OnComplete(() => {
            notificationGO.GetComponent<CanvasGroup>().DOFade(0f, .2f).SetDelay(2f).OnComplete(() => {
                notificationGO.SetActive(false);
            });
        });
    }
}
