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
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector]public Vector3 moveDir;
    float horizontal, vertical;
    public bool isFacingRight;
    public GameUI gameUI;
    Joystick moveJoystick;
    public float moveSpeed = 8f;
    public bool canMove = true;
    public bool isMoving;

    [Header("UI Info")]
    public GameObject canvasPlayerName;
    public GameObject canvasReleasedBar;
    public TextMeshProUGUI playerNameText;
    public GameObject rankImg;

    [Header("Dash")]
    [SerializeField] public float dashSpeedMultiplier = 2f;
    [SerializeField] public float dashDuration = .2f;
    [SerializeField] public float dashCooldown = 3f;
    [HideInInspector] public bool isDashing, isDashCooldown, isDashingButtonDown;
    GameObject btnDash;

    [Header("Fall")]
    public bool isFalling;
    [Header("Slow")]
    public bool isSlow;
    float MOVE_SPEED_MODIFIED;
    // Slow movement fixed
    public float slowTimer;
    [Header("Slide")]
    public bool isSlide;

    [Header("Throw")]
    public GameObject aimPoint; // for item
    public GameObject throwPoint; // for moneybag
    public float throwDistance = 20f;
    Vector3 startPos,endPos;
    
    [Header("Camera Related")]
    [HideInInspector] public CinemachineVirtualCamera cam2D;
    CinemachineImpulseSource impulseSource;

    [Header("Animation Related")]
    public PlayerAnimator playerAnim;
    public string characterCode;

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
        
        if(!photonView.IsMine){
            return;
        }

        GameManager.instance.ownedPlayerGO = this.gameObject;

        // Set cam follow
        cam2D.m_Follow = this.transform;
        cam2D.m_LookAt = this.transform;

        if(playerTeam == E_Team.POLICE){
            if(photonView.IsMine){
                Hashtable teamRole = new Hashtable();
                teamRole.Add("NetworkTeam", "Police");
                teamRole.Add("CharacterCode", characterCode);
                teamRole.Add("PlayerCaught", false);
                teamRole.Add("PlayerHoldMoneybag", false);
                teamRole.Add("PoliceCaughtCount", 0);
                PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
            }
        }else{
            if(photonView.IsMine){
                Hashtable teamRole = new Hashtable();
                teamRole.Add("NetworkTeam", "Robber");
                teamRole.Add("CharacterCode", characterCode);
                teamRole.Add("PlayerCaught", false);
                teamRole.Add("PlayerHoldMoneybag", false);
                teamRole.Add("RobberReleasedCount", 0);
                PhotonNetwork.LocalPlayer.SetCustomProperties(teamRole);
            }
        }

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

        // Aim & Shot Point START --------------------
        if(moveDir.magnitude >= 0.01f){
            isMoving = true;
            endPos = aimPoint.transform.position;
            endPos.z = 0;
            endPos = new Vector3((horizontal * 1.8f) + aimPoint.transform.position.x, (vertical * 1.8f) + aimPoint.transform.position.y, 0f);

            aimPoint.transform.GetChild(0).transform.position = endPos; // For any throwable item
        }else{
            isMoving = false;
            if(isFacingRight){
                endPos = new Vector3((1 * 1.8f) + aimPoint.transform.position.x, (0 * 1.8f) + aimPoint.transform.position.y, 0f);
                aimPoint.transform.GetChild(0).transform.position = endPos; // For any throwable item
                
            }else{
                endPos = new Vector3((-1 * 1.8f) + aimPoint.transform.position.x, (0 * 1.8f) + aimPoint.transform.position.y, 0f);
                aimPoint.transform.GetChild(0).transform.position = endPos; // For any throwable item
            }
        }
        // Aim & Shot Point END --------------------
        
        // IsSlow START ----------------------------------------------
        if(!isFalling && !isDashing && !isSlow && !isSlide){ // if is normal state
            if(GetComponent<PlayerRank>().rankLevel == 0 && moveSpeed > 8f && !isSlow){
                moveSpeed = 8f;
            }else if(GetComponent<PlayerRank>().rankLevel == 1 && moveSpeed > 8.5f && !isSlow){
                moveSpeed = 8.5f;
            }else if(GetComponent<PlayerRank>().rankLevel == 4 && moveSpeed > 9.5f && !isSlow){
                moveSpeed = 9.5f;
            }
        }

        if(isSlow){
            if(slowTimer <= 0){
                slowTimer = 0;
                isSlow = false;
                DisableSlowMovement();
            }else{
                slowTimer -= Time.deltaTime;
            }
        }
        // IsSlow END ----------------------------------------------
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

    [PunRPC]
    public IEnumerator PauseMovement(float duration){
        ForceStopMove();
        yield return new WaitForSeconds(duration);
        ForceStartMove();
    }

    public void EnableSlowMovement(){
        if(photonView.IsMine){
            if(isDashing){
                MOVE_SPEED_MODIFIED = moveSpeed;
                moveSpeed = moveSpeed / 2f;
            }else{
                MOVE_SPEED_MODIFIED = moveSpeed;
                moveSpeed = moveSpeed / 2f;
            }
            AudioManager.instance.PlaySound("PS_UI_StuckInMud");
            slowTimer = 5f;
        }
    }

    public void DisableSlowMovement(){
        if(photonView.IsMine){
            moveSpeed = MOVE_SPEED_MODIFIED;
        }
    }

    public void ForceStopMove(){  // Disable Movement
        if(photonView.IsMine){
            moveDir = Vector3.zero;
            //rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            canMove = false;
            playerAnim.PlayAnimation("Idle");
        }
    }

    public void ForceStartMove(){  // Re-enable movement
        if(photonView.IsMine){
            canMove = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

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
    public IEnumerator PlayerDash(float duration){
        if(!isDashing && !isDashCooldown && !isFalling && !isSlow){
            AudioManager.instance.PlaySound("PS_UI_Dash");

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
    [PunRPC]
    public IEnumerator PlayerFall(float fallDuration){
        if(!isFalling){
            SpawnSmoke();
            isFalling = true;
            canMove = false;
            moveDir = Vector3.zero;
            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(fallDuration);
            // If we prop undercover
            if(GetComponent<PlayerAbilities>().propUndercover){
                GetComponent<PlayerAbilities>().propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.All);
                GetComponent<PlayerAbilities>().propUndercover = false;
            }

            if(GetComponent<PlayerAbilities>().onUndercover){
                GetComponent<PlayerAbilities>().undercoverTimer = 0;
                GetComponent<PlayerAbilities>().onUndercover = false;
                GetComponent<PlayerAbilities>().photonView.RPC("DisableUndercover", RpcTarget.All);
                StartCoroutine(PauseMovement(.2f));
            }

            canMove = true;
            isFalling = false;
        }
    } // end PlayerFall

    public void FallSound(string type){
        if(photonView.IsMine){
            switch(type){
                case "Banana":
                    AudioManager.instance.PlaySound("PS_UI_BananaSlipped");
                break;
            }
        }
        
    }

#endregion // end FALL RELATED

#region ANIMATION RELATED
    public void HandleAnimation(){
        if(moveDir != Vector3.zero){
            if(!isDashing && !GetComponent<PlayerAbilities>().isItemDashingButtonDown){
                playerAnim.PlayAnimation("Run");
            }
        }else{
            if(!isDashing && !GetComponent<PlayerAbilities>().isItemDashingButtonDown){
                playerAnim.PlayAnimation("Idle");
            }
        }

        if(isFalling && !GetComponent<PlayerAbilities>().isItemDashingButtonDown){
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
                            playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Police", characterCode);
                            playerAnim.originalAnimatorController = animator.runTimeAnimController;
                            playerAnim.animator.runtimeAnimatorController = animator.runTimeAnimController;

                            
                        }
                    } // end foreach
                }
            break;

            case E_Team.ROBBER:
                if(SOManager.instance.animVariantRobber.animatorLists.Count > 0){
                    foreach(var animator in SOManager.instance.animVariantRobber.animatorLists){
                        if(animator.code == characterCode){
                            playerAnim.photonView.RPC("SwitchAnimController", RpcTarget.AllBuffered, "Robber", characterCode);
                            playerAnim.originalAnimatorController = animator.runTimeAnimController;
                            playerAnim.animator.runtimeAnimatorController = animator.runTimeAnimController;

                            
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
                policeAvatar.GetComponent<Btn_Avatar>().SetupButton("Police", GetComponent<PhotonView>().Owner.NickName, characterCode, this.gameObject);
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
                robberAvatar.GetComponent<Btn_Avatar>().SetupButton("Robber", GetComponent<PhotonView>().Owner.NickName, characterCode, this.gameObject);
                robberAvatar.transform.SetParent(UIManager.instance.gameUI.avatarRobberContent,false);
                UIManager.instance.gameUI.avatarBtnList.Add(robberAvatar.GetComponent<Btn_Avatar>());

                if(photonView.IsMine){
                    playerNameText.text = PhotonNetwork.NickName;
                }else{
                    playerNameText.text = photonView.Owner.NickName;
                }
            break;
        } // end switch
    }
#endregion

#region PARTICLES RELATED
    public void SpawnSmoke(){
        var smoke = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "PuffSmoke"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
    }

#endregion // End particles related
}
