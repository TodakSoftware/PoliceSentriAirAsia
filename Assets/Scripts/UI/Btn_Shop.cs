using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Btn_Shop : MonoBehaviour
{
    public Shop shopRef;
    public int btnIndex;
    public Image icon;
    public GameObject lockIcon;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener( delegate{
            ButtonSelected();
        } );
    }

    public void ButtonSelected(){
        shopRef.SetPreview(btnIndex);
        GetComponent<Image>().color = Color.red;
    }

    public void ButtonUnselected(){
        GetComponent<Image>().color = Color.white;
    }
}
