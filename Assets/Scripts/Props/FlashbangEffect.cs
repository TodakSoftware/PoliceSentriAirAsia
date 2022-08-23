using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FlashbangEffect : MonoBehaviour
{
    public static FlashbangEffect instance;
    public float fadeInDuration, stayDuration, fadeOutDuration, stayTimer;
    private bool explode;
    // Start is called before the first frame update

    void Start()
    {
        instance = this;
       //GetComponent<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if(explode){
            if(stayTimer <= 0){
                stayTimer = 0;
                GetComponent<Image>().DOFade(0, fadeOutDuration);
                explode = false;
            }else{
                stayTimer -= Time.deltaTime;
            }
        }
    }

    public void Explode(){
        stayTimer = stayDuration;
        //AudioManager.instance.PlaySound("PS_Flashbang");
        if(!explode){
            var temp = GetComponent<Image>().color;
            temp.a = 0;
            GetComponent<Image>().color = temp;
            GetComponent<Image>().DOFade(1, fadeInDuration);
        }
        explode = true;
    }
}
