using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreateRoom : MonoBehaviourPunCallbacks
{
    public SO_Maps mapSO;
    public TMP_InputField roomNameInput;
    [Header("Players Count")]
    [SerializeField] int currentCountIndex;
    [SerializeField] byte playerCountBytes;
    public List<int> playerCount = new List<int>();
    public Button playerLeftBtn;
    public TextMeshProUGUI playerCountText;
    public Button playerRightBtn;

    [Header("Map")]
    [SerializeField] int currentMapIndex;
    [SerializeField] string mapName;
    public Button mapLeftBtn;
    public TextMeshProUGUI mapText;
    public Button mapRightBtn;

    [Header("Others")]
    public Button createButton;
    public TextMeshProUGUI createText;

    bool isCreating;

    void Start(){
        if(roomNameInput.text == ""){
            roomNameInput.text = "My Room Name";
        }
        playerCountText.SetText(playerCount[0].ToString());
        playerCountBytes = (byte)playerCount[0];
        playerLeftBtn.interactable = false;

        mapText.SetText(mapSO.mapsList[0].displayName.ToString());
        mapName = mapSO.mapsList[0].name;
        mapLeftBtn.interactable = false;

        createText.SetText("Create");
    }

    public void IncreasePlayerCount(bool _add){
        if(!isCreating){
            if(_add){
                if(currentCountIndex < (playerCount.Count - 1)){
                    currentCountIndex += 1;
                    playerLeftBtn.interactable = true;

                    if(currentCountIndex >= (playerCount.Count - 1)){
                        playerRightBtn.interactable = false;
                    }
                }
            }else{
                if(currentCountIndex > 0){
                    currentCountIndex -= 1;
                    playerRightBtn.interactable = true;

                    if(currentCountIndex <= 0){
                        playerLeftBtn.interactable = false;
                    }
                }
            }
            playerCountText.SetText(playerCount[currentCountIndex].ToString());
            playerCountBytes = (byte)playerCount[currentCountIndex];
        }
    }

    public void IncreaseMapCount(bool _add){
        if(!isCreating){
            if(_add){
                if(currentMapIndex < (mapSO.mapsList.Count - 1)){
                    currentMapIndex += 1;
                    mapLeftBtn.interactable = true;

                    if(currentMapIndex >= (mapSO.mapsList.Count - 1)){
                        mapRightBtn.interactable = false;
                    }
                }
            }else{
                if(currentMapIndex > 0){
                    currentMapIndex -= 1;
                    mapRightBtn.interactable = true;

                    if(currentMapIndex <= 0){
                        mapLeftBtn.interactable = false;
                    }
                }
            }
            mapText.SetText(mapSO.mapsList[currentMapIndex].displayName.ToString());
            mapName = mapSO.mapsList[currentMapIndex].name;
        }
    }

    public void CreatePrivateRoom(){
        if(!isCreating){
            isCreating = true;

            roomNameInput.interactable = false;

            print("Create Room");
            createText.SetText("Creating...");
            createText.color = Color.grey;
            createButton.interactable = false;

            Invoke("ResetCreateButton", 5f);

            if (PhotonNetwork.InRoom)
            {
                Debug.Log("Already In the room");
                return;
            }

            if (roomNameInput.text == ""){
                roomNameInput.text = "My Room Name";
            }
                
            PhotonNetwork.CreateRoom(GetRandomMatchID(), CreateRoomOptions(), null);

            }
    }

    public static string GetRandomMatchID()
    {
        string _id = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            int random = Random.Range(0, 36);
            if (random < 26)
            {
                _id += (char)(random + 65);
            }
            else
            {
                _id += (random - 26).ToString();
            }
        }
        return _id;
    }

    RoomOptions CreateRoomOptions()
    {
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = playerCountBytes;//(byte)maxPlayer;
        ro.IsVisible = false;
        ro.CustomRoomProperties = new Hashtable();
        ro.CustomRoomProperties.Add("rName", roomNameInput.text.ToString());
        ro.CustomRoomProperties.Add("ver", PhotonNetwork.GameVersion);
        //ro.CustomRoomProperties.Add("roomDisplayName", roomName);
        ro.CustomRoomProperties.Add("mapScene", mapName);
        ro.CustomRoomProperties.Add("RoomPrivate", 1);
        //ro.CustomRoomProperties.Add("mapSceneName", mapSceneName);
        // to show on lists.. otherwise won't display
        //ro.CustomRoomPropertiesForLobby = new string[5] { "roomDisplayName", "rName", "ver", "gameMode", "mapSceneName" };
        return ro;
    }

    void ResetCreateButton(){
        createText.SetText("Create");
        createText.color = Color.white;
        createButton.interactable = true;

        roomNameInput.interactable = true;

        isCreating = false;
    }


    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        // GABAN EDIT START
            PhotonNetwork.LoadLevel(PhotonNetwork.CurrentRoom.CustomProperties["mapScene"].ToString());
            print("Successfully Created");
        // GABAN EDIT END
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }
}
