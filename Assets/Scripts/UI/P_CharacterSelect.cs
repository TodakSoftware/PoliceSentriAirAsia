using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class P_CharacterSelect : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI characterNameText;
    public Image characterIdleImage;
    public string selectedCharacter;
    public List<Btn_CharacterSelectAvatar> allBtnCharacters = new List<Btn_CharacterSelectAvatar>();
    public Transform listContent;
    public Button selectBtn;

    public void InitializeAllCharacters(string team){ // Called by Join Button (Select Role) after selecting robber / police
        allBtnCharacters.Clear(); // clear any existing

        switch(team){
            case "Police":
                selectBtn.onClick.AddListener(delegate{SelectSelectedCharacter("Police");}); // Link select button with function

                foreach(var policeChar in SOManager.instance.animVariantPolice.animatorLists){
                    var charAvatar = Instantiate(SOManager.instance.prefabs.btnCharacterSelectAvatar, Vector3.zero, Quaternion.identity);
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().iconHead.sprite = policeChar.iconHead;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().btnCode = policeChar.code;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().charName = policeChar.name;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().charIdleSprite = policeChar.idlePose;
                    charAvatar.transform.SetParent(listContent, false);

                    allBtnCharacters.Add(charAvatar.GetComponent<Btn_CharacterSelectAvatar>());
                }
            break;

            case "Robber":
                selectBtn.onClick.AddListener(delegate{SelectSelectedCharacter("Robber");}); // Link select button with function

                foreach(var robberChar in SOManager.instance.animVariantRobber.animatorLists){
                    var charAvatar = Instantiate(SOManager.instance.prefabs.btnCharacterSelectAvatar, Vector3.zero, Quaternion.identity);
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().iconHead.sprite = robberChar.iconHead;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().btnCode = robberChar.code;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().charName = robberChar.name;
                    charAvatar.GetComponent<Btn_CharacterSelectAvatar>().charIdleSprite = robberChar.idlePose;
                    charAvatar.transform.SetParent(listContent, false);

                    allBtnCharacters.Add(charAvatar.GetComponent<Btn_CharacterSelectAvatar>());
                }
            break;

            default:
                print("Unknown Team");
            break;
        } // end switch

        // Auto Select 1st One
        if(selectedCharacter == ""){
            allBtnCharacters[0].ButtonPressed();
        }else{
            foreach(var b in allBtnCharacters){
                if(b.btnCode == selectedCharacter){
                    b.ButtonPressed();
                }
            }
        }
        
    } // end InitializeAllCharacters

    public void DisplayChangeCharacter(){ // Called when we pressing Change Character while in lobby
        if(allBtnCharacters.Count > 0){
             // Auto Select 1st One
            if(selectedCharacter == ""){
                allBtnCharacters[0].ButtonPressed();
            }else{
                foreach(var b in allBtnCharacters){
                    if(b.btnCode == selectedCharacter){
                        b.ButtonPressed();
                    }
                }
            }
        } // end if(allBtnCharacters.Count > 0){
    } // end DisplayChangeCharacter()

    void ResetAllCharacterSelections(){ // This called when player switch teams
        allBtnCharacters.Clear();

        if(listContent.childCount > 0){
            foreach(Transform go in listContent){
                Destroy(go.gameObject);
            }
        }
    }

    public void ClearAllButtonColor(){ // this called everytime we press button on select character
        foreach(var btn in allBtnCharacters){
            btn.ClearButton();
        }
    } // ClearAllButtonColor

    public void SelectSelectedCharacter(string team){ // Assigned into Select Button in select character
        switch(team){
            case "Police":
                GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Police", selectedCharacter);
            break;

            case "Robber":
                GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Robber", selectedCharacter);
            break;

            default:
            break;
        }
    } // end ChangeCharacter

    // --------------------------------- CHANGE SKIN START -----------------------------------------

    // --------------------------------- CHANGE SKIN END -----------------------------------------
}
