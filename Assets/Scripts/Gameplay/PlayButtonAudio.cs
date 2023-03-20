using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButtonAudio : MonoBehaviour
{
    public void PlayButtonSound(){
        AudioManager.instance.PlaySound("PS_UI_Button_Click");
    }
}
