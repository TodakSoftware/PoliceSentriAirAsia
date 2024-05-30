// For handling network related

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager instance;
    DefaultPool pool;

    [Header("Connection Related")]
    [SerializeField] bool autoConnect = true; // Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.
    bool connectInUpdate = true; // if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()
    public bool isDisconnectedWhileGame; // status for checking if player disconnected while in game. Can be used for reconnecting & rejoin game. (Destroy Reconnect UI when successfully reconnect)
    public bool hasInternet;
    // AIRASIA RELATED END

    [Header("Find Game Related")]
    public float findGameTimeoutDuration, findGameAutoStart; // [Controlled by SO_GameSettings]
    [HideInInspector] public int maxPolicePerGame, maxRobberPerGame; // [Controlled by SO_GameSettings]
    byte maxPlayersPerRoom; // max player in a room (Addition of maxHumanPerGame & maxGhostPerGame)
    public bool isFindingGame, autoStartCreateGame; // Active when we are searching for games.
    public bool isQueing, doneQueing;
    public float queingTimer, queingCooldown = 5f;

    [Header("Ingame Related")]
    [Tooltip("0 : Police & Robber (Normal Mode)")]
    int gameModeIndex = 0; // default = 0
    public bool isInGame, isInTheRoom, isCreatingRoom,dontConnectInternet, joinedLobby, playOfflineGame;
    public bool playAgainExecuted, instantCreateFindGame;
    private LoadBalancingClient loadBalancingClient;
    public P_MainMenu cacheP_mainMenu;
    public bool offlineMode;

    void Awake(){
        cacheP_mainMenu = GameObject.FindGameObjectWithTag("P_MainMenu").GetComponent<P_MainMenu>();
        if(instance == null){
            instance = this;

            if(PlayerPrefs.HasKey("PlayAgain") && PlayerPrefs.GetInt("PlayAgain") == 1){
                PlayerPrefs.SetInt("PlayAgain", 0);
                PlayerPrefs.DeleteKey("PlayAgain");
                PlayerPrefs.Save();
            }
        }else{
            //Destroy(instance.gameObject);

            /*if(PlayerPrefs.HasKey("PlayAgain") && PlayerPrefs.GetInt("PlayAgain") == 1){
                playAgainEnable = true;
            }*/

            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;

        if(offlineMode){
            autoConnect = false;
            hasInternet = false;
            dontConnectInternet = true;
            playOfflineGame = true;
        }
    }

    public bool GetRandomBool(){
        var ran = Random.Range(0, 2);
        if(ran != 0){
            return true;
        }else{
            return false;
        }
    }

    void Start(){
        loadBalancingClient = new LoadBalancingClient();

        if(!dontConnectInternet){
            CheckForInternet(); // Check for internet
        }
        
        pool = PhotonNetwork.PrefabPool as DefaultPool;
        InitPrefabPooling(); // Start object pooling for photon gameobjects

        // Linking data via Game Settings Scriptable Objects
        findGameTimeoutDuration = SOManager.instance.gameSettings.gameMode[gameModeIndex].findGameTimeoutDuration;
        findGameAutoStart = SOManager.instance.gameSettings.gameMode[gameModeIndex].findGameAutoStart;
        maxPolicePerGame = SOManager.instance.gameSettings.gameMode[gameModeIndex].maxPolicePerGame;
        maxRobberPerGame = SOManager.instance.gameSettings.gameMode[gameModeIndex].maxRobberPerGame;
        maxPlayersPerRoom = (byte)(maxPolicePerGame + maxRobberPerGame); // Set maxPlayersPerRoom for photon references
    }

    void FindGameAutoStartAdder(float addAmount){
        if(UIManager.instance.timeoutTimer > findGameAutoStart){
            findGameAutoStart += ((UIManager.instance.timeoutTimer - findGameAutoStart) + addAmount);
        }else{
            findGameAutoStart += addAmount;
        }
        
    }

    void Update(){
        if(!dontConnectInternet && !playOfflineGame && !isInGame){
            CheckForInternet();
            // Reason putting in Update() & not Start() because to make sure we keep checking if we are not connected
            Connectz();
        }

        /* if(!isInGame){
            if(PhotonNetwork.IsConnectedAndReady && hasInternet && joinedLobby && !dontConnectInternet && UIManager.instance.p_MainMenu != null){
                if(UIManager.instance.p_MainMenu.playButton != null && !UIManager.instance.p_MainMenu.playButton.interactable)
                    UIManager.instance.p_MainMenu.playButton.interactable = true;
            }else{
                if(UIManager.instance.p_MainMenu.playButton != null && UIManager.instance.p_MainMenu.playButton.interactable)
                    UIManager.instance.p_MainMenu.playButton.interactable = false;
            }
        } */

        if(isQueing && queingTimer < queingCooldown && !isInTheRoom && !doneQueing){
            StartCoroutine(QueingFindRoom());
            doneQueing = true;
        }

        // Auto start if 1 minit passed
        if(!offlineMode && hasInternet && isInTheRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 1 && PhotonNetwork.CurrentRoom.PlayerCount < (int)maxPlayersPerRoom && UIManager.instance.timeoutTimer >= findGameAutoStart && !autoStartCreateGame){ // if we in room & timer find game >= autostart
            
            if(PhotonNetwork.IsMasterClient){
                //print("Auto Start Game With Current Players = " + PhotonNetwork.CurrentRoom.PlayerCount);
                HostWithCustomPlayer(PhotonNetwork.CurrentRoom.PlayerCount);

                PhotonNetwork.CurrentRoom.IsVisible = false; // Set Room IsVisible = false
                //StartCoroutine(ChangeScene(GetRandomMapBias("Map_Bali", 70)));// Host load level
                List<string> mapsLists = new List<string>();
                mapsLists.Add("Map_Bali");
                mapsLists.Add("Map_Bangkok01");
                mapsLists.Add("Map_Boracay01");
                mapsLists.Add("Map_Singapore");
                StartCoroutine(ChangeScene(GetRandomMapBiasLists(mapsLists, 70)));// Host load level

                isFindingGame = false; // Set status to isFindingGame

                autoStartCreateGame = true;
            }
        }else{
        }

        // Play Again 
        if(!playAgainExecuted && !isInGame && !isInTheRoom && PlayerPrefs.HasKey("PlayAgain") && PlayerPrefs.GetInt("PlayAgain") == 1 && PhotonNetwork.IsConnectedAndReady){ //PlayerPrefs.HasKey("PlayAgain") && PlayerPrefs.GetInt("PlayAgain") == 1
            print("Can Rejoin Any Game");
            PlayOnlineGame();
            playAgainExecuted = true;
        }
    } // end Update

    public void Connectz(){
        
        if(connectInUpdate && autoConnect && !PhotonNetwork.IsConnected && hasInternet){
            connectInUpdate = false; // run only once
            
            if(cacheP_mainMenu != null){
                /* cacheP_mainMenu.playButton.gameObject.SetActive(true);
                cacheP_mainMenu.playWithBotButton.gameObject.SetActive(false); */
                
                cacheP_mainMenu.playButton.interactable = false;
                cacheP_mainMenu.privateButton.interactable = false;
                cacheP_mainMenu.playText.text = "CONNECTING...";
                cacheP_mainMenu.playText.color = Color.white;
                
                if(cacheP_mainMenu.statusText != null)
                cacheP_mainMenu.statusText.text = "Network: CONNECTING...";
            }
            
            PhotonNetwork.ConnectUsingSettings(); // Connect to master server using settings | Noted: ConnectUsingSettings("v0.0.1") <-- Also can
        }else{
            if(!hasInternet){
                if(cacheP_mainMenu != null){
                   /*  cacheP_mainMenu.playButton.gameObject.SetActive(false);
                    cacheP_mainMenu.playWithBotButton.gameObject.SetActive(true); */
                    cacheP_mainMenu.playText.text = "Internet Unavailable";
                    cacheP_mainMenu.playText.color = Color.gray;
                    
                    if(cacheP_mainMenu.statusText != null)
                    cacheP_mainMenu.statusText.text = "Network: OFFLINE";
                }
            }
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
            if(cacheP_mainMenu.statusText != null)
                cacheP_mainMenu.statusText.text = "Network: <color=yellow>Initializing...</color>";

            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }else{
            print("not yet lobby");
        }
        

        connectInUpdate = true;
    } // end OnConnectedToMaster

    public override void OnJoinedLobby(){
        PhotonNetwork.AutomaticallySyncScene = true; // Enable AutoSyncScene        
        joinedLobby = true;
        loadBalancingClient.LoadBalancingPeer.MaximumTransferUnit = 520;


        if(cacheP_mainMenu.statusText != null)
                cacheP_mainMenu.statusText.text = "Network: <color=green>Connected</color>";

        if(cacheP_mainMenu!= null){
            cacheP_mainMenu.playButton.interactable = true;
            cacheP_mainMenu.privateButton.interactable = true;
            cacheP_mainMenu.playText.text = "PLAY";
        }
    } // end OnJoinedLobby
    
    public override void OnJoinedRoom(){ // Only host affected by this
        isInTheRoom = true;
        if(!PhotonNetwork.IsMessageQueueRunning){
            PhotonNetwork.IsMessageQueueRunning = true;
        }

        UpdateTotalFindGame(); // Update total players in room
        if(!isInGame){
            //UIManager.instance.activeFindgameCancel(true); // Enable cancel find game button when joined
            UIManager.instance.p_MainMenu.coroutinefindRoomTimeout = StartCoroutine(UIManager.instance.UpdateUI_FindgameTimeout(findGameTimeoutDuration)); // Timeout duration updates
        }
        
    } // end OnJoinedRoom

    public override void OnJoinRandomFailed(short returnCode, string message){ // we create new room with this
        if(instantCreateFindGame){
            HostTheRoom();
        }else{
            if(!isCreatingRoom && isQueing){
                Invoke("DelayRejoin", 1.5f);
            }
        }
    } // end OnJoinRandomFailed

    void DelayRejoin(){
        //print("Rejoin Called");
        Hashtable expectedRoomProperties = new Hashtable();
        expectedRoomProperties["RoomGamemodeIndex"] = 0;
        expectedRoomProperties["RoomPrivate"] = 0;
        // Join Random Room With expected properties
        if(!PhotonNetwork.InRoom && hasInternet && queingTimer < queingCooldown && isQueing){
            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, maxPlayersPerRoom);
        }
    } // end delayRejoin

    public void HostTheRoom(){
        // If not found, retry rejoin until 5s,
        // NetworkManager.instance.JoinTheGame(0);

        // If already >5s, then we create games
        // #Critical: we failed to join a random room, maybe none exists or they are all full. We create a new room.
        //if(queingTimer >= queingCooldown){
            print("No available room found! Creating...");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.PlayerTtl = -1; // -1 sec for infinite : Duration for player to reconnect before kicked / timeout <- Punca player kluar room n amount still melekat
            roomOptions.MaxPlayers = maxPlayersPerRoom;
            roomOptions.CustomRoomProperties = new Hashtable();
            roomOptions.CustomRoomProperties.Add("RoomPrivate", 0);
            
            roomOptions.EmptyRoomTtl = 0;

            PhotonNetwork.CreateRoom(null, roomOptions);
        //}
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("Cannot create room: "+ message);
        //StopAllCoroutines();
        PhotonNetwork.Disconnect();

        isInGame = false;
        isCreatingRoom = false;
        isInTheRoom = false;
        isFindingGame = false; // Set status to isFindingGame
        isQueing = false;
        doneQueing = false;

        UIManager.instance.p_MainMenu.findGameGO.SetActive(false);
        UIManager.instance.p_MainMenu.mainAreaGO.SetActive(true);
    }

    public override void OnCreatedRoom(){ // Create default room properties on 1st created room
        if(PhotonNetwork.IsMasterClient){ // Set room properties after we are in a room
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add("RoomGamemodeIndex", gameModeIndex);
            if(PhotonNetwork.OfflineMode){
                roomProperties.Add("RoomPolicePerGame", 4);
                roomProperties.Add("RoomRobberPerGame", 6);
                roomProperties.Add("RoomMaxTotalPlayer", 10);
                roomProperties.Add("RealTotalPlayer", 10);
            }else{
                roomProperties.Add("RoomPolicePerGame", maxPolicePerGame);
                roomProperties.Add("RoomRobberPerGame", maxRobberPerGame);
                roomProperties.Add("RoomMaxTotalPlayer", (int)maxPlayersPerRoom);
                roomProperties.Add("RealTotalPlayer", (int)maxPlayersPerRoom);
            }
            
            //roomProperties.Add("RoomMapName", GetRandomMapBias("Map_Bali", 70)); // Random map
            List<string> mapsLists = new List<string>();
            mapsLists.Add("Map_Bali");
            mapsLists.Add("Map_Bangkok01");
            mapsLists.Add("Map_Boracay01");
            roomProperties.Add("RoomMapName", GetRandomMapBiasLists(mapsLists, 70)); // Random map
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

            string[] exposedPropertiesInLobby = { "RoomGamemodeIndex", "RoomMaxTotalPlayer", "RoomMapName" }; 
            PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(exposedPropertiesInLobby);
        }
        
        print("Create room : " + PhotonNetwork.CurrentRoom.Name);
    } // end OnCreatedRoom

    public void HostWithCustomPlayer(int amount){
        if(PhotonNetwork.IsMasterClient){ // Set room properties after we are in a room
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add("RoomGamemodeIndex", gameModeIndex);

            roomProperties.Add("RoomPolicePerGame", SOManager.instance.gameSettings.gameMode[gameModeIndex].maxPolicePerGame);
            roomProperties.Add("RoomRobberPerGame", SOManager.instance.gameSettings.gameMode[gameModeIndex].maxRobberPerGame);
            roomProperties.Add("RoomMaxTotalPlayer", (int)SOManager.instance.gameSettings.gameMode[gameModeIndex].maxPolicePerGame + (int)SOManager.instance.gameSettings.gameMode[gameModeIndex].maxRobberPerGame);

            roomProperties.Add("RealTotalPlayer", amount);

            /* switch(amount){
                case 2:
                    roomProperties.Add("RoomPolicePerGame", 1);
                    roomProperties.Add("RoomRobberPerGame", 1);
                    roomProperties.Add("RoomMaxTotalPlayer", 2);
                break;

                case 3:
                    roomProperties.Add("RoomPolicePerGame", 1);
                    roomProperties.Add("RoomRobberPerGame", 2);
                    roomProperties.Add("RoomMaxTotalPlayer", 3);
                break;

                case 4:
                    roomProperties.Add("RoomPolicePerGame", 2);
                    roomProperties.Add("RoomRobberPerGame", 2);
                    roomProperties.Add("RoomMaxTotalPlayer", 4);
                break;

                case 5:
                    roomProperties.Add("RoomPolicePerGame", 2);
                    roomProperties.Add("RoomRobberPerGame", 3);
                    roomProperties.Add("RoomMaxTotalPlayer", 5);
                break;

                case 6:
                    roomProperties.Add("RoomPolicePerGame", 2);
                    roomProperties.Add("RoomRobberPerGame", 4);
                    roomProperties.Add("RoomMaxTotalPlayer", 6);
                break;

                case 7:
                    roomProperties.Add("RoomPolicePerGame", 3);
                    roomProperties.Add("RoomRobberPerGame", 4);
                    roomProperties.Add("RoomMaxTotalPlayer", 7);
                break;

                case 8:
                    roomProperties.Add("RoomPolicePerGame", 3);
                    roomProperties.Add("RoomRobberPerGame", 5);
                    roomProperties.Add("RoomMaxTotalPlayer", 8);
                break;

                case 9:
                    roomProperties.Add("RoomPolicePerGame", 4);
                    roomProperties.Add("RoomRobberPerGame", 5);
                    roomProperties.Add("RoomMaxTotalPlayer", 9);
                break;

                case 10:
                    roomProperties.Add("RoomPolicePerGame", 4);
                    roomProperties.Add("RoomRobberPerGame", 6);
                    roomProperties.Add("RoomMaxTotalPlayer", 10);
                break;

                default:
                    print("Invalid amount : HostCustomPlayer");
                break;
            } */

            List<string> mapsLists = new List<string>();
            mapsLists.Add("Map_Bali");
            mapsLists.Add("Map_Bangkok01");
            mapsLists.Add("Map_Boracay01");

            roomProperties.Add("RoomMapName", GetRandomMapBiasLists(mapsLists, 70)); // Random map
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

            string[] exposedPropertiesInLobby = { "RoomGamemodeIndex", "RoomMaxTotalPlayer", "RoomMapName" }; 
            PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(exposedPropertiesInLobby);
        } // end if isMasterclient
    } // end HostCustomPlayer

    public override void OnPlayerEnteredRoom(Player newPlayer){ // When non host player enter a room. "Display / Update UI"
        UpdateTotalFindGame();

        /* if(PhotonNetwork.IsMasterClient && isInTheRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1 && PhotonNetwork.CurrentRoom.PlayerCount < (int)maxPlayersPerRoom && UIManager.instance.timeoutTimer > findGameAutoStart){ //HOI
            FindGameAutoStartAdder(3f);
        } */
    } // end OnPlayerEnteredRoom

    public override void OnPlayerLeftRoom(Player otherPlayer){ // When player cancel find game or after leave a room
        
        UpdateTotalFindGame();
    } // end OnPlayerLeftRoom

    public override void OnLeftRoom(){ // When player successfully left the room
        //print("Player Has Left The Room Completely");
        UpdateTotalFindGame(); // Update total players in room

        if(isInGame && GameManager.instance.gameStarted && !GameManager.instance.gameEnded){
            GameManager.instance.CheckWinningCondition();
        }
        
        if(!isInGame){ // if we are in main menu
            if(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout != null){
                StopAllCoroutines();
            }
            //StopCoroutine(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout); // Stop running courotine

            isInTheRoom = false;
        }else{
            // Redirect to main menu
            isInGame = false;
            isCreatingRoom = false;
            isInTheRoom = false;
            isFindingGame = false; // Set status to isFindingGame
            isQueing = false;
            doneQueing = false;
            PhotonNetwork.Disconnect();

            PhotonNetwork.LoadLevel("MainMenu");
        }
    } // end OnLeftRoom

    public override void OnDisconnected(DisconnectCause cause){
        print("Error : " + cause);

        /*if(!isInGame){
            if(Application.internetReachability == NetworkReachability.NotReachable || hasInternet){
                hasInternet = false;
                if(UIManager.instance.p_MainMenu != null){
                    UIManager.instance.p_MainMenu.playButton.interactable = true;
                    UIManager.instance.p_MainMenu.playText.text = "PLAY OFFLINE";
                }
            }
        } // end !isInGame*/
        
        //DisconnectCause.DisconnectByClientLogic // <--- Player close the game suddenly
        // if disconnect suddenly -> Popup Reconnect UI Prefab
        // if internet not reachable -> loading screen will popup & reconnect
        AudioManager.instance.StopMusic();
    }

    public void CheckForInternet(){
        /* if(cacheP_mainMenu == null){
            cacheP_mainMenu = GameObject.FindGameObjectWithTag("P_MainMenu").GetComponent<P_MainMenu>();
        } */

        if(Application.internetReachability == NetworkReachability.NotReachable){
            hasInternet = false;
            //PhotonNetwork.OfflineMode = true;

            if(UIManager.instance.p_MainMenu != null){
                UIManager.instance.p_MainMenu.playButton.interactable = false;
                UIManager.instance.p_MainMenu.privateButton.interactable = false;
                UIManager.instance.p_MainMenu.playText.text = "No Network";
            }
        }else{
            hasInternet = true;
            //PhotonNetwork.OfflineMode = false;
            if(UIManager.instance.p_MainMenu != null && joinedLobby){
                UIManager.instance.p_MainMenu.playButton.interactable = true;
                UIManager.instance.p_MainMenu.privateButton.interactable = true;
                UIManager.instance.p_MainMenu.playText.text = "Play";
            }
        }
    }

    public void SetOffline(){
        //Application.internetReachability = NetworkReachability.NotReachable;
        //PhotonNetwork.NetworkingClient.State = ClientState.PeerCreated;
        //PhotonNetwork.NetworkingClient.Disconnect();
        
        StartCoroutine(CanJoinGame());
    }

    IEnumerator CanJoinGame(){
        dontConnectInternet = true;
        if(PhotonNetwork.IsConnected){
            while(PhotonNetwork.IsConnected){
            yield return new WaitForSeconds(1f);
            PhotonNetwork.Disconnect();

            if(!PhotonNetwork.IsConnected){
                    hasInternet = false;
                    PhotonNetwork.OfflineMode = true;

                    findGameAutoStart = 0f;
                    playOfflineGame = true;

                    //yield return new WaitForSeconds(1f);
                    //JoinTheGame(0);

                    JoinOfflineGame();
                }
            } // end while
        }else{
            hasInternet = false;
            PhotonNetwork.OfflineMode = true;

            findGameAutoStart = 0f;
            playOfflineGame = true;

            //yield return new WaitForSeconds(1f);
            //JoinTheGame(0);

            JoinOfflineGame();
        }
    } // end CanJoinGame

    // ----------------------- CONNECTION RELATED END -------------------


    // ----------------------- FIND GAME RELATED START -------------------
    public void UpdateTotalFindGame(){ // Update room properties when player enters | Start the game here
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
                //_roomMapName = GetRandomMapBias("Map_Bali", 70);
                List<string> mapsLists = new List<string>();
                mapsLists.Add("Map_Bali");
                mapsLists.Add("Map_Bangkok01");
                mapsLists.Add("Map_Boracay01");
                mapsLists.Add("Map_Singapore");
                _roomMapName = GetRandomMapBiasLists(mapsLists, 70);
            }

            foreach(var player in PhotonNetwork.CurrentRoom.Players){
                _enteredPlayer++;
            }

            isInTheRoom = true;
            print("Total enter room :" + _enteredPlayer);

            if(playOfflineGame){
                if(PhotonNetwork.IsMasterClient){
                    PhotonNetwork.CurrentRoom.IsVisible = false; // Set Room IsVisible = false
                    StartCoroutine(ChangeScene(_roomMapName));// Host load level
                }
            }else{
                if(PhotonNetwork.CurrentRoom.PlayerCount > 1){
                    UIManager.instance.p_MainMenu.waitingForPlayersText.text = "Waiting for player..." + PhotonNetwork.CurrentRoom.PlayerCount + "/" + (int)maxPlayersPerRoom;
                    
                }else{
                    UIManager.instance.p_MainMenu.waitingForPlayersText.text = "Waiting for player...";
                }
        
                // If a room match all requirement, Host responsible to change the scene
                    if(_enteredPlayer == _roomTotalPlayer && isFindingGame){ // Only do this when we are finding game
                        print("Matched!");
                        //UIManager.instance.PopupLoadingScene(); // Popup Loading Scene UI

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
        findGameAutoStart = SOManager.instance.gameSettings.gameMode[gameModeIndex].findGameAutoStart;
        instantCreateFindGame = GetRandomBool();
        autoStartCreateGame = false;

        UIManager.instance.p_MainMenu.mainAreaGO.SetActive(false);
        PhotonNetwork.OfflineMode = false;
        JoinTheGame(0);
    }

    public void JoinOfflineGame(){
        isInGame = false;
        isCreatingRoom = false;
        isInTheRoom = false;
        isFindingGame = true; // Set status to isFindingGame

        if(!hasInternet){
            print("No Internet, so we add bot, load offline level");
            //UIManager.instance.PopupLoadingScene(); // Popup Loading Scene UI
            HostTheRoom();
        }else{
            print("Cannot Join. Because maybe you are in a room....maybe.");
        }
    }
    
    public void JoinTheGame(int modeIndex){ // Used by buttons in ChooseRole Screen
        isInGame = false;
        isCreatingRoom = false;
        isInTheRoom = false;
        isFindingGame = true; // Set status to isFindingGame

        /* if(PlayerPrefs.HasKey("PlayAgain") && PlayerPrefs.GetInt("PlayAgain") == 1){
            PlayerPrefs.SetInt("PlayAgain", 0);
            PlayerPrefs.DeleteKey("PlayAgain");
            PlayerPrefs.Save();

            playAgainExecuted = false;
        } */

        UIManager.instance.timeoutTimer = 0; // Reset timer

        Hashtable expectedRoomProperties = new Hashtable();
        if(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout != null){
            StopCoroutine(UIManager.instance.p_MainMenu.coroutinefindRoomTimeout); // Stop Coroutine when enter game
        }
        
        UIManager.instance.PopupFindGame();
        UIManager.instance.p_MainMenu.waitingForPlayersText.text = "Finding room...";

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
            //UIManager.instance.activeFindgameCancel(false);
            //print("HAS INTERNET");
        }else{
            if(!hasInternet){
                print("No Internet, so we add bot, load offline level");
                //UIManager.instance.PopupLoadingScene(); // Popup Loading Scene UI
                HostTheRoom();
            }else{
                print("Cannot Join. Because maybe you are in a room....maybe.");
            }
        }

    } // end JoinTeam

    public void CancelFindGameOrLeaveRoom(){ // Cancel while finding game or Leave Room. Used by cancel button in Modal_Findgame
        // Makesure we are in a room
        StartCoroutine(InitLeaveRoom());
    } // end CancelFindGameOrLeaveRoom

    IEnumerator InitLeaveRoom(){
        if(PhotonNetwork.CurrentRoom != null)
        PhotonNetwork.LeaveRoom();

        isFindingGame = false; // Set status to isFindingGame
        isInTheRoom = false;
        doneQueing = false;
        isInGame = false;
        isCreatingRoom = false;
        autoStartCreateGame = false;
        playOfflineGame = false;
        dontConnectInternet = false;
        print("InitLeaveRoom");

        if(isQueing){
            isQueing = false;
            queingTimer = 0;
        }
        StopAllCoroutines();

       while(PhotonNetwork.InRoom){
        yield return null;
       } 
    } // end InitLeaveRoom

    public IEnumerator InGameLeaveRoom(){
        PhotonNetwork.LeaveRoom();
        
        yield return new WaitForSeconds(.5f);

        isFindingGame = false; // Set status to isFindingGame
        isInTheRoom = false;
        doneQueing = false;
        isInGame = false;
        isCreatingRoom = false;
        autoStartCreateGame = false;
        playOfflineGame = false;
        dontConnectInternet = false;
        print("InGameLeaveRoom");

        if(isQueing){
            isQueing = false;
            queingTimer = 0;
        }

        if(!PhotonNetwork.InRoom)
            StartCoroutine(ChangeScene("MainMenu"));
    }

    public string GetRandomMap(){
        int randomNumber = Random.Range(0, SOManager.instance.maps.mapsList.Count);
        return SOManager.instance.maps.mapsList[randomNumber].name;
    }

    public string GetRandomMapBias(string _mapName, float _chance) {
        int randomNumber = Random.Range(0, 100);
        if (randomNumber < _chance) {
            return _mapName;
        } else {
            int randomIndex = Random.Range(0, SOManager.instance.maps.mapsList.Count);
            return SOManager.instance.maps.mapsList[randomIndex].name;
        }
    }

    public string GetRandomMapBiasLists(List<string> _mapNameLists, float _chance) {
        int randomNumber = Random.Range(0, 100);
        if (randomNumber < _chance) {
            return _mapNameLists[Random.Range(0, _mapNameLists.Count)];
        } else {
            int randomIndex = Random.Range(0, SOManager.instance.maps.mapsList.Count);
            return SOManager.instance.maps.mapsList[randomIndex].name;
        }
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
            playOfflineGame = false;
            dontConnectInternet = false;
            print("ChangeScene");

            yield return new WaitForSeconds(1.3f);
            PhotonNetwork.IsMessageQueueRunning = false;
            //if(PhotonNetwork.IsConnectedAndReady){
                PhotonNetwork.LoadLevel(sceneName);
            //}
            
        //}
    } // end ChangeScene

    // ----------------------- INGAME RELATED END -------------------
}
