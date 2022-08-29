using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerRank : MonoBehaviourPunCallbacks
{
    public List<Sprite> spriteList;
    public GameObject rankGO;
    public int rankLevel = 0;

    [PunRPC]
    public void PromoteRank(){
        
        rankGO.SetActive(true);
        if(rankLevel < 4){
            rankLevel += 1;

            if(photonView.IsMine){
                //AudioManager.instance.PlaySound("PS_UI_LevelUp");
                // Add Player Statistic
                //GetComponent<PlayerStatisticPhoton>().veryCoolPromotion += 1;
                //GetComponent<PlayerStatisticPhoton>().photonView.RPC("SetRank", RpcTarget.All, rankLevel);
            }
            
            rankGO.GetComponent<Image>().sprite = spriteList[rankLevel - 1];
            switch(rankLevel){
                case 1:
                // Tambah speed
                //BASE_MOVESPEED += 1f;
                GetComponent<PlayerController>().moveSpeed += .5f;
                break;

                case 2:
                // Tambah Sprint Distance
                //dashDistance += 100f;
                GetComponent<PlayerController>().dashDuration += .1f;
                break;

                case 3:
                // Reduce sprint cooldown
                //dashCooldown = dashCooldown / 2f;
                GetComponent<PlayerController>().dashCooldown = GetComponent<PlayerController>().dashCooldown / 2f;
                break;

                case 4:
                // Tambah speed
                //BASE_MOVESPEED += 2f;
                GetComponent<PlayerController>().moveSpeed += 1;
                break;
            }
        }
    } // end promote rank
}