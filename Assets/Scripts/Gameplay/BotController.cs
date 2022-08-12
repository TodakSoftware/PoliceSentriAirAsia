using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class BotController : PlayerController
{
    [Header("Bot Related")]
    public PolyNavAgent botAgent;
    public GameObject currentTarget;
    GameObject previousTarget;
    public bool isGoingToTarget;
    List<GameObject> cacheRobbers, cachePolices;
    public bool isAggresive; // random on start
    bool doneCachePlayers;
    bool doneSearchingNewTarget;
    bool canSearch,stopSearch; // If true, stop HandleTarget looping
    bool doneExecute;
    [SerializeField] float reactDelay;
    public float detectRange = 3f;
    public bool isRoaming;
    List<Transform> randomGoToPositions = new List<Transform>();

    public bool inRange;
    public float idleTimer, changeDirectionTimer;
    bool changeDirectionFromPolice;
    
    void Start()
    {
        if(gameObject.tag == "Police"){
            reactDelay = Random.Range(0.6f, .8f);
        }else{
            reactDelay = 0.3f;
        }
        
        if(GameManager.instance.gameStarted && !doneCachePlayers){
            cacheRobbers = GameManager.GetAllPlayersRobber();
            cachePolices = GameManager.GetAllPlayersPolice();
            doneCachePlayers = true;
            //StartCoroutine(HandleTargeting());
        }

        foreach(var p in GameManager.instance.policeSpawnpoints){
            randomGoToPositions.Add(p);
        }

        foreach(var r in GameManager.instance.robberSpawnpoints){
            randomGoToPositions.Add(r);
        }
        
    }

    IEnumerator DelaySearchAllowed(){
        yield return new WaitForSeconds(Random.Range(1, 3));
        canSearch = true;
        StartCoroutine(HandleTargeting());
    }
    

    // Update is called once per frame
    void Update()
    {
        HandleBotFacing();
        HandleBotAnimation();

        if(GameManager.instance.gameStarted && !GameManager.instance.gameEnded){
            if(!doneExecute){
                doneExecute = true;
                StartCoroutine(DelaySearchAllowed());
            }
            
            if(canSearch){
                if(currentTarget != null && gameObject.tag == "Police"){
                    // if out of range, null kan target
                    Vector3 directionToTarget = currentTarget.transform.position - transform.position;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if(dSqrToTarget < detectRange * 100f)
                    {
                        inRange = true;
                    }else{
                        inRange = false;
                        currentTarget = null;
                        doneSearchingNewTarget = false;
                    }
                }else if(currentTarget == null && gameObject.tag == "Police" && isRoaming && !isGoingToTarget){ //botAgent.botDestinationReach
                    BotFindRandomGoToPoint();
                }

                if(currentTarget == null && botAgent.botDestinationReach && isGoingToTarget){
                    isRoaming = false;
                    isGoingToTarget = false;
                }
                
            } // end canSearch
        } // end gameStarted && gameEnd

    } // end Update

#region AI BOT RELATED
    public IEnumerator HandleTargeting(){
        switch(gameObject.tag){
            case "Robber":  // if we are robber, find moneybag, run from police, save teammates
                if(!GetComponent<Robber>().isCaught){
                    if(!GameManager.instance.moneyBagOccupied){
                        BotRobberFindMoneybag();
                    }else{
                        if(!changeDirectionFromPolice){
                            BotFindRandomGoToPoint();
                        }
                        
                        if(GetClosestEnemy("Police") != null){
                            if(changeDirectionTimer <= 0 && isRoaming){
                                BotRobberAvoidPolice();
                                //changeDirectionTimer = 0;
                            }else if(changeDirectionTimer > 0){
                                changeDirectionTimer -= Time.deltaTime;
                            }
                        }else{
                            if(changeDirectionTimer != 0){
                                changeDirectionTimer = 0;
                            }
                        }
                        
                    }
                }
                
            break;

            case "Police":  // if we are police, find robber that !caught

                if(isAggresive){ // Lock 1 target
                    if(currentTarget == null){ // 1 time
                        if(!doneSearchingNewTarget){
                            BotPoliceFindRobber();
                            doneSearchingNewTarget = true;
                        }
                    }else if(currentTarget != null && currentTarget.GetComponent<Robber>().isCaught){ // if the target is Caught, clear n find another uncaught robber
                        print("Caught & Clear Target");
                        currentTarget = null;
                        doneSearchingNewTarget = false;
                        inRange = false;
                        BotFindRandomGoToPoint();
                    }
                }else{ // Change able when nearby
                    BotPoliceFindRobber();
                }
            break;

            default:
            break;
        } // end switch

        yield return new WaitForSeconds(reactDelay);

        if(!stopSearch){
            StartCoroutine(HandleTargeting());
        }
    } // end HandleTargeting

    public void BotPoliceFindRobber(){
        if(GetClosestEnemy("Robber") != null && !GetClosestEnemy("Robber").gameObject.GetComponent<Robber>().isCaught){ // Check robber is !caught
            currentTarget = GetClosestEnemy("Robber").gameObject;
            previousTarget = currentTarget;
            GoToTarget(currentTarget.transform);

            if(isRoaming){
                isRoaming = false;
            }
        }else if(currentTarget != null && currentTarget.GetComponent<Robber>() != null && currentTarget.GetComponent<BotController>() != null && currentTarget.GetComponent<Robber>().isCaught){
            currentTarget.GetComponent<BotController>().botAgent.Stop();
            currentTarget = null;
            previousTarget = currentTarget;

            doneSearchingNewTarget = false;
            inRange = false;
            BotFindRandomGoToPoint();

        }else if(currentTarget == null && !isRoaming){
            BotFindRandomGoToPoint();
        }
    }

    public void BotRobberFindMoneybag(){
        if(GetClosestMoneybag() != null && !GameManager.instance.moneyBagOccupied){ // Check robber is !caught
            var targetMoneybag = GetClosestMoneybag().gameObject;
            GoToTarget(targetMoneybag.transform);
        }
    }

    public void BotRobberAvoidPolice(){
        if(GetClosestEnemy("Police") != null && isRoaming && changeDirectionTimer <= 0){ // Check robber is !caught
            var newTarget = GetRandomPositionTransform();
            GoToTarget(newTarget.transform);
            changeDirectionTimer = .03f; 
        }
    }

    public IEnumerator BotRobberGotoEscapePoint(){
        yield return new WaitForSeconds(2f);
        var randomIndex = Random.Range(0, GameManager.instance.botEscapeSpawnpoints.Count);
        //if(GetComponent<Robber>() != null && GetComponent<Robber>().isCaught){ // Check robber is !caught
            GoToTarget(GameManager.instance.botEscapeSpawnpoints[randomIndex]);
            isRoaming = false;
        //}
    }

    public void BotFindRandomGoToPoint(){
        
        switch(gameObject.tag){
            case "Robber":
                // If currentTarget is Null, find random go to point, and set go to
                if(currentTarget == null && !isRoaming && !GetComponent<Robber>().isCaught){ //  && !isGoingToTarget
                    isRoaming = true;
                    GoToTarget(GetRandomPositionTransform());
                }
            break;

            case "Police":
                // If currentTarget is Null, find random go to point, and set go to
                if(currentTarget == null && !isRoaming){ //  && !isGoingToTarget
                    isRoaming = true;
                    GoToTarget(GetRandomPositionTransform());
                }
            break;
        }
    }

    Transform GetRandomPositionTransform(){
        var randomIndex = Random.Range(0, randomGoToPositions.Count);
        return randomGoToPositions[randomIndex];
    }
    
    public void HandleBotFacing(){
        if(botAgent.botFacingRight){
            FlipRight(true);
        }else{
            FlipRight(false);
        }
    } // end HandleBotFacing

    public void HandleBotAnimation(){
        if(botAgent.currentSpeed > 0){
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
    } // end HandleBotAnimation

    public Transform GetClosestEnemy(string team)
    {
        Transform bestTarget = null;
        //float closestDistanceSqr = Mathf.Infinity;
        float closestDistanceSqr = detectRange * 100f;
        Vector3 currentPosition = transform.position;
        if(team == "Robber"){
            //foreach(GameObject potentialTarget in cacheRobbers){
            foreach(GameObject potentialTarget in GameManager.GetAllPlayersRobber()){
                if(!potentialTarget.GetComponent<Robber>().isCaught){
                    Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if(dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = potentialTarget.transform;
                    }
                }
            }
        }else{
            foreach(GameObject potentialTarget in GameManager.GetAllPlayersPolice()){
            //foreach(GameObject potentialTarget in cachePolices){
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if(dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.transform;
                }
            }
        }
        
        return bestTarget;
    } // end GetClosestEnemy

    public Transform GetClosestMoneybag()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        
        foreach(GameObject potentialTarget in GameManager.GetAllMoneybag()){
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }
        
        return bestTarget;
    } // end GetClosestEnemy

    public void GoToTarget(Transform target){
        idleTimer = 15f;
        botAgent.SetDestination(target.position);
        isGoingToTarget = true;
        botAgent.botDestinationReach = false;
    }
    
#endregion // End AI BOT RELATED
}
