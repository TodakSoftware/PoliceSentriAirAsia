using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallSync : MonoBehaviourPunCallbacks
{
    private void OnCollisionEnter2D(Collision2D col) {
        if(col.gameObject.tag == "Police" || col.gameObject.tag == "Robber") {
            this.photonView.TransferOwnership(col.gameObject.GetPhotonView().Owner);
        }
    }
}
