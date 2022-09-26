using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Moneybag : MonoBehaviourPunCallbacks
{
    bool isPickup;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Robber") && !GameManager.instance.moneyBagOccupied && !isPickup){
            GetComponent<CircleCollider2D>().enabled = false;
            isPickup = true;
            // Set robber properties to notify others that they are holding the moneybag
            if(!other.gameObject.GetComponent<Robber>().isBot){ // if not bot, do as normal
                Hashtable updateData = new Hashtable();
                updateData.Add("PlayerHoldMoneybag", true);
                other.gameObject.GetPhotonView().Owner.SetCustomProperties(updateData);
            }else{
                other.gameObject.GetComponent<Robber>().isHoldMoneybag = true;
            }
            
            // Display moneybag at robber's back
            other.gameObject.GetComponent<Robber>().photonView.RPC("DisplayMoneybag", RpcTarget.All, true);
        }
    } // end OnTriggerEnter2D

    public void Thrown(){

    }
}
