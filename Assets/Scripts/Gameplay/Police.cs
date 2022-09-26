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
    public bool isBot;
    
    void OnTriggerEnter2D(Collider2D other){
        if(!isBot){
            if(other.CompareTag("Robber") && GameManager.instance.gameStarted && !GameManager.instance.gameEnded && photonView.IsMine){
                if(other.gameObject.GetComponent<Robber>().isCaught == false){ // if that robber is !caught, set him to HasBeenCaught
                    StartCoroutine(GetComponent<PlayerController>().PauseMovement(.5f));

                    other.gameObject.GetComponent<Robber>().photonView.RPC("HasBeenCaught", other.gameObject.GetPhotonView().Owner, GetComponent<PlayerController>().playerNameText.text.ToString());

                    //Save Police Caught Count
                    if(photonView.IsMine){
                        var _currentCaught = (int)photonView.Owner.CustomProperties["PoliceCaughtCount"] + 1;

                        Hashtable teamRole = new Hashtable();
                        teamRole.Add("PoliceCaughtCount", _currentCaught);
                        PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                        print ("Total Police Caught :" + _currentCaught);
                    }
                    
                    photonView.RPC("PopupGotchaBustedUI", RpcTarget.All, true);
                }
            } // end CompareTag
        }else{ // else if BOT
            if(other.CompareTag("Robber") && GameManager.instance.gameStarted && !GameManager.instance.gameEnded){
                StartCoroutine(GetComponent<AIPolice>().PauseMovement(.5f));

                other.gameObject.GetComponent<Robber>().photonView.RPC("HasBeenCaught", other.gameObject.GetPhotonView().Owner, GetComponent<AIPolice>().playerNameText.text.ToString());

                GetComponent<AIPolice>().caughtCount += 1;

                photonView.RPC("PopupGotchaBustedUI", RpcTarget.All, true);
            }
        } // end !isBot
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
