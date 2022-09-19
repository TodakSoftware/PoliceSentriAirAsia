using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiPoliceNEW : MonoBehaviour
{
    //public Transform target;

    private PolyNavAgent _agent;
    //private PolyNavAgent agent;

    public float EnemyDistanceRun = 4.0f;

    public bool chaseRobber;

    void Start()
    {
         _agent = GetComponent<PolyNavAgent>();
    }

    void Update()
    {
       getMoneyBag();

       if (!chaseRobber){
        chaseRobber = true;

       }
     
    }

    void getMoneyBag(){

        if(chaseRobber){
            //_agent.SetDestination(GetClosestMoneybag().position);
            _agent.SetDestination(GetClosestEnemy("Robber").position);
            chaseRobber = false;
        }
    }

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

}
