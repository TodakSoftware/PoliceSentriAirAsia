using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PolyNavAgent))]
public class AIRobber : MonoBehaviourPunCallbacks
{
    PolyNavAgent agent;
    public Transform target;
    public Transform roamTarget;
    public E_Team targetTagTeam;
    string targetTag = "";
    public float detectRange = 400f;
    public bool isHitWall, isRescuing, rescueExecuted;
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
    public int releaseCount;
    [Header("Item Related")]
    public bool catStunned;

    void Awake(){
        agent = GetComponent<PolyNavAgent>();

        // Set isBot
        GetComponent<Robber>().isBot = true;

        // Set Bot Name
        playerNameText.text = "Robber" + photonView.OwnerActorNr;

        // Set PolyNav
        agent.map = GameObject.FindObjectOfType<PolyNav2D>();

        // Add Random Location
        foreach(var p in GameManager.instance.moneybagSpawnpoints){
            randomGoToPositions.Add(p);
        }

        InitBot();
    }

    public void InitBot(){
        InvokeRepeating("CollectMoneybag", 0f, .5f);
        InvokeRepeating("AvoidPoliceNearby", 0f, .02f);
        InvokeRepeating("UpdateRoaming", 0f, .8f);
        InvokeRepeating("InvokeDash", 4f, 7f);
    }

    void Update(){
        if(target != null){
            if(target.CompareTag("MoneyBag")){
                if(!GameManager.instance.moneyBagOccupied){
                    agent.SetDestination(target.position);
                }else{
                    agent.Stop();
                    ClearTarget();
                }
            }else{
                agent.SetDestination(target.position);
            }
        }

        if(isRescuing && agent.botDestinationReach && !GetComponent<Robber>().isCaught){ // if we in rescue mode && arrived, 
            // wait 5s then isRescuing = false
            if(!rescueExecuted){
                Invoke("ClearRescue", 3f);
                rescueExecuted = true;
            }
        }

        /* if(Input.GetKeyDown(KeyCode.N) && GetComponent<Robber>().isCaught){
            GoInsidePrison();
        } */

        if(isFalling){
            agent.Stop();
        }

        HandleBotAnimation();
    }

    void AvoidPoliceNearby(){
        if(GetClosestEnemy("Police") != null && !GetComponent<Robber>().isCaught && !isHitWall){
            ClearRescue();
            ClearTarget();
            ClearRoamTarget();

            Vector3 dirToPlayer = (transform.position - GetClosestEnemy("Police").position).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer * 3f);
            if(hit.collider.gameObject.CompareTag("Walls") || hit.collider.gameObject.CompareTag("Objects")){
                //print("Hit Wall");  
                isHitWall = true;
            }else{
                isHitWall = false;
                agent.SetDestination(transform.position + dirToPlayer * 3f);
                Debug.DrawRay(transform.position, dirToPlayer * 3f, Color.green);
            }
        }else if(GetClosestEnemy("Police") == null && isHitWall && !GetComponent<Robber>().isCaught){
            isHitWall = false;
        }
    }

    void UpdateRoaming(){
        if(roamTarget == null && target == null && !isRescuing && !GetComponent<Robber>().isCaught){
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

    void CollectMoneybag(){
        if(GetClosestMoneybag() != null && !GameManager.instance.moneyBagOccupied && GetClosestEnemy("Police") == null && !GetComponent<Robber>().isCaught && !isRescuing){
            target = GetClosestMoneybag();
            ClearRoamTarget();
        }else{
            //print("No Moneybag");
            ClearTarget();
        }
    }

    void ClearTarget(){
        target = null;
    }

    void ClearRoamTarget(){
        roamTarget = null;
    }

    void ClearRescue(){
        print("Clear resuc");
        isRescuing = false;
        rescueExecuted = false;
    }

    public void RescueTeammate(){
        isRescuing = true;
        target = GetNearJailPosition();
        ClearRoamTarget();
    }

    public void GoInsidePrison(){ // go to specific location inside prison to be saved
        if(GetComponent<Robber>().isCaught){
            target = GetNearInsideJailPosition();
            ClearRoamTarget();
        }
    }

    // ------------------------------------- GLOBAL ------------------------------------------
    public Transform GetClosestMoneybag()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        
        foreach(GameObject potentialTarget in GameObject.FindGameObjectsWithTag("MoneyBag")){
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                return bestTarget;
            }
        }
        
        return null;
    } // end GetClosestEnemy

    public float GetTargetDistance(Transform target){
        if(target != null){
            float distance = Vector3.Distance(transform.position, target.position);
            return distance;
        }
        
        return 0;
    } // end GetTargetDistance

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
            }else{
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

    Transform GetNearJailPosition(){ // outside
        var randomIndex = Random.Range(0, GameManager.instance.botEscapeSpawnpoints.Count);
        return GameManager.instance.botEscapeSpawnpoints[randomIndex].GetChild(0);
    }

    Transform GetNearInsideJailPosition(){ // inside prison
        var randomIndex = Random.Range(0, GameManager.instance.botEscapeSpawnpoints.Count);
        return GameManager.instance.botEscapeSpawnpoints[randomIndex];
    }

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

        if(catStunned){
            catStunned = false;
        }
        //Roaming();
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
        if(!GetComponent<Robber>().isCaught){
            StartCoroutine(BotDash(.2f));
        }
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

    //[PunRPC]
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

#endregion // end DASH RELATED

    public void SlowMovement(bool isEnable){
        if(isEnable){
            agent.maxSpeed = agent.maxSpeed / 2;
        }else{
            agent.maxSpeed = 8f;
        }
    } // end SlowMovement
}
