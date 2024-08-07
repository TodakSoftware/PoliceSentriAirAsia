using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Modal_FindGame : MonoBehaviour
{
    public TextMeshProUGUI findRoomTimeoutText, findGameInfoText;
    public Button cancelFindGameBtn;
    public Coroutine coroutinefindRoomTimeout; // store coroutine for stopping timer

    void Start(){
        cancelFindGameBtn.onClick.AddListener(delegate{PhotonNetworkManager.instance.CancelFindGameOrLeaveRoom();}); // Assign cancel find game function from networkManager into cancel button
    }
}
