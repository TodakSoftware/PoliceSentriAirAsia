// For handling network related

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    DefaultPool pool;

    [Header("Connection Related")]
    [SerializeField] bool autoConnect = true; // Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.
    bool connectInUpdate = true; // if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()
    public bool isDisconnectedWhileGame; // status for checking if player disconnected while in game. Can be used for reconnecting & rejoin game. (Destroy Reconnect UI when successfully reconnect)
    public bool hasInternet;
    // AIRASIA RELATED END

    [Header("Find Game Related")]
    [HideInInspector] public float findGameTimeoutDuration; // [Controlled by SO_GameSettings]
    [HideInInspector] public int maxPolicePerGame, maxRobberPerGame; // [Controlled by SO_GameSettings]
    byte maxPlayersPerRoom; // max player in a room (Addition of maxHumanPerGame & maxGhostPerGame)
    bool isFindingGame; // Active when we are searching for games.
    public bool isQueing, doneQueing;
    public float queingTimer, queingCooldown = 5f;

    [Header("Ingame Related")]
    [Tooltip("0 : Police & Robber (Normal Mode)")]
    int gameModeIndex = 0; // default = 0
    public bool isInGame, isInTheRoom, isCreatingRoom,dontConnectInternet, joinedLobby, playOfflineGame;
    

    void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
        
    }

    void Start(){
        if(!dontConnectInternet){
            CheckForInternet(); // Check for internet
        }
        
        pool = PhotonNetwork.PrefabPool as DefaultPool;
        InitPrefabPooling(); // Start object pooling for photon gameobjects

        // Linking data via Game Settings Scriptable Objects
        findGameTimeoutDuration = SOManager.instance.gameSettings.gameMode[gameModeIndex].findGameTimeoutDuration;
        maxPolicePerGame = SOManager.instance.gameSettings.gameMode[gameModeIndex].maxPolicePerGame;
        maxRobberPerGame = SOManager.instance.gameSettings.gameMode[gameModeIndex].maxRobberPerGame;
        maxPlayersPerRoom = (byte)(maxPolicePerGame + maxRobberPerGame); // Set maxPlayersPerRoom for photon references
    }

    void Update(){
        if(!dontConnectInternet && !playOfflineGame){
            CheckForInternet();
            // Reason putting in Update() & not Start() because to make sure we keep checking if we are not connected
            Connectz();
        }

        if(PhotonNetwork.IsConnectedAndReady && hasInternet && joinedLobby && !dontConnectInternet && UIManager.instance.p_MainMenu != null){
            
                if(!UIManager.instance.p_MainMenu.playButton.interactable)
                UIManager.instance.p_MainMenu.playButton.interactable = true;
        }
        

        if(Input.GetKeyDown(KeyCode.H)){ // Temporary, just for debugging
            
        }

        if(Input.GetKeyDown(KeyCode.J)){ // Temporary, just for debugging
           
        }

        if(Input.GetKeyDown(KeyCode.Space)){ // Temporary, just for debugging
            
        }

        if(isQueing && queingTimer < queingCooldown && !isInTheRoom && !doneQueing){
            StartCoroutine(QueingFindRoom());
            doneQueing = true;
        }
        
    } // end Update

    public void Connectz(){
        if(connectInUpdate && autoConnect && !PhotonNetwork.IsConnected && hasInternet){
            connectInUpdate = false; // run only once
            if(UIManager.instance.p_MainMenu != null){
                UIManager.instance.p_MainMenu.playButton.interactable = false;
                UIManager.instance.p_MainMenu.playText.text = "CONNECTING...";
            }
            PhotonNetwork.OfflineMode = false;
            
            PhotonNetwork.ConnectUsingSettings(); // Connect to master server using settings | Noted: ConnectUsingSettings("v0.0.1") <-- Also can
            
        }
    }

    IEnumerator QueingFindRoom(){
        var timer = 0;
        while(timer < queingCooldown && !isInTheRoom){
            timer += 1;
            queingTimer = timer;   
            yield return new WaitForSeconds(1f);
            //JoinTheGame(0);
        }

        if(timer >= queingCooldown && !isInTheRoom && !isCreatingRoom){
            HostTheRoom();
            isCreatingRoom = true;
        }
    }

    // ----------------------- PREFABS POOLING RELATED START -------------------
    void InitPrefabPooling(){
        AddPrefabPool(SOManager.instance.prefabs.characterPrefabs); // Add Characters Prefabs Lists
        AddPrefabPool(SOManager.instance.prefabs.propsPrefabs); // Add Props Prefabs Lists
        AddPrefabPool(SOManager.instance.prefabs.particlePrefabs); // Add Particle Prefabs Lists
    } // end InitPrefabPooling

    public void AddPrefabPool(List<C_PhotonPrefabAttributes> prefabAttributes)
    {
        if (pool != null && prefabAttributes != null)
        {
            foreach (C_PhotonPrefabAttributes prefabAtt in prefabAttributes)
            {
                if (!pool.ResourceCache.ContainsKey(prefabAtt.name))
                    pool.ResourceCache.Add(prefabAtt.name, prefabAtt.prefabs);
            }
        }
    } // end AddPrefabPool

    public static string GetPhotonPrefab(string category, string prefabName){
        string pref = "";
        List<C_PhotonPrefabAttributes> photonPrefabsList = new List<C_PhotonPrefabAttributes>();
        photonPrefabsList.Clear(); // clear if not empty

        switch(category){
            case "Characters":
                photonPrefabsList = SOManager.instance.prefabs.characterPrefabs;
            break;

            case "Props":
                photonPrefabsList = SOManager.instance.prefabs.propsPrefabs;
            break;

            case "Particles":
                photonPrefabsList = SOManager.instance.prefabs.particlePrefabs;
            break;

            default:
                photonPrefabsList = null;
            break;
        }

        // Search for matching prefabs in the lists
        foreach(C_PhotonPrefabAttributes go in photonPrefabsList){
            if(go.name == prefabName){
                return go.name; // return this value if found
            }
        }

        return pref; // return default pref if not found
        
    } // end GetPhotonPrefab

    // ----------------------- PREFABS POOLING RELATED END -------------------


    // ----------------------- CONNECTION RELATED START -------------------
    public override void OnConnectedToMaster(){
        
        if(PhotonNetwork.IsConnected){
            if(UIManager.instance.p_MainMenu.statusText != null)
                UIManager.instance.p_MainMenu.statusText.text = "Network: <color=yellow>Initializing...</color>";

            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }else{
            print("not yet lobby");
        }
        

        connectInUpdate = true;
    } // end OnConnectedToMaster

    public override void OnJoinedLobby(){
        PhotonNetwork.AutomaticallySyncScene = true; // Enable AutoSyncScene
        joinedLobby = true;

        if(UIManager.instance.p_MainMenu.statusText != null)
                UIManager.instance.p_MainMenu.statusText.text = "Network: <color=green>Connected</color>";

        if(UIManager.instance.p_MainMenu != null){
            UIManager.instance.p_MainMenu.playButton.interactable = true;
            UIManager.instance.p_MainMenu.playText.text = "PLAY";
        }
    } // end OnJoinedLobby
    
    public override void OnJoinedRoom(){ // Only host affected by this
        isInTheRoom = true;

        UpdateTotalFindGame(); // Update total players in room
        if(!isInGame){
            UIManager.instance.activeFindgameCancel(true); // Enable cancel find game button when joined
            UIManager.instance.p_MainMenu.coroutinefindRoomTimeout = StartCoroutine(UIManager.instance.UpdateUI_FindgameTimeout(findGameTimeoutDuration)); // Timeout duration updates
        }
        
    } // end OnJoinedRoom

    public override void OnJoinRandomFailed(short returnCode, string message){ // we create new room with this
        if(!isCreatingRoom){
            Invoke("DelayRejoin", 1.5f);
        }
        
    } // end OnJoinRandomFailed

    void DelayRejoin(){
        print("Rejoin Called");
        Hashtable expectedRoomProperties = new Hashtable();
        expectedRoomProperties["RoomGamemodeIndex"] = 0;

        // Join Random Room With expected properties
        if(!PhotonNetwork.InRoom && hasInternet && queingTimer < queingCooldown){
            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, maxPlayersPerRoom);
        }
    }

    public void HostTheRoom(){
        // If not found, retry rejoin until 5s,
        // NetworkManager.instance.JoinTheGame(0);

        // If already >5s, then we create games
        // #Critical: we failed to join a random room, maybe none exists or they are all full. We create a new room.
        //if(queingTimer >= queingCooldown){
            print("No available room found! Creating...");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.PlayerTtl = -1; // -1 sec for infinite : Duration for player to reconnect before kicked / timeout
            if(PhotonNetwork.OfflineMode){
                roomOptions.MaxPlayers = 3;
            }else{
                roomOptions.MaxPlayers = maxPlayersPerRoom;
            }
            
            roomOptions.EmptyRoomTtl = 0;

            PhotonNetwork.CreateRoom(null, roomOptions);
        //}
    }

    public override void OnCreatedRoom(){ // Create default room properties on 1st created room
        if(PhotonNetwork.IsMasterClient){ // Set room properties after we are in a room
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add("RoomGamemodeIndex", gameModeIndex);
            if(PhotonNetwork.OfflineMode){
                roomProperties.Add("RoomPolicePerGame", 1);
                roomProperties.Add("RoomRobberPerGame", 2);
                roomProperties.Add("RoomMaxTotalPlayer", 3);
            }else{
                roomProperties.Add("RoomPolicePerGame", maxPolicePerGame);
                roomProperties.Add("RoomRobberPerGame", maxRobberPerGame);
                roomProperties.Add("RoomMaxTotalPlayer", (int)maxPlayersPerRoom);
            }
            
            roomProperties.Add("RoomMapName", GetRandomMap()); // Random map
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

            string[] exposedPropertiesInLobby = { "RoomGamemodeIndex", "RoomMaxTotalPlayer", "RoomMapName" }; 
            PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(exposedPropertiesInLobby);
        }
        
        print("Create room : " + PhotonNetwork.CurrentRoom.Name);
    } // end OnCreatedRoom

    public override void OnPlayerEnteredRoom(Player newPlayer){ // When non host player enter a room. "Display / Update UI"
        UpdateTotalFindGame();
    } // end OnPlayerEnteredRoom

    public override void OnPlayerLeftRoom(Player otherPlayer){ // When player cancel find game or after leave a room
        print("Player Left Room");
        if(!isInGame){
            Invoke("UpdateTotalFindGame", 1f);
        }
    } // end OnPlayerLeftRoom

    public override void OnLeftRoom(){ // When player successfully left the room
        print("Player Has Left The Room Completely");
        dontConnectInternet = false;
        if(PhotonNetwork.IsConnected){
            PhotonNetwork.NetworkingClient.State = ClientState.JoinedLobby;
            PhotonNetwork.OfflineMode = false;
            playOfflineGame = false;
            
        }
        
        
        if(!isInGame){ // if we are in main menu
            if(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout != null)
            StopCoroutine(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout); // Stop running courotine

            isInTheRoom = false;
        }else{
            // Redirect to main menu
            isInGame = false;
            isCreatingRoom = false;
            isInTheRoom = false;
            isFindingGame = false; // Set status to isFindingGame
            isQueing = false;
            doneQueing = false;

            PhotonNetwork.LoadLevel("MainMenu");
        }
    } // end OnLeftRoom

    public override void OnDisconnected(DisconnectCause cause){
        print("Error : " + cause);

        if(!isInGame){
            if(Application.internetReachability == NetworkReachability.NotReachable || hasInternet){
                hasInternet = false;
                if(UIManager.instance.p_MainMenu != null){
                    UIManager.instance.p_MainMenu.playButton.interactable = true;
                    UIManager.instance.p_MainMenu.playText.text = "PLAY OFFLINE";
                }
            }
        } // end !isInGame
        
        //DisconnectCause.DisconnectByClientLogic // <--- Player close the game suddenly
        // if disconnect suddenly -> Popup Reconnect UI Prefab
        // if internet not reachable -> loading screen will popup & reconnect
    }

    public void CheckForInternet(){
        if(Application.internetReachability == NetworkReachability.NotReachable){
            hasInternet = false;
            //PhotonNetwork.OfflineMode = true;

            if(UIManager.instance.p_MainMenu != null){
                UIManager.instance.p_MainMenu.playButton.interactable = false;
                UIManager.instance.p_MainMenu.playText.text = "No Network";
            }
        }else{
            hasInternet = true;
            //PhotonNetwork.OfflineMode = false;
            if(UIManager.instance.p_MainMenu != null && joinedLobby){
                UIManager.instance.p_MainMenu.playButton.interactable = true;
                UIManager.instance.p_MainMenu.playText.text = "Play";
            }
        }
    }

    public void SetOffline(){
        //Application.internetReachability = NetworkReachability.NotReachable;
        //PhotonNetwork.Disconnect();
        playOfflineGame = true;
        PhotonNetwork.NetworkingClient.State = ClientState.PeerCreated;
        dontConnectInternet = true;
        //PhotonNetwork.NetworkingClient.Disconnect();
        
        StartCoroutine(CanJoinGame());
    }

    IEnumerator CanJoinGame(){
        while(PhotonNetwork.IsConnected){
            yield return new WaitForSeconds(1f);
            print("retry canjoingame");
            PhotonNetwork.Disconnect();
        }
        
        if(!PhotonNetwork.IsConnected){
            hasInternet = false;
            PhotonNetwork.OfflineMode = true;
            JoinTheGame(0);
        }
        
    }

    // ----------------------- CONNECTION RELATED END -------------------


    // ----------------------- FIND GAME RELATED START -------------------
    void UpdateTotalFindGame(){ // Update room properties when player enters | Start the game here
        if(PhotonNetwork.InRoom){
            // Cache Network Room Variable
            int _roomTotalPlayer = 0;
            string _roomMapName = "";
            
            // Local variable for room count
            int _enteredPlayer = 0;

            if(PhotonNetwork.CurrentRoom.CustomProperties["RoomMaxTotalPlayer"] != null){
                _roomTotalPlayer = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoomMaxTotalPlayer"];
            }else{
                _roomTotalPlayer = maxPlayersPerRoom;
            }
            
            if(PhotonNetwork.CurrentRoom.CustomProperties["RoomMapName"] != null){
                _roomMapName = PhotonNetwork.CurrentRoom.CustomProperties["RoomMapName"].ToString();
            }else{
                _roomMapName = GetRandomMap();
            }

            foreach(var player in PhotonNetwork.CurrentRoom.Players){
                _enteredPlayer++;
            }

            isInTheRoom = true;

            if(PhotonNetwork.OfflineMode){
                if(PhotonNetwork.IsMasterClient){
                    PhotonNetwork.CurrentRoom.IsVisible = false; // Set Room IsVisible = false
                    StartCoroutine(ChangeScene(_roomMapName));// Host load level
                }
            }else{
                // If a room match all requirement, Host responsible to change the scene
                if(_enteredPlayer == _roomTotalPlayer && isFindingGame){ // Only do this when we are finding game
                    print("Matched!");
                    UIManager.instance.PopupLoadingScene(); // Popup Loading Scene UI

                    if(PhotonNetwork.IsMasterClient){
                        PhotonNetwork.CurrentRoom.IsVisible = false; // Set Room IsVisible = false
                        StartCoroutine(ChangeScene(_roomMapName));// Host load level
                    }

                    isFindingGame = false; // Set status to isFindingGame
                } // end _totalHuman == roomTotalMaxHuman
            }
        } // end PhotonNetwork.InRoom

    } // end UpdateTotalFindGame

    public void PlayOnlineGame(){
        PhotonNetwork.OfflineMode = false;
        JoinTheGame(0);
    }
    
    public void JoinTheGame(int modeIndex){ // Used by buttons in ChooseRole Screen
        isInGame = false;
        isCreatingRoom = false;
        isInTheRoom = false;
        isFindingGame = true; // Set status to isFindingGame

        UIManager.instance.timeoutTimer = 0; // Reset timer

        Hashtable expectedRoomProperties = new Hashtable();
        if(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout != null){
            StopCoroutine(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout); // Stop Coroutine when enter game
        }
        
        UIManager.instance.PopupFindGame();

        // ExpectedCustomRoom properties. Example, Human search room that where human is not full
        gameModeIndex = modeIndex;
        expectedRoomProperties["RoomGamemodeIndex"] = modeIndex;

        // Join Random Room With expected properties
        if(!PhotonNetwork.InRoom && hasInternet){
            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, maxPlayersPerRoom);
            if(!isQueing){
                isQueing = true;
                queingTimer = 0;
            }
            UIManager.instance.activeFindgameCancel(false);
            print("HAS INTERNET");
        }else{
            if(!hasInternet){
                print("No Internet, so we add bot, load offline level");
                HostTheRoom();
            }else{
                print("Cannot Join. Because maybe you are in a room....maybe.");
            }
        }

    } // end JoinTeam

    public void CancelFindGameOrLeaveRoom(){ // Cancel while finding game or Leave Room. Used by cancel button in Modal_Findgame
        // Makesure we are in a room
        if(PhotonNetwork.IsConnectedAndReady){
            print("Player cancelling find game / leave room");
            isFindingGame = false; // Set status to isFindingGame
            isInTheRoom = false;
            doneQueing = false;
            isInGame = false;
            isCreatingRoom = false;

            if(isQueing){
                isQueing = false;
                queingTimer = 0;
            }
            PhotonNetwork.LeaveRoom();
        }else{
            print("Not ready yet to cancel or leave.");
        }
    } // end CancelFindGameOrLeaveRoom

    public string GetRandomMap(){
        int randomNumber = Random.Range(0, SOManager.instance.maps.mapsList.Count);
        return SOManager.instance.maps.mapsList[randomNumber].name;
    }

    // ----------------------- FIND GAME RELATED END -------------------

    
    // ----------------------- INGAME RELATED START -------------------
    public void ReconnectToGame(){ // Attached into reconnect button on Reconnect Screen Popup
        if(PhotonNetwork.IsConnectedAndReady){
            PhotonNetwork.ReconnectAndRejoin(); // only works when player doesnt call Photon.LeaveRoom() or disconnect suddenly
        }else{
            print("Not ready to reconnect into the game");
        }
    } // end ReconnectToGame

    public IEnumerator ChangeScene(string sceneName){ // Load level instantly without loading screen
        //if(PhotonNetwork.IsMasterClient){
            yield return new WaitForSeconds(1.3f);
            PhotonNetwork.LoadLevel(sceneName);
        //}
    } // end ChangeScene

    // ----------------------- INGAME RELATED END -------------------
}
