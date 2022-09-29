using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerAbilities : MonoBehaviourPunCallbacks
{
    PlayerController playerController;
    [Header("Item Related")]
    GameObject btnItem;
    public bool hasItem;

    [Header("Radar")]
    public float radarTimer;
    public bool radarActivated;
    public float radarZoomVal;

    [Header("Boots")]
    public bool isItemDashingButtonDown;

    [Header("Lock Pick")]
    public E_EscapeArea escapeArea;
    public bool hasLockpick, lockpickUsed;
    public float lockpickTimer, lockpickDur;

    [Header("Undercover")]
    public bool onUndercover;
    public float undercoverTimer;

    [Header("Caught Immune")]
    public bool onCaughtImmune;
    public float caughtImmuneTimer;

    [Header("Prop Undercover")]
    public bool propUndercover;
    public float propUndercoverTimer;
    public GameObject propUndercoverGO;

    [Header("Char Invinsible")]
    public bool charInvisible;
    public float charInvisibleTimer;

    [Header("Sticky Trails")]
    public bool stickyEnable, canSpawnSticky;
    public int stickyCount, stickyTotal = 5;
    public float stickDuration;
    [Header("Cat Stun")]
    public bool catStunned;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        btnItem = UIManager.instance.gameUI.itemButton.gameObject;

        if(!hasItem){
            btnItem.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){
            if(Input.GetKeyDown(KeyCode.B)){
                //EnableItem("Cat Stun");
                /* EnableItem("Cauliflower");
                print("Enable Item"); */
            }

            if(Input.GetKeyDown(KeyCode.G)){
                
            }

            #if UNITY_EDITOR || UNITY_STANDALONE
                if(Input.GetButtonDown("UseItem") && hasItem && btnItem.GetComponent<Button>().interactable){ // X joystick
                    btnItem.GetComponent<Button>().onClick.Invoke();
                }
            #endif

            

            //  -------------------------------- Item Radar START ------------------------------------------------
            if(radarActivated){
                if(radarTimer <= 0){
                    radarTimer = 0;
                    if(playerController.cam2D.m_Lens.OrthographicSize > 7f){
                        playerController.cam2D.m_Lens.OrthographicSize -= Time.deltaTime * 20f;
                    }else if(playerController.cam2D.m_Lens.OrthographicSize <= 7f){
                        radarActivated = false;
                    }
                }else{
                    radarTimer -= Time.deltaTime;
                    if(playerController.cam2D.m_Lens.OrthographicSize < radarZoomVal){
                        playerController.cam2D.m_Lens.OrthographicSize += Time.deltaTime * 20f;
                    }
                }
            } // end id radarActivated
            //  -------------------------------- Item Radar END ------------------------------------------------

            //  -------------------------------- Item Boots START ------------------------------------------------
            if(playerController.moveDir == Vector3.zero){

            // Dash
            if(playerController.isDashingButtonDown && !isItemDashingButtonDown){ // Permenant Dash
                // If no direction pointing, refer left or right direction
                if(playerController.moveDir == Vector3.zero){
                    if(playerController.isFacingRight){
                        playerController.rb.velocity = new Vector3(1f, 0, 0) * playerController.moveSpeed;
                    }else{
                        playerController.rb.velocity = new Vector3(-1f, 0, 0) * playerController.moveSpeed;
                    }
                }
            }
            
            if(isItemDashingButtonDown && !playerController.isDashingButtonDown){  // Item Dash
                    // If no direction pointing, refer left or right direction
                    if(playerController.moveDir == Vector3.zero){
                        if(playerController.isFacingRight){
                            playerController.rb.velocity = new Vector3(1f, 0, 0) * playerController.moveSpeed;
                        }else{
                            playerController.rb.velocity = new Vector3(-1f, 0, 0) * playerController.moveSpeed;
                        }
                    }
                }
            }
            //  -------------------------------- Item Boots END ------------------------------------------------

            //  -------------------------------- Item Undercover START ------------------------------------------------
            if(onUndercover){
                if(undercoverTimer <= 0 && !charInvisible){
                    undercoverTimer = 0;
                    onUndercover = false;
                    photonView.RPC("DisableUndercover", RpcTarget.All);
                    playerController.SpawnSmoke();
                    StartCoroutine(playerController.PauseMovement(.2f));
                }else{
                    undercoverTimer -= Time.deltaTime;
                }
            }
            //  -------------------------------- Item Undercover END ------------------------------------------------


            //  -------------------------------- Item Prop Undercover START ------------------------------------------------
            if(propUndercover){
                if(propUndercoverTimer <= 0){
                    propUndercoverTimer = 0;
                    propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.All);
                    propUndercover = false;
                    propUndercoverGO.SetActive(false);

                    // also hide pet when invi
                    /*if(doneSpawnPet){
                        ForceEnablePet();
                    }*/
                    
                }else{
                    propUndercoverTimer -= Time.deltaTime;
                }
            }
            //  -------------------------------- Item Prop Undercover END ------------------------------------------------

            //  -------------------------------- Item Invisible START ------------------------------------------------
            if(charInvisible){
                if(charInvisibleTimer <= 0){
                    photonView.RPC("DisableInvisible", RpcTarget.All);
                    charInvisibleTimer = 0;
                    charInvisible = false;
                    /*if(propUndercover){
                        GetComponent<PlayerCollision>().photonView.RPC("DisableInvulnerable", RpcTarget.All, "Prop Undercover");
                    }*/

                    // also hide pet when invi
                    /*if(doneSpawnPet){
                        ForceEnablePet();
                    }*/
                }else{
                    charInvisibleTimer -= Time.deltaTime;
                }
            }
            //  -------------------------------- Item Invisible END ------------------------------------------------

            //  -------------------------------- Item Sticky Trail START ------------------------------------------------
            // Sticy Trail
            if(stickyEnable && canSpawnSticky){
                if(stickyCount < stickyTotal){
                    StartCoroutine(SpawnStickyTrail(stickDuration));
                    canSpawnSticky = false;
                }else{
                    stickyEnable = false;
                    canSpawnSticky = false;
                    print("Done Spawning");
                }
            }
            //  -------------------------------- Item Sticky Trail END ------------------------------------------------

            //  -------------------------------- Item Cat Stun START ------------------------------------------------
            if(catStunned){
                catStunned = false;
            }
            //  -------------------------------- Item Cat Stun END ------------------------------------------------


            //  -------------------------------- Item Lockpick START ------------------------------------------------
            if(hasLockpick && lockpickUsed && GetComponent<Robber>().isCaught && !GameManager.instance.gameEnded){
                if(lockpickTimer <= 0 && !GameManager.instance.gameEnded){
                        lockpickTimer = 0;
                        hasLockpick = false;
                        lockpickUsed = false;
                        playerController.ForceStartMove();
                        //GetComponent<PlayerCollision>().HideReleaseBar();
                        GetComponent<Robber>().photonView.RPC("PicklockReleased", RpcTarget.All, playerController.playerNameText.text, escapeArea, transform.position);
                }else{
                    lockpickTimer -= Time.deltaTime;
                    //GetComponent<PlayerCollision>().CalculateReleaseBar(lockpickTimer / lockpickDur);
                }
            }else if(lockpickUsed && GetComponent<Robber>().isCaught && GameManager.instance.gameEnded){
                //GetComponent<PlayerCollision>().HideReleaseBar();
                playerController.ForceStartMove();
            }
            //  -------------------------------- Item Lockpick END ------------------------------------------------


        } // end isMine
    }

#region ENABLE ITEM & RESET
    public void EnableItem(string itemName){
        if(photonView.IsMine){
            ResetItem();
            hasItem = true;
            btnItem.SetActive(true);
            var itemCode = 0;

            // Enable button effect
            // Send Name, Icon
            switch(itemName){
                case "Radar":
                    itemCode = 0;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemRadar(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;

                case "Boots":
                    itemCode = 1;
                    if(!playerController.isDashingButtonDown && !playerController.isDashing){
                        btnItem.GetComponent<Button>().onClick.AddListener(delegate{StartCoroutine(ItemBoots());});
                    }
                break;

                case "Lockpick":
                    itemCode = 2;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemLockpick(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                    btnItem.GetComponent<Button>().interactable = false;
                    hasLockpick = true;
                break;

                case "Banana":
                    itemCode = 3;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemBanana(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;

                case "Undercover":
                    itemCode = 4;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemUndercover(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;

                case "Cauliflower":
                    itemCode = 5;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemCauliflower(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                    playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(true);
                break;

                case "Flashbang":
                    itemCode = 6;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemFlashbang(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                    playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(true);
                break;

                case "Caught Immune":
                    //itemCode = 7;
                    //btnItem.GetComponent<Button>().onClick.AddListener(delegate{photonView.RPC("ItemCaughtImmune", RpcTarget.All, itemDetailsSO.itemsDetails[itemCode].effectDuration);});
                break;

                case "Prop Undercover":
                    itemCode = 8;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{photonView.RPC("ItemPropUndercover", RpcTarget.All, SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;

                case "Invisible":
                    itemCode = 9;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{photonView.RPC("ItemCharInvisible", RpcTarget.All, SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;

                case "Cat Stun":
                    itemCode = 10;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemCatStun(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                    playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(true);
                break;
                
                case "Sticky Trail":
                    itemCode = 11;
                    btnItem.GetComponent<Button>().onClick.AddListener(delegate{ItemStickyTrail(SOManager.instance.itemInfo.itemsDetails[itemCode].effectDuration);});
                break;
            }

            btnItem.transform.GetChild(0).GetComponent<Image>().sprite = SOManager.instance.itemInfo.itemsDetails[itemCode].itemIcon; // Img
            btnItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = SOManager.instance.itemInfo.itemsDetails[itemCode].name; // Name
        }
    } // end Enable Item


    public void ResetItem(){
        btnItem.GetComponent<Button>().onClick.RemoveAllListeners();
        btnItem.GetComponent<Button>().interactable = true;
        btnItem.transform.GetChild(0).GetComponent<Image>().sprite = null; // Img
        btnItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ""; // Name

        if(hasLockpick){
            hasLockpick = false;
        }

        //aimPoint.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        btnItem.SetActive(false);
    } // end RestItem
#endregion

    //  -------------------------------- Item Radar START ------------------------------------------------
    public void ItemRadar(float duration){
        //if(photonView.IsMine && !radarActivated){
        if(photonView.IsMine && !radarActivated){
            radarActivated = true;
            radarZoomVal = 16f;
            radarTimer = duration;
            AudioManager.instance.PlaySound("PS_UI_Radar");
            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemRadar
    //  -------------------------------- Item Radar END ------------------------------------------------

    //  -------------------------------- Item Boots START ------------------------------------------------
    public IEnumerator ItemBoots(){
        // Dash
        if(photonView.IsMine && !playerController.isDashingButtonDown && !playerController.isDashing){
            playerController.playerAnim.PlayAnimation("Dash"); // Play Dash Animation
            if(!playerController.isDashingButtonDown){
                isItemDashingButtonDown = true;
            }
            float _oldSpeed = playerController.moveSpeed; // Store old speed
            /*if(!isSlow && !isDashingButtonDown){
                ITEM_STORED_SPEED = MOVE_SPEED;
            }else{
                if(!isDashingButtonDown){
                    ITEM_STORED_SPEED = MOVE_SPEED_MODIFIED;
                }
            }*/

            // Set speed to dash speed
            if(!playerController.isDashingButtonDown){
                playerController.moveSpeed = playerController.moveSpeed * playerController.dashSpeedMultiplier; // Multiple current movespeed to multiplier
            }
            //playerState = E_State.DASH;
            
            yield return new WaitForSeconds(playerController.dashDuration);
            playerController.rb.velocity = Vector2.zero;
            //playerState = E_State.NORMAL;
            // Reset back to ori speed
            if(!playerController.isDashingButtonDown){
                playerController.moveSpeed = _oldSpeed;
            }
            //dashEFX.SetActive(false);
            
            if(!playerController.isDashingButtonDown){
                isItemDashingButtonDown = false;
            }

            // Play Dash Sound
            AudioManager.instance.PlaySound("PS_UI_Dash");

            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemBoots
    //  -------------------------------- Item ItemBoots END ------------------------------------------------
    
    //  -------------------------------- Item ItemBanana START ------------------------------------------------
    public void ItemBanana(float duration){
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_ThrowObject");
            GameObject ban = null;
            if(playerController.isFacingRight){
                ban = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Props", "prop_banana01_SP"), transform.position + new Vector3(-2f,0,0), Quaternion.identity);
            }else{
                ban = PhotonNetwork.InstantiateRoomObject(NetworkManager.GetPhotonPrefab("Props", "prop_banana01_SP"), transform.position + new Vector3(2f,0,0), Quaternion.identity);
            }   
            ban.GetComponent<Banana>().itemOwner = PhotonNetwork.LocalPlayer;
            ban.GetComponent<Banana>().ownerTeam = playerController.playerTeam;
            ban.GetComponent<Banana>().ownerGO = this.gameObject;
            ban.GetComponent<Banana>().dur = duration;
            
            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemBanana
    //  -------------------------------- Item ItemBanana END ------------------------------------------------
    
    //  -------------------------------- Item ItemUndercover START ------------------------------------------------
    public void ItemUndercover(float duration){
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_Undercover");
            playerController.SpawnSmoke();
            StartCoroutine(playerController.PauseMovement(.3f));
            
            if(!onUndercover){
                photonView.RPC("EnableUndercover", RpcTarget.All);
            }
            onUndercover = true;
            undercoverTimer = duration;

            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemUndercover

    [PunRPC]
    public void EnableUndercover(){
        // If we prop undercover
        if(propUndercover){
            //propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.AllBuffered);
            propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.All);
            propUndercover = false;
        }

        if(gameObject.tag == "Robber"){
            playerController.playerAnim.SwitchAnimController("Police", "P01"); //SOManager.instance.animVariantPolice.animatorLists[0].runTimeAnimController
            //playerController.playerNameText.color = new Color32(81,151,255,255); // Change to police blue
        }else{
            playerController.playerAnim.SwitchAnimController("Robber", "R01");
            //playerController.playerNameText.color = new Color32(255,140,16,255); // Change to robber orange
        }
    } // end EnableUndercover

    [PunRPC]
    public void DisableUndercover(){
        //anim.photonView.RPC("RevertAnimController", RpcTarget.AllBuffered);
        playerController.playerAnim.photonView.RPC("RevertAnimController", RpcTarget.All);
        if(onUndercover){ // double confirm
            onUndercover = false;
        }
        if(gameObject.tag == "Robber"){
            //playerController.playerNameText.color = new Color32(255,140,16,255); // Change to robber orange
        }else{
            //playerController.playerNameText.color = new Color32(81,151,255,255); // Change to police blue
        }
    } // end DisableUndercover
    //  -------------------------------- Item ItemUndercover END ------------------------------------------------
    
    //  -------------------------------- Item ItemPropUndercover START ------------------------------------------------
    [PunRPC]
    public void ItemPropUndercover(float duration){
        propUndercoverGO.SetActive(true);
        if(photonView.IsMine){
            if(!propUndercover){
                var ran = Random.Range(0, propUndercoverGO.GetComponent<PropUndercover>().propList.Count);
                propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("ChangeToProps", RpcTarget.All, ran);
                AudioManager.instance.PlaySound("PS_UI_Jutsu");
            }

            // also hide pet when invi
            /*if(doneSpawnPet){
                photonView.RPC("DisablePet", RpcTarget.AllBuffered);
            }*/
            

            propUndercover = true;
            propUndercoverTimer = duration;

            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemPropUndercover
    //  -------------------------------- Item ItemPropUndercover END ------------------------------------------------

    //  -------------------------------- Item ItemCharInvisible START ------------------------------------------------
    [PunRPC]
    public void ItemCharInvisible(float duration){
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_Invisible");
            // if on prop under cover. cancel it
            if(propUndercover){
                propUndercoverTimer = 0;
                //propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.AllBuffered);
                propUndercoverGO.GetComponent<PropUndercover>().photonView.RPC("PropsToNormal", RpcTarget.All);
                propUndercover = false;
                propUndercoverGO.SetActive(false);
            }

            if(onUndercover){
                undercoverTimer = 0;
                onUndercover = false;
                //photonView.RPC("DisableUndercover", RpcTarget.AllBuffered);
                photonView.RPC("DisableUndercover", RpcTarget.All);
                playerController.SpawnSmoke();
                StartCoroutine(playerController.PauseMovement(.2f));
            }

            if(!charInvisible){
                //photonView.RPC("EnableInvisible", RpcTarget.AllBuffered);
                photonView.RPC("EnableInvisible", RpcTarget.All);
                AudioManager.instance.PlaySound("PS_UI_Jutsu");
            }

            // also hide pet when invi
            /*if(doneSpawnPet){
                photonView.RPC("DisablePet", RpcTarget.AllBuffered);
            }*/


            charInvisible = true;
            charInvisibleTimer = duration;

            hasItem = false;
            btnItem.SetActive(false);
        }
    } // end ItemCharInvisible

    [PunRPC]
    public void EnableInvisible(){
        // If we prop undercover
        if(gameObject.tag == "Robber" || gameObject.tag == "Police"){
            StartCoroutine(playerController.PauseMovement(.05f));
            playerController.canvasPlayerName.SetActive(false);
            playerController.playerAnim.SwitchAnimController("Shared","Invi01");
        }
    } // end EnableInvisible

    [PunRPC]
    public void DisableInvisible(){
        StartCoroutine(playerController.PauseMovement(.05f));
        playerController.playerAnim.photonView.RPC("RevertAnimController", RpcTarget.All);

        if(!propUndercover){
            playerController.canvasPlayerName.SetActive(true);
        }
    } // end DisableInvisible
    //  -------------------------------- Item ItemInvisible END ------------------------------------------------

    //  -------------------------------- Item ItemStickyTrail START ------------------------------------------------
    // Item Sticky Trail
    public void ItemStickyTrail(float duration){
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_ThrowObject");
            stickyEnable = true;
            canSpawnSticky = true;
            stickyCount = 0;
            stickDuration = duration;
            hasItem = false;
            btnItem.SetActive(false);
        }
    }

    IEnumerator SpawnStickyTrail(float dur){
        if(playerController.isFacingRight){
            var ban = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "floor_mud_small01_SP"), transform.position + new Vector3(-2f,0,0), Quaternion.identity);
            ban.AddComponent<PropsAnimation>();
            if(ban.GetComponent<PropsAnimation>() != null){
                ban.GetComponent<PropsAnimation>().duration = dur;
            }
        }else{
            var ban = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "floor_mud_small01_SP"), transform.position + new Vector3(2f,0,0), Quaternion.identity);
            ban.AddComponent<PropsAnimation>();
            if(ban.GetComponent<PropsAnimation>() != null){
                ban.GetComponent<PropsAnimation>().duration = dur;
            }
        }
        stickyCount += 1;
        yield return new WaitForSeconds(.17f);
        canSpawnSticky = true;
    }
    //  -------------------------------- Item ItemStickyTrail END ------------------------------------------------

    //  -------------------------------- Item ItemCauliflower START ------------------------------------------------
    public void ItemCauliflower(float duration){
        var cauliSpeed = 20f; // adjust speed
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_ThrowObject"); 
            
            var cau = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "prop_cauliflower01_SP"), playerController.aimPoint.transform.GetChild(0).position, Quaternion.identity);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), cau.GetComponent<Collider2D>());
            cau.GetComponent<Cauliflower>().teamOwner = gameObject.tag;
            cau.GetComponent<Cauliflower>().ownerGO = this.gameObject;
            cau.GetComponent<Cauliflower>().itemOwner = PhotonNetwork.LocalPlayer;
            cau.GetComponent<Cauliflower>().dur = duration;
            if(playerController.moveDir == Vector3.zero){
                if(playerController.isFacingRight){
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(1f, 0, 0) * cauliSpeed;
                }else{
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(-1f, 0, 0) * cauliSpeed;
                }
            }else{
                cau.GetComponent<Rigidbody2D>().velocity = playerController.moveDir * cauliSpeed;
            }

            hasItem = false;
            btnItem.SetActive(false);

            // Hide crosshair
            playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(false);
        }
    }// end ItemCauliflower
    //  -------------------------------- Item ItemCauliflower END ------------------------------------------------

    //  -------------------------------- Item ItemFlashbang START ------------------------------------------------
    public void ItemFlashbang(float duration){
        var throwSpeed = 28f; // adjust speed
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_ThrowObject"); 
            var cau = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "prop_flashbang01_SP"), playerController.aimPoint.transform.GetChild(0).transform.position, Quaternion.identity);
            cau.GetComponent<Flashbang>().explodeCountdown = duration;
            if(playerController.moveDir == Vector3.zero){
                if(playerController.isFacingRight){
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(1f, 0, 0) * throwSpeed;
                }else{
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(-1f, 0, 0) * throwSpeed;
                }
            }else{
                cau.GetComponent<Rigidbody2D>().velocity = playerController.moveDir * throwSpeed;
            }

            hasItem = false;
            btnItem.SetActive(false);

            // Hide crosshair
            playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(false);
        }
    } // end ItemFlashbang

    [PunRPC]
    public void BlindByFlashbang(){
        if(photonView.IsMine){
            FlashbangEffect.instance.Explode();
        }
    }
    //  -------------------------------- Item ItemFlashbang END ------------------------------------------------

    //  -------------------------------- Item ItemCaughtImmune START ------------------------------------------------
    public void ItemCatStun(float duration){
        var throwSpeed = 20f; // adjust speed
        if(photonView.IsMine){
            AudioManager.instance.PlaySound("PS_UI_ThrowObject");
            var cau = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Props", "prop_catstun01_SP"), playerController.aimPoint.transform.GetChild(0).transform.position, Quaternion.identity);
        
            if(playerController.isFacingRight){
                cau.GetComponent<CatStun>().facingRight = true;
            }else{
                cau.GetComponent<CatStun>().facingRight = false;
            }
            if(playerController.moveDir == Vector3.zero){
                if(playerController.isFacingRight){
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(1f, 0, 0) * throwSpeed;
                }else{
                    cau.GetComponent<Rigidbody2D>().velocity = new Vector3(-1f, 0, 0) * throwSpeed;
                }
            }else{
                cau.GetComponent<Rigidbody2D>().velocity = playerController.moveDir * throwSpeed;
            }

            hasItem = false;
            btnItem.SetActive(false);

            // Hide crosshair
            playerController.aimPoint.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void StunnedByCat(float duration, Vector3 catPos){
        if(photonView.IsMine && !catStunned){
            
            if(catPos.x > transform.position.x){
                if(!playerController.isFacingRight){
                    playerController.FlipRight(true);
                }
            }else if(catPos.x < transform.position.x){
                if(playerController.isFacingRight){
                    playerController.FlipRight(false);
                }
            }else{
                print("Dont change flip");
            }
            // Falling
            playerController.gameObject.GetPhotonView().RPC("PlayerFall", RpcTarget.All, duration);
            // Spawn Love Emote
            var loveableEfx = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "Loveable"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
            catStunned = true;
        }else{
            print("What happen?");
        }
    }
    //  -------------------------------- Item ItemCaughtImmune END ------------------------------------------------

    //  -------------------------------- Item ItemLockPick START ------------------------------------------------
    public void ItemLockpick(float duration){
        if(photonView.IsMine){
            lockpickUsed = true;
            AudioManager.instance.PlaySound("PS_Lockpick");
            playerController.ForceStopMove();
            lockpickTimer = duration;
            lockpickDur = duration;
            hasItem = false;
            //hasLockpick = false;
            btnItem.SetActive(false);
        }
    }
    //  -------------------------------- Item ItemLockPick END ------------------------------------------------

}
