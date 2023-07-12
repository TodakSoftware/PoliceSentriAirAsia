using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UI_ShopTab : MonoBehaviour
{
    [HideInInspector]public Shop shopRef;
    public int index; // set auto by Shop.cs
    public TextMeshProUGUI tabText;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener( delegate{
            shopRef.OpenShopTab(index, 0);
        } );
    }

    public void ButtonSelected(){
        tabText.color = Color.yellow;
        GetComponent<RectTransform>().DOAnchorPosY(-20,.3f);
       
    }

    public void ButtonDeselected(){
        tabText.color = Color.white;
        GetComponent<RectTransform>().DOAnchorPosY(0,.3f);
    }
}
