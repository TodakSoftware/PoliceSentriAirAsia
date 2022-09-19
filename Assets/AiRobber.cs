using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiRobber : MonoBehaviour
{
    //public Transform target;

    private PolyNavAgent _agent;
    //private PolyNavAgent agent;

    public float EnemyDistanceRun = 4.0f;

    public LayerMask mask;
    public bool isHit;

    //We will check if enemy can flee to the direction opposite from the player, we will check if there are obstacles
    public bool isDirSafe = false;

    //We will need to rotate the direction away from the player if straight to the opposite of the player is a wall
    float vRotation = 0;

    public bool isAggresive, isGettingMoneybag, robberFlee;
    


    void Start()
    {
         _agent = GetComponent<PolyNavAgent>();
    }

    void Update()
    {
       RobberFlee();
       if (_agent.botDestinationReach && isDirSafe){
            isDirSafe = false;
       }

       if(!GameManager.instance.moneyBagOccupied){ // if nobody hold moneybag, robber find moneybag
             getMoneyBag();
       }else if (GameManager.instance.moneyBagOccupied && isGettingMoneybag)
       {
            isGettingMoneybag = false;
       }
    }

    void getMoneyBag(){

        if(!isGettingMoneybag){
            if(GetClosestMoneybag() != null){
                _agent.SetDestination(GetClosestMoneybag().position);
                isGettingMoneybag = true;
            }
        } 
    }

    void RobberFlee()
    {
        if(!isAggresive){
         float distance = Vector3.Distance(transform.position, GetClosestEnemy("Police").position);

        //Debug.Log("Distance: " + distance);

         if (distance < EnemyDistanceRun){

            while (!isDirSafe)
                    {
                        //Calculate the vector pointing from Player to the Enemy
                        Vector3 dirToPlayer = transform.position - GetClosestEnemy("Police").position;

                        //Calculate the vector from the Enemy to the direction away from the Player the new point
                        Vector3 newPos = transform.position + dirToPlayer;

                        //Rotate the direction of the Enemy to move
                        newPos = Quaternion.Euler(0, 0, vRotation) * newPos;

                        //Shoot a Raycast out to the new direction with 5f length (as example raycast length) and see if it hits an obstacle
                        isHit = Physics.Raycast(transform.position, newPos, out RaycastHit hit, 6f, mask);

                        if (hit.transform == null)
                        {
                            //If the Raycast to the flee direction doesn't hit a wall then the Enemy is good to go to this direction
                            _agent.SetDestination(newPos);
                           

                             if (isHit && (hit.transform.CompareTag("Walls") || hit.transform.CompareTag("Objects")))
                            {
                                vRotation -= 90;
                                isDirSafe = false;
                                Debug.Log("Hit Something");
                            }
                            else
                            {
                                //If the Raycast to the flee direction doesn't hit a wall then the Enemy is good to go to this direction
                                _agent.SetDestination(newPos);
                                isDirSafe = true;
                                
                            }
                            //isDirSafe = true;

                            robberFlee = true;
                        }

                        //Change the direction of fleeing is it hits a wall by 20 degrees
                        //if (isHit && hit.transform.CompareTag("Wall"))
                       
                    }
             }else {
                robberFlee = false;
             } 
        } // end !isAggresive
    } //end Robber Flee



    public Transform GetClosestEnemy(string team)
    {
        Transform bestTarget = null;
        //float closestDistanceSqr = Mathf.Infinity;
        float closestDistanceSqr = EnemyDistanceRun * 100f;
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
    } // end GetClosestMoneyBag
}
