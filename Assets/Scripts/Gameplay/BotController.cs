using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class BotController : PlayerController
{
    [Header("Bot Related")]
    public PolyNavAgent botAgent;
    float detectRange = 3f;
    public LayerMask maskLayer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 10f, maskLayer);
  
        if(hit)
            Debug.Log("wall detected");
    } // end Update

#region AI BOT RELATED

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
    
#endregion // End AI BOT RELATED
}
