using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Police : MonoBehaviourPunCallbacks
{
    [Header("Caught Related")]
    public GameObject gotchaUI;
    public GameObject bustedUI;
    
    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Robber") && GameManager.instance.gameStarted && !GameManager.instance.gameEnded && photonView.IsMine){
            if(other.gameObject.GetComponent<Robber>().isCaught == false){ // if that robber is !caught, set him to HasBeenCaught
                other.gameObject.GetComponent<Robber>().photonView.RPC("HasBeenCaught", other.gameObject.GetPhotonView().Owner, PhotonNetwork.NickName);
                
                photonView.RPC("PopupGotchaBustedUI", RpcTarget.All, true);
            }
        } // end CompareTag
    } // end OnTriggerEnter2D()

    [PunRPC] // only police needed for PopupBustedUI
    public IEnumerator PopupGotchaBustedUI(bool gotcha){
        if(gotcha){
            gotchaUI.SetActive(true);
            yield return new WaitForSeconds(2f);
            gotchaUI.SetActive(false);
        }else{
            bustedUI.SetActive(true);
            yield return new WaitForSeconds(2f);
            bustedUI.SetActive(false);
        }
    } // end PopupGotchaBustedUI()
}
