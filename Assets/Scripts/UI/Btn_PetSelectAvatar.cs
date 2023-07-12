using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn_PetSelectAvatar : MonoBehaviour
{
    public Image iconHead;
    public string btnCode, charName;
    public Sprite charIdleSprite;
    public GameObject lockIcon;

    public void ButtonPressed(){
        UIManager.instance.p_PetSelect.ClearAllButtonColor();
        GetComponent<Image>().color = Color.red;
        UIManager.instance.p_PetSelect.selectedCharacter = btnCode;
        UIManager.instance.p_PetSelect.characterNameText.text = charName;
        UIManager.instance.p_PetSelect.characterIdleImage.sprite = charIdleSprite;
    }

    public void ClearButton(){
        GetComponent<Image>().color = Color.white;
    }

    public void LockCharacter(){
        lockIcon.SetActive(true);
        GetComponent<Button>().interactable = false;
    }
}
