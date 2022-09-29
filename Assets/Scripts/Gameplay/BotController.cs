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
    // Rescue Related
    public bool willSaveTeammate, suicideRescue, isGoingToRescue; // "willSaveTeammate = TRUE, will go save teammate. FALSE, will NEVER rescue | suicideRescue (if willSaveTeammate TRUE), TRUE = will stay until teammate rescued. FALSE = run away if police nearby"
    public Transform targetRescue;
    public Transform escapePointTarget;
    public float useDashTimer;
    Vector2 useDashMinMax = new Vector2(7f, 10f);
    bool isGettingMoneybag;

    void Start()
    {
        if(gameObject.tag == "Police"){
            reactDelay = Random.Range(0.3f, .4f);
            playerNameText.text = "Police" + photonView.OwnerActorNr;
        }else{
            reactDelay = 0.2f;
            playerNameText.text = "Robber" + photonView.OwnerActorNr;
        }
        
        if(GameManager.instance.gameStarted && !doneCachePlayers){
            cacheRobbers = GameManager.GetAllPlayersRobber();
            cachePolices = GameManager.GetAllPlayersPolice();
            doneCachePlayers = true;
            StartCoroutine(HandleTargeting());
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

            if(GameManager.instance.moneyBagOccupied && isGettingMoneybag && gameObject.tag == "Robber" && !GetComponent<Robber>().isCaught){
                isGettingMoneybag = false;
                isGoingToRescue = false;
                BotRobberSaveTeammate();
            }


            if(botAgent.botDestinationReach && isGoingToTarget){
                isRoaming = false;
            }

            // Dash Related
            if(useDashTimer > 0){
                useDashTimer -= Time.deltaTime;
            }else{
                useDashTimer = 0;
                print("Use Dash!");
                if(isGoingToTarget && !isDashing && !botAgent.botDestinationReach){
                    if(gameObject.tag == "Robber" && !GetComponent<Robber>().isCaught){
                        StartCoroutine(BotDash(dashDuration));
                    }else if(gameObject.tag == "Police"){
                        StartCoroutine(BotDash(dashDuration));
                    }else{
                        useDashTimer = Random.Range(useDashMinMax.x, useDashMinMax.y);
                    }
                }else{
                    useDashTimer = Random.Range(useDashMinMax.x, useDashMinMax.y);
                }
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
                }
                
                if(currentTarget == null && isRoaming && !isGoingToTarget){ //botAgent.botDestinationReach
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

#region DASH RELATED
    public IEnumerator BotDash(float duration){
        if(!isDashing && !isDashCooldown && !isFalling){
            StartCoroutine(CooldownPlayerDash(dashCooldown)); // Set Button Cooldown
            playerAnim.PlayAnimation("Dash"); // Play Dash Animation
            float _oldSpeed = botAgent.maxSpeed; // Store old speed
            botAgent.maxSpeed = botAgent.maxSpeed * dashSpeedMultiplier; // Multiple current movespeed to multiplier

            isDashing = true; // Set isDashing = true
            useDashTimer = Random.Range(useDashMinMax.x, useDashMinMax.y);

            yield return new WaitForSeconds(duration); // Waiting duration

            rb.velocity = Vector2.zero; // Force velocity = 0
            botAgent.maxSpeed = _oldSpeed; // Revert back to old speed
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

    public IEnumerator HandleTargeting(){

        switch(gameObject.tag){
            case "Robber":  // if we are robber, find moneybag, run from police, save teammates
                if(!GetComponent<Robber>().isCaught){
                    if(!GameManager.instance.moneyBagOccupied){ // if no money taken, find moneybag
                        BotRobberFindMoneybag();
                    }else{
                        if(!changeDirectionFromPolice && !isGoingToRescue){ // if not police nearby, go random point
                            BotFindRandomGoToPoint();
                        }
                        
                        if(GetClosestEnemy("Police") != null){ // if police nearby && !saveteammate, change new point
                            if(changeDirectionTimer <= 0 && isRoaming && !suicideRescue){
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
                    } // end if moneybag occupied or not

                    if(targetRescue != null && !targetRescue.GetComponent<Robber>().isCaught && isGoingToRescue){
                        targetRescue = null;
                        isGoingToRescue = false;
                    }

                    // if not suiceideRescue, we clear when police nearby
                }else{
                    
                }/*else if(willSaveTeammate && !GetComponent<Robber>().isCaught && isGoingToRescue && targetRescue != null){
                    //GoToTarget(targetRescue);
                    BotRobberSaveTeammate();
                }else if(willSaveTeammate && isGoingToRescue && targetRescue != null && !targetRescue.GetComponent<Robber>().isCaught){
                    targetRescue = null;
                    isGoingToRescue = false;
                } // end if !caught */
                
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
        if(GetClosestMoneybag() != null && !GameManager.instance.moneyBagOccupied && !isGettingMoneybag){ // Check robber is !caught
            var targetMoneybag = GetClosestMoneybag().gameObject;
            GoToTarget(targetMoneybag.transform);
            isGettingMoneybag = true;
            isGoingToRescue = false;
        }
    }

    public void BotRobberSaveTeammate(){
        if(willSaveTeammate){
            var filterTarget = GetClosestCapturedRobber();
            
            if(filterTarget != null && filterTarget.GetComponent<Robber>().isInPrison){ // Check robber is caught
                targetRescue = filterTarget;
                if(targetRescue.GetComponent<BotController>() != null){
                    Transform child = targetRescue.GetComponent<BotController>().escapePointTarget.GetChild(0);
                    if(child != null){
                        GoToTarget(child);
                    }
                }else{
                    var randomPoint = Random.Range(0, GameManager.instance.botEscapeSpawnpoints.Count);
                    Transform child = GameManager.instance.botEscapeSpawnpoints[randomPoint].GetChild(0);
                    if(child != null){
                        GoToTarget(child);
                    }
                }
                
                isGoingToRescue = true;
            }
        }
    }

    public void BotRobberAvoidPolice(){
        if(GetClosestEnemy("Police") != null && isRoaming && changeDirectionTimer <= 0 && !suicideRescue){ // Check robber is !caught
            var newTarget = GetRandomPositionTransform();
            GoToTarget(newTarget.transform);
            changeDirectionTimer = .1f; 

            /*if(isGoingToRescue){
                isGoingToRescue = false;
                targetRescue = null;
            }*/
        }
    }

    public void BotFindRandomGoToPoint(){
        
        switch(gameObject.tag){
            case "Robber":
                // If currentTarget is Null, find random go to point, and set go to
                if(currentTarget == null && !isRoaming && !GetComponent<Robber>().isCaught){ //  && !isGoingToTarget
                    isRoaming = true;
                    if(isGoingToRescue){
                        isGoingToRescue = false;
                    }
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

    public Transform GetClosestCapturedRobber()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        //float closestDistanceSqr = detectRange * 100f;
        Vector3 currentPosition = transform.position;

        foreach(GameObject potentialTarget in GameManager.GetAllPlayersRobber()){
            if(potentialTarget.GetComponent<Robber>().isCaught){
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
