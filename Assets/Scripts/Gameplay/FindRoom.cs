using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class FindRoom : MonoBehaviourPunCallbacks
{
    public TMP_InputField findCodeInput;
    public Button findBtn;

    void Start()
    {
        findBtn.onClick.AddListener(() => {
            JoinCustomRoom();
        });
    }

    public void JoinCustomRoom()
    {
        if (findCodeInput.text != ""){
            PhotonNetwork.JoinRoom(findCodeInput.text.ToUpper(), null);
        }else{
            //print("Room name not set!");
            NotificationManager.instance.PopupNotification("Room code not set!");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        //print("JoinRandoFailed");
        NotificationManager.instance.PopupNotification(message);
    }


}
