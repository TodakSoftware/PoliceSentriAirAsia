using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemLockpick : MonoBehaviour
{
    public E_EscapeArea area;
    public bool someOneInArea = false;


    void OnTriggerStay2D(Collider2D other) {
        if(other.CompareTag("RobberJailCollider")){
            if(other.GetComponent<JailCollider>().robber.isCaught && other.transform.parent.GetComponent<PlayerAbilities>().hasLockpick && GameManager.instance.gameStarted && !GameManager.instance.gameEnded){ // & has picklock
                switch(area){
                    case E_EscapeArea.TOP:
                        other.GetComponent<JailCollider>().playerAbilities.escapeArea = E_EscapeArea.TOP;
                    break;

                    case E_EscapeArea.RIGHT:
                        other.GetComponent<JailCollider>().playerAbilities.escapeArea = E_EscapeArea.RIGHT;
                    break;

                    case E_EscapeArea.BOTTOM:
                        other.GetComponent<JailCollider>().playerAbilities.escapeArea = E_EscapeArea.BOTTOM;
                    break;

                    case E_EscapeArea.LEFT:
                        other.GetComponent<JailCollider>().playerAbilities.escapeArea = E_EscapeArea.LEFT;
                    break;
                }

                other.GetComponent<JailCollider>().playerController.gameUI.itemButton.GetComponent<Button>().interactable = true;
                someOneInArea = true;
            }else{
                if(!other.GetComponent<JailCollider>().robber.isCaught && other.GetComponent<JailCollider>().playerAbilities.hasLockpick){
                    other.GetComponent<JailCollider>().playerController.gameUI.itemButton.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(someOneInArea){
            if(other.CompareTag("Robber") && other.GetComponent<JailCollider>().robber.isCaught && other.GetComponent<JailCollider>().playerAbilities.hasLockpick && GameManager.instance.gameStarted && !GameManager.instance.gameEnded){
                other.GetComponent<JailCollider>().playerController.gameUI.itemButton.GetComponent<Button>().interactable = false;
            }
            someOneInArea = false;
        }
    }

}
