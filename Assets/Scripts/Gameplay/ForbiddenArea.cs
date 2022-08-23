using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForbiddenArea : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D other) {
        if(other.GetComponent<Robber>().isHoldMoneybag){
            //other.GetComponent<ThrowablePhoton>().btnThrowMoneybag.GetComponent<Button>().interactable = false;
            //other.GetComponent<ThrowablePhoton>().btnThrowMoneybag.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(other.GetComponent<Robber>().isHoldMoneybag){
            //other.GetComponent<ThrowablePhoton>().btnThrowMoneybag.GetComponent<Button>().interactable = false;
            //other.GetComponent<ThrowablePhoton>().btnThrowMoneybag.SetActive(true);
        }
    }
}
