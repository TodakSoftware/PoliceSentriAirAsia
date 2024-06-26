﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Banana : MonoBehaviourPunCallbacks
{
    public Player itemOwner;
    public E_Team ownerTeam;
    public GameObject ownerGO;
    public float dur = 3f;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
             // Send Statistic
            /*if(itemOwner != null && ownerTeam == E_Team.POLICE && other.CompareTag("Robber") && ownerGO != null && ownerGO.GetComponent<PlayerStatisticPhoton>().slipHereSlipThereHit == 0){
                ownerGO.GetComponent<PlayerStatisticPhoton>().photonView.RPC("AddSlipHereSlipThereCount", RpcTarget.AllBuffered, 1);
            }else if(itemOwner != null && ownerTeam == E_Team.ROBBER && other.CompareTag("Police") && ownerGO != null && ownerGO.GetComponent<PlayerStatisticPhoton>().slipHereSlipThereHit == 0){
                ownerGO.GetComponent<PlayerStatisticPhoton>().photonView.RPC("AddSlipHereSlipThereCount", RpcTarget.AllBuffered, 1);
            }*/

            if(other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot && !other.GetComponent<Robber>().isCaught){
                //other.GetComponent<PlayerControllerPhoton>().PlayerFall(dur);
                other.GetComponent<PlayerController>().photonView.RPC("PlayerFall", RpcTarget.AllBuffered, dur);
                other.GetComponent<PlayerController>().FallSound("Banana");
            }else if(other.CompareTag("Police") && !other.GetComponent<Police>().isBot){
                //other.GetComponent<PlayerControllerPhoton>().PlayerFall(dur);
                other.GetComponent<PlayerController>().photonView.RPC("PlayerFall", RpcTarget.AllBuffered, dur);
                other.GetComponent<PlayerController>().FallSound("Banana");
            }else{
                if(other.CompareTag("Robber") && other.GetComponent<Robber>().isBot){
                    other.GetComponent<AIRobber>().photonView.RPC("BotFalling", RpcTarget.AllBuffered, dur);
                }else if(other.CompareTag("Police") && other.GetComponent<Police>().isBot){
                    other.GetComponent<AIPolice>().photonView.RPC("BotFalling", RpcTarget.AllBuffered, dur);
                }
            }
            

            if(photonView.IsMine){
                PhotonNetwork.Destroy(this.gameObject);
            }
        } // End other.tag
    }

    [PunRPC]
    public void DestroyMe(){
        Destroy(gameObject);
    }
}
