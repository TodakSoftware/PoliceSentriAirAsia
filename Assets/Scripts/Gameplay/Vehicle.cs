﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Vehicle : MonoBehaviourPunCallbacks
{
    //public bool cannotStun;

    void OnTriggerEnter2D(Collider2D other) {
        //if(!cannotStun){
            if(other.gameObject.CompareTag("Police") || other.gameObject.CompareTag("Robber") && GameManager.instance.gameStarted){
                if(!other.gameObject.GetComponent<PlayerController>().isFalling){
                    other.gameObject.GetComponent<PlayerController>().photonView.RPC("PlayerFall", RpcTarget.All, 3f);
                    //cannotStun = true;
                    //Invoke("DelayReset", 4f);
                }
            }
        //}
        
    }

    //public void DelayReset(){
    //    cannotStun = false;
    //}
}
