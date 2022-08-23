using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class PropsAnimation : MonoBehaviourPunCallbacks
{
    public float duration;
    private float timer;
    private bool timeEnd;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine){
            timer = duration;
            transform.localScale = Vector3.zero;
            //transform.DOScale(1,1f).SetEase(Ease.OutBounce);
        }
        GetComponent<Animator>().SetBool("Popup",true);
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){
            if(timer <= 0 && !timeEnd){
                timer = 0;
                
                // play dissolve anim
                GetComponent<Animator>().SetBool("Popup",false);
                GetComponent<Animator>().SetBool("Shrink",true);
                //transform.DOScale(0,1f).SetEase(Ease.InBounce).OnComplete(()=>{
                    //transform.DOKill(true);
                    
                   // DOTween.KillAll(true);
                    if(gameObject != null){
                        Destroy(GetComponent<Collider2D>());
                        Invoke("DestroyMe", 1f);
                    }
                //});
            timeEnd = true;
            }else{
                timer -= Time.deltaTime;
            }
        }
    }

    public void DestroyMe(){
        if(photonView.IsMine){
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }
    }
}
