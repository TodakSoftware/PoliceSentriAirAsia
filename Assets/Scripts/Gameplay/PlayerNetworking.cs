// Handling player networking related. Example, if the player is not ours, then ignore the scripts
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworking : MonoBehaviourPunCallbacks
{
    public PlayerController playerController;
    public List<MonoBehaviour> scriptsToIgnore = new List<MonoBehaviour>();

    void Start()
    {
        if(!photonView.IsMine){

            foreach(var script in scriptsToIgnore){
                script.enabled = false;
            }
        }
    }
}
