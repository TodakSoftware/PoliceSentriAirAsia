using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Vehicle : MonoBehaviourPunCallbacks
{/*
    //public bool cannotStun;

    void OnTriggerEnter2D(Collider2D other) {
        //if(!cannotStun){
            if(other.gameObject.CompareTag("Police") || other.gameObject.CompareTag("Robber")){
                if(!other.gameObject.GetComponent<PlayerControllerPhoton>().isFalling){
                    other.gameObject.GetComponent<PlayerControllerPhoton>().photonView.RPC("PlayerFall", RpcTarget.AllBuffered, 3f);
                    //cannotStun = true;
                    //Invoke("DelayReset", 4f);
                }
            }
        //}
        
    }

    //public void DelayReset(){
    //    cannotStun = false;
    //}*/
}
