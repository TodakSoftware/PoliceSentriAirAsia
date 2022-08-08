using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public E_Team playerTeam;
    [Header("Movement")]
    Rigidbody2D rb;
    Vector3 moveDir;
    float horizontal, vertical;
    public bool isFacingRight;
    GameUI gameUI;
    Joystick moveJoystick;
    [SerializeField] float moveSpeed = 8f;
    public bool canMove = true;
    public bool isMoving;

    [Header("UI Info")]
    public GameObject canvasPlayerName;
    public GameObject canvasReleasedBar;
    public TextMeshProUGUI playerNameText;
    public GameObject rankImg;

    [Header("Dash")]
    [SerializeField] float dashSpeedMultiplier = 2f;
    [SerializeField] public float dashDuration = .2f;
    [SerializeField] public float dashCooldown = 3f;
    [HideInInspector] public bool isDashing, isDashCooldown;
    GameObject btnDash;

    [Header("Fall")]
    [HideInInspector] public bool isFalling;
    
    [Header("Camera Related")]
    [HideInInspector] public CinemachineVirtualCamera cam2D;
    CinemachineImpulseSource impulseSource;

    [Header("Animation Related")]
    public PlayerAnimator playerAnim;
    public string characterCode;
    public System.Guid myGUID = System.Guid.NewGuid();

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        cam2D = GameObject.Find("2DCam").GetComponent<CinemachineVirtualCamera>();
        impulseSource = cam2D.gameObject.GetComponent<CinemachineImpulseSource>();
        gameUI = UIManager.instance.gameUI;
        moveJoystick = gameUI.movementJoystick.GetComponent<Joystick>();
        btnDash = gameUI.dashButton.gameObject;

        // Link Dash
        btnDash.GetComponent<Button>().onClick.AddListener(delegate{StartCoroutine(PlayerDash(dashDuration));});
    }

    void Start(){
        print(myGUID.ToString());
        if(!photonView.IsMine){
            return;
        }

        GameManager.instance.ownedPlayerGO = this.gameObject;

        // Set cam follow
        cam2D.m_Follow = this.transform;
        cam2D.m_LookAt = this.transform;

        SetupPlayerAnimator(); // cannot put on awake because of order execution

        // Enable Joystick on Mobile Devices
        #if UNITY_EDITOR || UNITY_STANDALONE
            if(moveJoystick.gameObject.activeSelf){
                moveJoystick.gameObject.SetActive(false);
            }
        #elif UNITY_ANDROID || UNITY_IOS
            if(!moveJoystick.gameObject.activeSelf){
                moveJoystick.gameObject.SetActive(true);
            }
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine){
            return;
        }

        HandleMovement();
        HandleAnimation();

        if(Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(PlayerDash(dashDuration));
        }

        if(Input.GetKeyDown(KeyCode.J)){
            //StartCoroutine(PlayerFall(3f));
        }

        // FPS Counter
        if(NetworkManager.instance.hasInternet){
            if(PhotonNetwork.GetPing() <= 100){
                gameUI.fpsText.text = "<color=green>" + PhotonNetwork.GetPing() + "ms</color>";
            }else if(PhotonNetwork.GetPing() > 100 && PhotonNetwork.GetPing() < 200){
                gameUI.fpsText.text = "<color=yellow>" + PhotonNetwork.GetPing() + "ms</color>";
            }else{
                gameUI.fpsText.text = "<color=red>" + PhotonNetwork.GetPing() + "ms</color>";
            }
        }else{
            gameUI.fpsText.text = "";
        } // end if hasInternet (FPS Counter)
        
    }

    void FixedUpdate(){
        if(!photonView.IsMine){
            return;
        }

        if(canMove){
            rb.velocity = moveDir * moveSpeed;
        } // end canMove
    }

#region MOVEMENT RELATED
    void HandleMovement(){
        if(Input.GetAxisRaw("Horizontal") != 0){
            horizontal = Input.GetAxisRaw("Horizontal");
        }else if(moveJoystick.Horizontal != 0){
            horizontal = moveJoystick.Horizontal;
        }else{
            horizontal = 0;
        }
        
        if(Input.GetAxisRaw("Vertical") != 0){
            vertical = Input.GetAxisRaw("Vertical");
        }else if(moveJoystick.Vertical != 0){
            vertical = moveJoystick.Vertical;
        }else{
            vertical = 0;
        }

        moveDir = new Vector3(horizontal, vertical).normalized;

        if(moveDir.x > 0){
            FlipRight(true);
        }else if(moveDir.x < 0){
            FlipRight(false);
        }

        if(moveDir != Vector3.zero){
            isMoving = true;
        }else{
            isMoving = false;
            
            if(isDashing){ // if we are !moving but dash, push into direction
                if(isFacingRight){
                    moveDir = new Vector3(1, 0).normalized;
                    rb.velocity = (moveDir * dashSpeedMultiplier) * moveSpeed;
                }else{
                    moveDir = new Vector3(-1, 0).normalized;
                    rb.velocity = (moveDir * dashSpeedMultiplier) * moveSpeed;
                }
            }
        }
    } // end HandleMovement

    public void FlipRight(bool flipRight){
        if(!isFalling){ // only do this when !Falling
            if(flipRight){
                isFacingRight = true;
                transform.localScale = new Vector3(-1f,transform.localScale.y,transform.localScale.z);
                canvasPlayerName.transform.localScale = new Vector3(-0.01f,canvasPlayerName.transform.localScale.y,canvasPlayerName.transform.localScale.z);
                canvasReleasedBar.transform.localScale = new Vector3(-0.01f,canvasReleasedBar.transform.localScale.y,canvasReleasedBar.transform.localScale.z);
            }else{
                isFacingRight = false;
                transform.localScale = new Vector3(1f,transform.localScale.y,transform.localScale.z);
                canvasPlayerName.transform.localScale = new Vector3(0.01f,canvasPlayerName.transform.localScale.y,canvasPlayerName.transform.localScale.z);
                canvasReleasedBar.transform.localScale = new Vector3(0.01f,canvasReleasedBar.transform.localScale.y,canvasReleasedBar.transform.localScale.z);
            }
        }
    } // end FlipRight

#endregion // end MOVEMENT RELATED

#region DASH RELATED
    IEnumerator PlayerDash(float duration){
        if(!isDashing && !isDashCooldown && !isFalling){
            btnDash.GetComponent<ButtonCooldown>().StartCooldown(dashCooldown);
            StartCoroutine(CooldownPlayerDash(dashCooldown)); // Set Button Cooldown
            playerAnim.PlayAnimation("Dash"); // Play Dash Animation
            float _oldSpeed = moveSpeed; // Store old speed
            moveSpeed = moveSpeed * dashSpeedMultiplier; // Multiple current movespeed to multiplier
            impulseSource.GenerateImpulse(); // Shake the screen
            isDashing = true; // Set isDashing = true

            yield return new WaitForSeconds(duration); // Waiting duration

            rb.velocity = Vector2.zero; // Force velocity = 0
            moveSpeed = _oldSpeed; // Revert back to old speed
            isDashing = false; // Set isDashing = false
        }
    } // end PlayerDash

    IEnumerator CooldownPlayerDash(float cooldownDuration){
        if(!isDashCooldown){
            isDashCooldown = true;
            yield return new WaitForSeconds(cooldownDuration);
            isDashCooldown = false;
        } // end !isDashCooldown
    } // end CooldownPlayerDash

#endregion // end DASH RELATED

#region FALL RELATED
    public IEnumerator PlayerFall(float fallDuration){
        if(!isFalling){
            SpawnSmoke();
            isFalling = true;
            canMove = false;
            moveDir = Vector3.zero;
            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(fallDuration);
            canMove = true;
            isFalling = false;
        }
    } // end PlayerFall

#endregion // end FALL RELATED

#region ANIMATION RELATED
    public void HandleAnimation(){
        if(moveDir != Vector3.zero){
            if(!isDashing){
                playerAnim.PlayAnimation("Run");
            }
        }else{
            if(!isDashing){
                playerAnim.PlayAnimation("Idle");
            }
        }

        if(isFalling){
            playerAnim.PlayAnimation("Fall");
        }
    } // end HandleAnimation

    public void SetupPlayerAnimator(){
        // Setup Tag
        switch(playerTeam){
            case E_Team.POLICE:
                if(SOManager.instance.animVariantPolice.animatorLists.Count > 0){
                    foreach(var animator in SOManager.instance.animVariantPolice.animatorLists){
                        if(animator.code == characterCode){
                            playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Police", animator.code);
                            playerAnim.originalAnimatorController = animator.runTimeAnimController;

                            if(photonView.IsMine){
                                Hashtable teamRole = new Hashtable();
                                teamRole.Add("NetworkTeam", "Police");
                                teamRole.Add("CharacterCode", characterCode);
                                teamRole.Add("PlayerCaught", false);
                                teamRole.Add("PlayerHoldMoneybag", false);
                                teamRole.Add("PlayerViewID", photonView.ViewID);
                                PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                            }
                        }
                    } // end foreach
                }
            break;

            case E_Team.ROBBER:
                if(SOManager.instance.animVariantRobber.animatorLists.Count > 0){
                    foreach(var animator in SOManager.instance.animVariantRobber.animatorLists){
                        if(animator.code == characterCode){
                            playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Robber", animator.code);
                            playerAnim.originalAnimatorController = animator.runTimeAnimController;

                            if(photonView.IsMine){
                                Hashtable teamRole = new Hashtable();
                                teamRole.Add("NetworkTeam", "Robber");
                                teamRole.Add("CharacterCode", characterCode);
                                teamRole.Add("PlayerCaught", false);
                                teamRole.Add("PlayerHoldMoneybag", false);
                                teamRole.Add("PlayerViewID", photonView.ViewID);
                                PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
                            }
                        }
                    } // end foreach
                }
            break;

            default:
                print("Not Found!");
            break;
        } // end switch
    } // end SetupPlayerAnimator

#endregion // end ANIMATION RELATED

#region UI RELATED
    [PunRPC]
    public void CreateAvatar(){
        switch(playerTeam){
            case E_Team.POLICE:
                var policeAvatar = Instantiate(UIManager.instance.gameUI.avatarBtnPrefab);
                if(GetComponent<BotController>() != null){ // if we are bot
                    var randomSkin = Random.Range(0, SOManager.instance.animVariantPolice.animatorLists.Count);

                    policeAvatar.GetComponent<Btn_Avatar>().SetupButton("Police", GetComponent<PhotonView>().Owner.NickName, SOManager.instance.animVariantPolice.animatorLists[randomSkin].code, myGUID.ToString());
                }else{
                    policeAvatar.GetComponent<Btn_Avatar>().SetupButton("Police", GetComponent<PhotonView>().Owner.NickName, characterCode, myGUID.ToString());
                }
                
                policeAvatar.transform.SetParent(UIManager.instance.gameUI.avatarPoliceContent,false);
                UIManager.instance.gameUI.avatarBtnList.Add(policeAvatar.GetComponent<Btn_Avatar>());

                if(photonView.IsMine){
                    playerNameText.text = PhotonNetwork.NickName;
                }else{
                    playerNameText.text = photonView.Owner.NickName;
                }
            break;

            case E_Team.ROBBER:
                var robberAvatar = Instantiate(UIManager.instance.gameUI.avatarBtnPrefab);
                if(GetComponent<BotController>() != null){ // if we are bot
                    var randomSkin = Random.Range(0, SOManager.instance.animVariantRobber.animatorLists.Count);

                    robberAvatar.GetComponent<Btn_Avatar>().SetupButton("Robber", GetComponent<PhotonView>().Owner.NickName, SOManager.instance.animVariantRobber.animatorLists[randomSkin].code, myGUID.ToString());
                }else{
                    robberAvatar.GetComponent<Btn_Avatar>().SetupButton("Robber", GetComponent<PhotonView>().Owner.NickName, characterCode, myGUID.ToString());
                }
                
                robberAvatar.transform.SetParent(UIManager.instance.gameUI.avatarRobberContent,false);
                UIManager.instance.gameUI.avatarBtnList.Add(robberAvatar.GetComponent<Btn_Avatar>());

                if(photonView.IsMine){
                    playerNameText.text = PhotonNetwork.NickName;
                }else{
                    playerNameText.text = photonView.Owner.NickName;
                }
            break;
        }
    }
#endregion

#region PARTICLES RELATED
    public void SpawnSmoke(){
        var smoke = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "PuffSmoke"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
    }

#endregion // End particles related
}
