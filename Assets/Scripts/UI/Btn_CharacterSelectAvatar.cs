using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn_CharacterSelectAvatar : MonoBehaviour
{
    public Image iconHead;
    public string btnCode, charName;
    public Sprite charIdleSprite;

    public void ButtonPressed(){
        UIManager.instance.p_CharacterSelect.ClearAllButtonColor();
        GetComponent<Image>().color = Color.red;
        UIManager.instance.p_CharacterSelect.selectedCharacter = btnCode;
        UIManager.instance.p_CharacterSelect.characterNameText.text = charName;
        UIManager.instance.p_CharacterSelect.characterIdleImage.sprite = charIdleSprite;
    }

    public void ClearButton(){
        GetComponent<Image>().color = Color.white;
    }
}
