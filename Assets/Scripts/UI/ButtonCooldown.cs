using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonCooldown : MonoBehaviour, ISelectHandler , IPointerEnterHandler
{
    public Image imageSlider;

    public void StartCooldown(float duration){
        GetComponent<Button>().interactable = false;
        imageSlider.fillAmount = 1;
        imageSlider.DOFillAmount(0, duration).OnComplete(() => { GetComponent<Button>().interactable = true; });
    }

    // When highlighted with mouse.
     public void OnPointerEnter(PointerEventData eventData)
     {
        GetComponent<Button>().onClick.Invoke();
     }
     // When selected. 
     public void OnSelect(BaseEventData eventData)
     {
        GetComponent<Button>().onClick.Invoke();
     }
}
