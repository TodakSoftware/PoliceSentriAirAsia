using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class ItemBox : MonoBehaviourPunCallbacks
{
    public GameObject itemBoxIcon;
    public bool doneAdd;
    public List<string> robberItems, policeItems;
    
    void Start()
    {
        //if(photonView.IsMine){
            Invoke("PlayIdleDelay", 1f);
        //}
        //itemBoxIcon.transform.DOMoveY((transform.position.y + -0.15f), .8f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnTriggerEnter2D(Collider2D other) {
        if(!doneAdd){
        //DOTween.KillAll(true);
        if(other.CompareTag("Robber")){
            //print("Pick it up");
            var random = Random.Range(0, robberItems.Count);
            
            if(!other.GetComponent<Robber>().isBot){
                other.GetComponent<PlayerAbilities>().EnableItem(robberItems[random]);

                if(photonView.IsMine){
                    HideAppearance();
                    if(photonView.IsMine){
                        PhotonNetwork.Destroy(GetComponent<PhotonView>());
                    }
                }else{
                    HideAppearance();
                    if(gameObject != null)
                        Destroy(gameObject);
                }
            }else{
                if(gameObject != null)
                    Destroy(gameObject);
            }

            }else if(other.CompareTag("Police")){
                var random = Random.Range(0, policeItems.Count);
                if(!other.GetComponent<Police>().isBot){
                    other.GetComponent<PlayerAbilities>().EnableItem(policeItems[random]);

                    if(photonView.IsMine){
                        HideAppearance();
                        if(photonView.IsMine){
                            PhotonNetwork.Destroy(GetComponent<PhotonView>());
                        }
                    }else{
                        HideAppearance();
                        if(gameObject != null)
                            Destroy(gameObject);
                    }
                }else{
                    if(gameObject != null)
                        Destroy(gameObject);
                }

                
            }
            doneAdd = true;
        } // end doneAdd
    }

    [PunRPC]
    public void DestroyMe(){
        Destroy(gameObject);
    }

    public void HideAppearance(){
        GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void PlayIdleDelay(){
        GetComponent<Animator>().SetBool("Idle", true);
    }
}
