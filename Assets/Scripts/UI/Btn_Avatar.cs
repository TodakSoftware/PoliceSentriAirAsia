using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class Btn_Avatar : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public Image slotBG, playerIcon;
    public GameObject caughtGO, moneybagGO;
    public bool isBot;

    [Header("Team Related")]
    public Sprite policeSlotBG;
    public Sprite robberSlotBG;
    public int actorNumber;
    public GameObject ownerOfThisAvatarGO;

    public void SetupButton(string team, string playerNickname, string characterCode, GameObject actorGO, bool _isBot){ // Called by GameUI (CreateAvatar) 
        isBot = _isBot;
        switch(team){
            case "Police":
                /* if(ownerOfThisAvatarGO.GetComponent<Police>().isBot){
                    characterCode = "P001";
                } */
                slotBG.sprite = policeSlotBG;
                playerName.text = playerNickname;
                ownerOfThisAvatarGO = actorGO;

                foreach(var c in SOManager.instance.animVariantPolice.animatorLists){
                    if(c.code == characterCode){
                        playerIcon.sprite = c.iconHead;
                        break;
                    }
                }
            break;

            case "Robber":
                /* if(ownerOfThisAvatarGO.GetComponent<Robber>().isBot){
                    characterCode = "R001";
                } */

                slotBG.sprite = robberSlotBG;
                playerName.text = playerNickname;
                ownerOfThisAvatarGO = actorGO;

                foreach(var c in SOManager.instance.animVariantRobber.animatorLists){
                    if(c.code == characterCode){
                        playerIcon.sprite = c.iconHead;
                        break;
                    }
                }
            break;
        } // end switch
    }

    public void UpdateButton(string team, string CharCode, bool isCaught, bool holdMoneybag){ // Called everytime by GameManager(UpdateAvatarsUI) when player custom properties changed
        switch(team){
            case "Police":
                if(ownerOfThisAvatarGO.GetComponent<Police>().isBot){
                    CharCode = "P001";
                }else{
                    CharCode = ownerOfThisAvatarGO.GetComponent<PlayerController>().characterCode;
                }
                
                foreach(var b in SOManager.instance.animVariantPolice.animatorLists){
                    if(b.code == CharCode){
                        playerIcon.sprite = b.iconHead; // Setup New Icon
                        /* if(isCaught){ // Setup Jailed Icon
                            playerName.color = Color.red;
                            caughtGO.SetActive(true);
                        }else{
                            playerName.color = Color.white;
                            caughtGO.SetActive(false);
                        } */

                        if(holdMoneybag){ // Setup Moneybag Icon
                            moneybagGO.SetActive(true);
                        }else{
                            moneybagGO.SetActive(false);
                        }
                    }
                    //break;
                }

                
            break;

            case "Robber":
                if(ownerOfThisAvatarGO.GetComponent<Robber>().isBot){
                    CharCode = "R001";
                }else{
                    CharCode = ownerOfThisAvatarGO.GetComponent<PlayerController>().characterCode;
                }
                foreach(var c in SOManager.instance.animVariantRobber.animatorLists){
                    if(c.code == CharCode){
                        playerIcon.sprite = c.iconHead; // Setup New Icon

                        if(ownerOfThisAvatarGO.GetComponent<Robber>().isCaught){ // Setup Jailed Icon
                            playerName.color = Color.red;
                            caughtGO.SetActive(true);
                        }else{
                            playerName.color = Color.white;
                            caughtGO.SetActive(false);
                        }

                        if(holdMoneybag && ownerOfThisAvatarGO.GetComponent<Robber>().isHoldMoneybag){ // Setup Moneybag Icon
                            moneybagGO.SetActive(true);
                        }else{
                            moneybagGO.SetActive(false);
                        }
                        //break;
                    }
                }

                if(ownerOfThisAvatarGO.GetComponent<Robber>().isCaught){ // Setup Jailed Icon
                    playerName.color = Color.red;
                    caughtGO.SetActive(true);
                }else{
                    playerName.color = Color.white;
                    caughtGO.SetActive(false);
                }
            break;
        } // end switch
    }
}
