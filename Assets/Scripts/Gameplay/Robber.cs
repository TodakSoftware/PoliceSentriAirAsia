using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Robber : MonoBehaviourPunCallbacks
{
    public bool isHoldMoneybag, isCaught, isInPrison, done;
    public GameObject moneybagDisplay;
    public GameObject jailCollider;
    public bool readyToGoToEscape; // for bot only

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

    public Player savior;
    public bool isBot;

    void OnTriggerEnter2D(Collider2D other) {
            // If we(are in jail), collide with other non caught robber, onRelease start
            if(isCaught && other.gameObject.CompareTag("Robber")){
                if(other.gameObject.GetComponent<Robber>().isCaught == false){ // our team mates who not caught
                    if(photonView.IsMine){
                        photonView.RPC("SetIsReleasing", RpcTarget.All, true);
                        if(!other.gameObject.GetComponent<Robber>().isBot){
                            photonView.RPC("SetTeammateName", RpcTarget.All, other.gameObject.GetComponent<PlayerController>().playerNameText.text); 
                        }else{
                            photonView.RPC("SetTeammateName", RpcTarget.All, other.gameObject.GetComponent<AIRobber>().playerNameText.text); 
                        }
                        
                        
                        savior = other.gameObject.GetPhotonView().Controller;
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
        //if(!isBot){
            // If we are still isReleasing, but teammates go outside range, cancel
            if(isCaught && isReleasing && other.gameObject.CompareTag("Robber")){
                if(photonView.IsMine){
                    photonView.RPC("SetIsReleasing", RpcTarget.All, false);
                    teammateGO = null;
                }
            }
        //}
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
            if(!isBot){
                UIManager.instance.NotificationPickupMoneybag(GetComponent<PlayerController>().playerNameText.text);
            }else{
                UIManager.instance.NotificationPickupMoneybag(GetComponent<AIRobber>().playerNameText.text);
            }
            
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
            AudioManager.instance.PlaySound("PS_UI_Caught");
            
            if(!isBot){
                photonView.RPC("SetIsCaught", RpcTarget.AllBuffered, true, policeName);
                StartCoroutine(GetComponent<PlayerController>().PlayerFall(1.5f)); // Fall
                photonView.RPC("DisableCollider", RpcTarget.All, true); // Disable Collider
                // Popup Caugh Image

                Hashtable updateData = new Hashtable();
                updateData.Add("PlayerCaught", true); // Set PlayerCaught -> TRUE
                PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);
            }else{
                photonView.RPC("SetIsCaught", RpcTarget.AllBuffered, true, policeName);
                StartCoroutine(GetComponent<AIRobber>().BotFalling(1.5f)); // Fall
                photonView.RPC("DisableCollider", RpcTarget.All, true); // Disable Collider
                // Popup Caugh Image
            }
            

            // Redirect to jail with delay 2s
            print("Redirecting to jailed");
            StartCoroutine(RedirectToJailed(1.5f));
        }
    } // end HasBeenCaught()

    [PunRPC]
    public void AddReleasedCount(int value)
    {
        //Save Police Caught Count
       if(!isBot){
            var _currentReleased = (int)photonView.Controller.CustomProperties["RobberReleasedCount"] + value;

            Hashtable teamRole = new Hashtable();
            teamRole.Add("RobberReleasedCount", _currentReleased);
            PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
       }else{
            GetComponent<AIRobber>().releaseCount += 1;
       }
        
       
    }

    [PunRPC]
    public void SetIsCaught(bool caught, string policeName){
        if(caught){
            isCaught = true;

            if(GetComponent<AIRobber>() != null){
                StartCoroutine(GetComponent<AIRobber>().BotFalling(1.5f));
            }

            if(!isBot){
                GetComponent<PlayerController>().playerNameText.color = Color.red;
            }else{
                GetComponent<AIRobber>().playerNameText.color = Color.red;
            }
            
            StartCoroutine(PopupGotchaBustedUI(false)); // popup busted UI

            if(!isBot){
                UIManager.instance.NotificationPoliceCapture(policeName, GetComponent<PlayerController>().playerNameText.text.ToString()); // Popup Notification that police caught ourself
            }else{
                UIManager.instance.NotificationPoliceCapture(policeName, GetComponent<AIRobber>().playerNameText.text.ToString()); // Popup Notification that police caught ourself
            }

        }else{
            isCaught = false;
            if(!isBot){
                GetComponent<PlayerController>().playerNameText.color = Color.white;

                if(teammateName != null)
                UIManager.instance.NotificationReleasedBy(GetComponent<PlayerController>().playerNameText.text.ToString(), teammateName); // Popup Notification that police caught ourself
            }else{
                GetComponent<AIRobber>().playerNameText.color = Color.white;

                if(teammateName != null)
                UIManager.instance.NotificationReleasedBy(GetComponent<AIRobber>().playerNameText.text.ToString(), teammateName); // Popup Notification that police caught ourself
            }
        }
    }

    IEnumerator RedirectToJailed(float delay){
        yield return new WaitForSeconds(delay);
        transform.position = new Vector3(GameManager.instance.jailSpawnpoint.position.x + Random.Range(0,2f), GameManager.instance.jailSpawnpoint.position.y + Random.Range(0,2f), GameManager.instance.jailSpawnpoint.position.z);
        isInPrison = true;
        readyToGoToEscape = true;

        if(isBot){
            GetComponent<AIRobber>().GoInsidePrison();

            if(isHoldMoneybag){ // if we are holding moneybag
                GameManager.instance.photonView.RPC("SpawnAllMoneybag", RpcTarget.MasterClient); 
                GameManager.instance.photonView.RPC("SetMoneybagOccupied", RpcTarget.All, false);
            }

            photonView.RPC("DisplayMoneybag", RpcTarget.All, false); // Hide moneybag
            photonView.RPC("DisableCollider", RpcTarget.All, false); // Enable Collider
            photonView.RPC("EnableJailCollider", RpcTarget.All, true); // Enable Jail COllider for released
        }

        if(photonView.IsMine && !isBot){
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

                if(!isBot){
                    Hashtable updateData = new Hashtable();
                    updateData.Add("PlayerCaught", false); // Set PlayerCaught -> FALSE
                    PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);
                }

                print("Get out of jailed");
                
                GetOutOfJailed(teammateGO.transform.position);
                teammateGO = null; // Clear existing saviour

                isInPrison = false;
                done = false;
                readyToGoToEscape = false;

                if (savior != null){
                    photonView.RPC("AddReleasedCount", savior, 1); 
                } else {
                    print("NOT WORKING");
                }
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

    [PunRPC]
    public void PicklockReleased(string robberName, E_EscapeArea jailArea, Vector3 location){
        // Notify global robber has been released
        if(photonView.IsMine && isCaught){

        photonView.RPC("SetIsCaught", RpcTarget.AllBuffered, false, ""); // Passing empty "" because dont need
        //photonView.RPC("SetIsReleasing", RpcTarget.AllBuffered, false);
        Hashtable updateData = new Hashtable();
        updateData.Add("PlayerCaught", false); // Set PlayerCaught -> FALSE
        PhotonNetwork.LocalPlayer.SetCustomProperties(updateData);

        print("Get out of jailed with lockpick");

        switch(jailArea){
            case E_EscapeArea.TOP:
                transform.position = location + new Vector3(0, 3f, 0);
            break;

            case E_EscapeArea.RIGHT:
                transform.position = location + new Vector3(3f, 0, 0);
            break;

            case E_EscapeArea.BOTTOM:
                transform.position = location + new Vector3(0, -3f, 0);
            break;

            case E_EscapeArea.LEFT:
                transform.position = location + new Vector3(-3f, 0, 0);
            break;
        }

        photonView.RPC("EnableJailCollider", RpcTarget.All, false); // Enable Jail COllider for released
        isInPrison = false;
        done = false;
        readyToGoToEscape = false;

        UIManager.instance.NotificationReleasedByLockpick(GetComponent<PlayerController>().playerNameText.text.ToString());
        }
    }

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