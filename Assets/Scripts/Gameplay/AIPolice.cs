using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PolyNavAgent))]
public class AIPolice : MonoBehaviourPunCallbacks
{
    PolyNavAgent agent;
    public Transform target;
    public Transform roamTarget;
    public E_Team targetTagTeam;
   string targetTag = "";
    public float detectRange = 300f;
    List<Transform> randomGoToPositions = new List<Transform>();

    [Header("Animations")]
    public Animator anim;
    public SpriteRenderer botSpriteRenderer;
    public bool isDashing, isFalling;
    [Header("UI")]
    public Transform nameCanvas;
    public Transform barCanvas;
    public TextMeshProUGUI playerNameText;
    [Header("Statistic")]
    public int caughtCount;
    [Header("Item Related")]
    public bool catStunned;
    public bool pauseMovement;

    void Awake(){
        agent = GetComponent<PolyNavAgent>();

        // Set isBot
        GetComponent<Police>().isBot = true;

        // Set Bot Name
        playerNameText.text = "Police" + photonView.OwnerActorNr;

        // Set PolyNav
        agent.map = GameObject.FindObjectOfType<PolyNav2D>();

        // Set which player tag for we to chase
        switch(targetTagTeam){
            case E_Team.POLICE:
                targetTag = "Police";
            break;

            case E_Team.ROBBER:
                targetTag = "Robber";
            break;

            default:
            break;
        }

        // Add Random Location
        foreach(var p in GameManager.instance.moneybagSpawnpoints){
            randomGoToPositions.Add(p);
        }

        foreach(var r in GameManager.instance.robberSpawnpoints){
            randomGoToPositions.Add(r);
        }

        // Create Avatar
        CreateAvatarUI();
        
    }

    void CreateAvatarUI(){
        var policeAvatar = Instantiate(UIManager.instance.gameUI.avatarBtnPrefab);
        policeAvatar.GetComponent<Btn_Avatar>().SetupButton("Police", playerNameText.text, "P01", this.gameObject);
        policeAvatar.transform.SetParent(UIManager.instance.gameUI.avatarPoliceContent,false);
        UIManager.instance.gameUI.avatarBtnList.Add(policeAvatar.GetComponent<Btn_Avatar>());
    }

    public void InitBot(){
        InvokeRepeating("ChaseRobber", 0f, 1f);
        InvokeRepeating("UpdateRoaming", 0f, 1f);
        InvokeRepeating("InvokeDash", 3f, 5f);
    }

    void Update(){
        if(target != null){
            agent.SetDestination(target.position);
        }

        if(isFalling || pauseMovement){
            agent.Stop();
        }

        HandleBotAnimation();

        if(Input.GetKeyDown(KeyCode.K)){
            StartCoroutine(BotFalling(3f));
        }
    }

    void UpdateRoaming(){
        if(roamTarget == null && target == null){
            Roaming();
        }else if(roamTarget != null && agent.botDestinationReach){ // if isroaming && 
            agent.botDestinationReach = false;
            ClearRoamTarget();
        }
    }

    void Roaming(){
        if(roamTarget == null){
            roamTarget = GetRoamRandomPosition();
            agent.SetDestination(roamTarget.position);
        }
    }

    void ChaseRobber(){
        if(GetClosestEnemy(targetTag) != null){
            target = GetClosestEnemy(targetTag);
            ClearRoamTarget();
        }else{
            ClearTarget();
        }
    }

    void ClearTarget(){
        target = null;
    }

    void ClearRoamTarget(){
        roamTarget = null;
    }

    // ------------------------------------- GLOBAL ------------------------------------------
    public Transform GetClosestEnemy(string team)
    {
        Transform bestTarget = null;
        //float closestDistanceSqr = Mathf.Infinity;
        float closestDistanceSqr = detectRange;
        Vector3 currentPosition = transform.position;
        
        foreach(GameObject potentialTarget in GameObject.FindGameObjectsWithTag(team)){
            if(team == "Robber" && potentialTarget.GetComponent<Robber>() != null && !potentialTarget.GetComponent<Robber>().isCaught){
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if(dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.transform;
                    return bestTarget;
                }
            }else if(team == "Police"){
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if(dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.transform;
                    return bestTarget;
                }
            }
        }
        
        return null;
    } // end GetClosestEnemy


    Transform GetRoamRandomPosition(){
        var randomIndex = Random.Range(0, randomGoToPositions.Count);
        return randomGoToPositions[randomIndex];
    }

    public IEnumerator BotFalling(float dur){
        isFalling = true;
        isDashing = false;

        agent.Stop();
        ClearRoamTarget();
        ClearTarget();

        yield return new WaitForSeconds(dur);

        isFalling = false;
        agent.botDestinationReach = true;
        Roaming();
    }

    public void HandleBotAnimation(){
        if(agent.currentSpeed > 0){
            if(!isDashing){
                PlayAnimation("Run");

                if(agent.botFacingRight){
                    transform.localScale = new Vector3(-1f,1f,1f);

                    nameCanvas.localScale = new Vector3(-0.01f,0.01f,0.01f);
                    barCanvas.localScale = new Vector3(-0.01f,0.01f,0.01f);
                }else{
                    transform.localScale = new Vector3(1f,1f,1f);

                    nameCanvas.localScale = new Vector3(0.01f,0.01f,0.01f);
                    barCanvas.localScale = new Vector3(0.01f,0.01f,0.01f);
                }

                
            }else{
                PlayAnimation("Dash");
            }
        }else{
            if(!isDashing){
                PlayAnimation("Idle");
            }
        }

        if(isFalling){
            PlayAnimation("Fall");
        }
    } // end HandleBotAnimation

    public void PlayAnimation(string animName){
        switch(animName){
            case "Idle":
                anim.SetBool("Run", false);
                anim.SetBool("Dash", false);
                anim.SetBool("Fall", false);
            break;

            case "Run":
                anim.SetBool("Run", true);
                anim.SetBool("Dash", false);
                anim.SetBool("Fall", false);
            break;

            case "Dash":
                anim.SetBool("Run", false);
                anim.SetBool("Dash", true);
                anim.SetBool("Fall", false);
            break;

            case "Fall":
                anim.SetBool("Run", false);
                anim.SetBool("Dash", false);
                anim.SetBool("Fall", true);
            break;
        } // end switch
    } // end PlayAnim

    #region DASH RELATED
    void InvokeDash(){
        StartCoroutine(BotDash(.2f));
    }

    public IEnumerator BotDash(float duration){
        if(!isDashing && !isFalling && !agent.botDestinationReach){
            print("DASHH");
            PlayAnimation("Dash"); // Play Dash Animation
            float _oldSpeed = agent.maxSpeed; // Store old speed
            agent.maxSpeed = agent.maxSpeed * 3f; // Multiple current movespeed to multiplier

            isDashing = true; // Set isDashing = true

            yield return new WaitForSeconds(duration); // Waiting duration

            GetComponent<Rigidbody2D>().velocity = Vector2.zero; // Force velocity = 0
            agent.maxSpeed = _oldSpeed; // Revert back to old speed
            isDashing = false; // Set isDashing = false
        }
    } // end PlayerDash

    public void StunnedByCat(float duration, Vector3 catPos){
        if(!catStunned){
            // Falling
            //playerController.gameObject.GetPhotonView().RPC("PlayerFall", RpcTarget.All, duration);
            StartCoroutine(BotFalling(duration));
            // Spawn Love Emote
            var loveableEfx = PhotonNetwork.Instantiate(NetworkManager.GetPhotonPrefab("Particles", "Loveable"), (transform.position + new Vector3(0, 1f, 0)), Quaternion.identity);
            catStunned = true;
        }else{
            print("What happen?");
        }
    }

    public IEnumerator PauseMovement(float dur){
        pauseMovement = true;
        yield return new WaitForSeconds(dur);
        pauseMovement = false;
    } // end PauseMovement

    public void SlowMovement(bool isEnable){
        if(isEnable){
            agent.maxSpeed = 4f;
        }else{
            agent.maxSpeed = 8f;
        }
    } // end SlowMovement

#endregion // end DASH RELATED
}
