using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class P_EndScreen : MonoBehaviour
{
    public Button endScreenleaveBtn;

    void Start(){
        endScreenleaveBtn.onClick.AddListener(delegate{NetworkManager.instance.CancelFindGameOrLeaveRoom();});
    }
}
