using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Robber : MonoBehaviourPunCallbacks
{
    public bool isHoldMoneybag, isCaught;
    public GameObject moneybagDisplay;
    public GameObject jailCollider;

    [Header("Released Related")]
    public bool isReleasing;
    public float releasedTimer, releasedDuration = 3f;
    public GameObject teammateGO;
    private string teammateName;
    public GameObject releaseBarCanvas;
    public Image releaseBarSlider;

    [Header("Caught Related")]
    public GameObject gotchaUI;
    public GameObject bustedUI;

    void OnTriggerEnter2D(Collider2D other) {
        // If we(are in jail), collide with other non caught robber, onRelease start
        if(isCaught && other.gameObject.CompareTag("Robber")){
            if(other.gameObject.GetComponent<Robber>().isCaught == false){ // our team mates who not caught
                if(photonView.IsMine){
                    photonView.RPC("SetIsReleasing", RpcTarget.All, true);
                    photonView.RPC("SetTeammateName", RpcTarget.All, other.gameObject.GetPhotonView().Owner.NickName);
                    teammateGO = other.gameObject; // Set our saviour
                }
            }
        }
    } // end OnTriggerEnter2D()

    [PunRPC]
    public void SetTeammateName(string memberName){
        teammateName = memberName;
    }

    void OnTriggerExit2D(Collider2D other) {
        // If we are still isReleasing, but teammates go outside range, cancel
        if(isCaught && isReleasing && other.gameObject.CompareTag("Robber")){
            if(photonView.IsMine){
                photonView.RPC("SetIsReleasing", RpcTarget.All, false);
                teammateGO = null;
            }
        }
    } // end OnTriggerExit2D()

    void Update(){
        if(isCaught && isReleasing){
            if(releasedTimer <= 0){
                releaseBarCanvas.SetActive(false);
                releasedTimer = 0;
                photonView.RPC("HasBeenReleased", RpcTarget.All); // Release From Jail
                isReleasing = false;
            }else{
                releasedTimer -= Time.deltaTime;
                releaseBarSlider.fillAmount = releasedTimer / releasedDuration;
            }
        }
    } // end Update()
    
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
        
    }

    [PunRPC]
    public void DisplayMoneybag(bool show){ // Called by Moneybag (OnTriggerEnter2D)
        if(show){
            moneybagDisplay.SetActive(true);
            isHoldMoneybag = true;

            if(PhotonNetwork.IsMasterClient){
                GameManager.instance.DestroyAllMoneybag(); // if someone already pickup, destroy all others moneybag
            }

            GameManager.instance.photonView.RPC("SetMoneybagOccupied", RpcTarget.All, true);
            UIManager.instance.NotificationPickupMoneybag(photonView.Owner.NickName);
            
        }else{
            moneybagDisplay.SetActive(false);
            isHoldMoneybag = false;
        }
    } // end DisplayMoneybag()

#region CAUGHT RELATED
    [PunRPC]
    public void HasBeenCaught(string policeName){
        // Update properties PlayerCaught -> true
        if(!isCaught){
            photonView.RPC("SetIsCaught", RpcTarget.AllBuffered, true, policeName);
            StartCoroutine(GetComponent<PlayerController>().PlayerFall(1.5f)); // Fall
            photonView.RPC("DisableCollider", RpcTarget.All, true); // Disable Collider
            // Popup Caugh Image

            Hashtable updateData = new Hashtable();
            updateData.Add("PlayerCaught", true); // Set PlayerCaught -> TRUE
            PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);

            // Redirect to jail with delay 2s
            print("Redirecting to jailed");
            StartCoroutine(RedirectToJailed(1.5f));
        }
    } // end HasBeenCaught()

    [PunRPC]
    public void SetIsCaught(bool caught, string policeName){
        if(caught){
            isCaught = true;
            GetComponent<PlayerController>().playerNameText.color = Color.red;
            StartCoroutine(PopupGotchaBustedUI(false)); // popup busted UI

            UIManager.instance.NotificationPoliceCapture(policeName, photonView.Owner.NickName); // Popup Notification that police caught ourself
        }else{
            isCaught = false;
            GetComponent<PlayerController>().playerNameText.color = Color.white;

            UIManager.instance.NotificationReleasedBy(photonView.Owner.NickName, teammateName); // Popup Notification that police caught ourself
        }
    }

    IEnumerator RedirectToJailed(float delay){
        yield return new WaitForSeconds(delay);
        transform.position = new Vector3(GameManager.instance.jailSpawnpoint.position.x + Random.Range(0,2f), GameManager.instance.jailSpawnpoint.position.y + Random.Range(0,2f), GameManager.instance.jailSpawnpoint.position.z);
        if(photonView.IsMine){
            Hashtable updateData = new Hashtable();
            if(isHoldMoneybag){ // if we are holding moneybag
                updateData.Add("PlayerHoldMoneybag", false); // Set PlayerHoldMoneybag -> FALSE *will auto respawn new moneybag
                
                GameManager.instance.photonView.RPC("SpawnAllMoneybag", RpcTarget.MasterClient); 
                GameManager.instance.photonView.RPC("SetMoneybagOccupied", RpcTarget.All, false);
            }
            PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);

            photonView.RPC("DisplayMoneybag", RpcTarget.All, false); // Hide moneybag
            photonView.RPC("DisableCollider", RpcTarget.All, false); // Enable Collider
            photonView.RPC("EnableJailCollider", RpcTarget.All, true); // Enable Jail COllider for released
        }
    } // end RedirectToJailed()
#endregion // end region CAUGHT RELATED

#region RELEASED RELATED
    public void ShowReleaseBar(bool show){
        if(show){
            if(!releaseBarCanvas.activeSelf){
                releaseBarCanvas.SetActive(true);
                releaseBarSlider.fillAmount = 1f; // full 
            }
        }else{
            if(releaseBarCanvas.activeSelf){
                releaseBarCanvas.SetActive(false);
            }
        }
    } // end ShowReleaseBar

    [PunRPC]
    public void HasBeenReleased(){
        if(photonView.IsMine && isCaught){
            photonView.RPC("SetIsCaught", RpcTarget.AllBuffered, false, ""); // Passing empty "" because dont need
            photonView.RPC("SetIsReleasing", RpcTarget.AllBuffered, false);
            // Popup Caugh Image

            Hashtable updateData = new Hashtable();
            updateData.Add("PlayerCaught", false); // Set PlayerCaught -> FALSE
            PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);

            print("Get out of jailed");
            
            GetOutOfJailed(teammateGO.transform.position);
            teammateGO = null; // Clear existing saviour
        }
        
    } // end HasBeenReleased()

    public void GetOutOfJailed(Vector3 teammatePosition){
        transform.position = teammatePosition;
        if(photonView.IsMine){
            photonView.RPC("EnableJailCollider", RpcTarget.All, false); // Enable Jail COllider for released
        }
    } // end RedirectToJailed()

    [PunRPC]
    public void SetIsReleasing(bool isRelease){
        if(isRelease){
            isReleasing = true; // set is currentl rescued by teammates
            releasedTimer = releasedDuration; // set current releasedTimer = releaseDuration
            ShowReleaseBar(true);
        }else{
            isReleasing = false; // set is currentl rescued by teammates
            ShowReleaseBar(false);
        }
    } // end SetIsReleasing()

#endregion // end region RELEASED RELATED

#region COLLIDER RELATED
    [PunRPC]
    public void EnableJailCollider(bool isEnable){
        if(isEnable){
            jailCollider.SetActive(true);
        }else{
            jailCollider.SetActive(false);
        }
    } // end EnableJailCollider()

    [PunRPC]
    public void DisableCollider(bool isDisable){
        if(isDisable){
            foreach(Collider2D col in transform.GetComponentsInChildren<Collider2D>()){
                col.enabled = false;
            }
        }else{
            foreach(Collider2D col in transform.GetComponentsInChildren<Collider2D>()){
                col.enabled = true;
            }
        }
    } // end DisableCollider()
#endregion // end region COLLIDER RELATED

}