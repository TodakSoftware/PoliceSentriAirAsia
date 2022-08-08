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

    [Header("Team Related")]
    public Sprite policeSlotBG;
    public Sprite robberSlotBG;
    public string goID;

    public void SetupButton(string team, string playerNickname, string characterCode, string actorNmbr){ // Called by GameUI (CreateAvatar) 
        switch(team){
            case "Police":
                slotBG.sprite = policeSlotBG;
                playerName.text = playerNickname;
                goID = actorNmbr;
                foreach(var c in SOManager.instance.animVariantPolice.animatorLists){
                    if(c.code == characterCode){
                        playerIcon.sprite = c.iconHead;
                    }
                }
            break;

            case "Robber":
                slotBG.sprite = robberSlotBG;
                playerName.text = playerNickname;
                goID = actorNmbr;
                foreach(var c in SOManager.instance.animVariantRobber.animatorLists){
                    if(c.code == characterCode){
                        playerIcon.sprite = c.iconHead;
                    }
                }
            break;

            default:
                print("unknown");
            break;
        } // end switch
    }

    public void UpdateButton(string team, string CharCode, bool isCaught, bool holdMoneybag){ // Called everytime by GameManager(UpdateAvatarsUI) when player custom properties changed
        print("Update Button: isCaught:" + isCaught + " | isHoldMoney:" + holdMoneybag);
        switch(team){
            case "Police":
                foreach(var c in SOManager.instance.animVariantPolice.animatorLists){
                    if(c.code == CharCode){
                        playerIcon.sprite = c.iconHead; // Setup New Icon
                        
                        if(isCaught){ // Setup Jailed Icon
                            playerName.color = Color.red;
                            caughtGO.SetActive(true);
                        }else{
                            playerName.color = Color.white;
                            caughtGO.SetActive(false);
                        }

                        if(holdMoneybag){ // Setup Moneybag Icon
                            moneybagGO.SetActive(true);
                        }else{
                            moneybagGO.SetActive(false);
                        }
                    }
                }
            break;

            case "Robber":
                foreach(var c in SOManager.instance.animVariantRobber.animatorLists){
                    if(c.code == CharCode){
                        playerIcon.sprite = c.iconHead; // Setup New Icon

                        if(isCaught){ // Setup Jailed Icon
                            playerName.color = Color.red;
                            caughtGO.SetActive(true);
                        }else{
                            playerName.color = Color.white;
                            caughtGO.SetActive(false);
                        }

                        if(holdMoneybag){ // Setup Moneybag Icon
                            moneybagGO.SetActive(true);
                        }else{
                            moneybagGO.SetActive(false);
                        }
                    }
                }
            break;

            default:
                print("unknown");
            break;
        } // end switch
    }
}
