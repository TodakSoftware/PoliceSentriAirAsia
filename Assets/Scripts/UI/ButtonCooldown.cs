using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonCooldown : MonoBehaviour
{
    public Image imageSlider;

    public void StartCooldown(float duration){
        GetComponent<Button>().interactable = false;
        imageSlider.fillAmount = 1;
        imageSlider.DOFillAmount(0, duration).OnComplete(() => { GetComponent<Button>().interactable = true; });
    }
}
