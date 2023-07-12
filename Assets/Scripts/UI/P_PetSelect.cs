using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class P_PetSelect : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI characterNameText;
    public Image characterIdleImage;
    public string selectedCharacter;
    public List<Btn_PetSelectAvatar> allBtnCharacters = new List<Btn_PetSelectAvatar>();
    public Transform listContent;
    public Button selectBtn;

    public void InitializePet(){ // Called by Join Button (Select Role) after selecting robber / police
        //allBtnCharacters.Clear(); // clear any existing
        ResetAllCharacterSelections();

        selectBtn.onClick.AddListener(delegate{SelectSelectedPet();}); // Link select button with function

        foreach(var petChar in SOManager.instance.animVariantPet.animatorLists){
            var charAvatar = Instantiate(SOManager.instance.prefabs.btnPetSelectAvatar, Vector3.zero, Quaternion.identity);
            charAvatar.GetComponent<Btn_PetSelectAvatar>().iconHead.sprite = petChar.iconHead;
            charAvatar.GetComponent<Btn_PetSelectAvatar>().btnCode = petChar.code;
            charAvatar.GetComponent<Btn_PetSelectAvatar>().charName = petChar.name;
            charAvatar.GetComponent<Btn_PetSelectAvatar>().charIdleSprite = petChar.idlePose;
            charAvatar.transform.SetParent(listContent, false);

            if(petChar.type == E_ValueType.PREMIUM && !UserDataManager.instance.petList.Contains(petChar.code)){
                charAvatar.GetComponent<Btn_PetSelectAvatar>().LockCharacter();
            }

            allBtnCharacters.Add(charAvatar.GetComponent<Btn_PetSelectAvatar>());
        }

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

    public void SelectSelectedPet(){ // Assigned into Select Button in select character
    print("Seleceted Pet " + selectedCharacter);
    GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().photonView.RPC("UpdatePetIndex", RpcTarget.AllBuffered, GetPetIndex(selectedCharacter));
    GameManager.instance.ownedPlayerGO.GetComponent<PlayerController>().photonView.RPC("ChangePetAnim", RpcTarget.AllBuffered, GetPetIndex(selectedCharacter), IsPetFlyType(selectedCharacter));
    } // end ChangeCharacter

    public int GetPetIndex(string _code){
        int indexCount = 0;
        foreach(var pet in SOManager.instance.animVariantPet.animatorLists){
            if(pet.code == _code){
                return indexCount;
            }
            indexCount += 1;
        }

        return indexCount;
    }

    public bool IsPetFlyType(string _code){
        foreach(var pet in SOManager.instance.animVariantPet.animatorLists){
            if(pet.code == _code){
                return pet.flyPet;
            }
        }

        return false;
    }

    // --------------------------------- CHANGE SKIN START -----------------------------------------

    // --------------------------------- CHANGE SKIN END -----------------------------------------
}
