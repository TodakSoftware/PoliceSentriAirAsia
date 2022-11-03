using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SimpleObjectWaypoint : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform[] waypoints;
    private GameObject ownerObject;
    private Animator ownerAnimator;

    [Header("Object Related")]
    [SerializeField] float moveSpeed = 5f;
    public float pauseMove = 1f;
    private float waitTime;
    public float hitPlayerPause = 3f;
    public float startSpawnDelay;
    
    [Header("Sequence Related")]
    public bool dontLoop;
    private bool canMove;
    private int currentWaypointIndex;

    [Header("Random Related")]
    public bool randomMove;
    public bool firstSpawnRandom;
    private int randomWaypointIndex;
    private bool playRunAnim;

    [Header("Special Case Airasia")]
    public bool changeSpriteOnFlip;
    public Sprite leftSprite, rightSprite; // leftSprite if going to left, vice versa
    
    void Awake(){
        ownerObject = this.gameObject;
        if(ownerObject != null && ownerObject.GetComponent<Animator>() != null){
            ownerAnimator = ownerObject.GetComponent<Animator>();
        }
    }

    void Start()
    {
        if(waypoints.Length != 0){
            if(randomMove){
                waitTime = pauseMove;
                randomWaypointIndex = Random.Range(0, waypoints.Length);
                if(firstSpawnRandom){
                    transform.position = waypoints[randomWaypointIndex].position;
                }else{
                    transform.position = waypoints[0].position;
                }
            }else{
                transform.position = waypoints[0].position;
            }
            
            //canMove = true;
            Invoke("DelayCanMove", startSpawnDelay);
        }
    }

    void DelayCanMove(){
        canMove = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(canMove && !GameManager.instance.gameEnded && GameManager.instance.gameStarted){
        //if(canMove){
            if(waypoints.Length != 0){
                if(randomMove){
                    transform.position = Vector2.MoveTowards(transform.position, waypoints[randomWaypointIndex].position, moveSpeed * Time.deltaTime);
                    if(ownerAnimator != null && !playRunAnim){
                        ownerAnimator.SetBool("Run", true);
                        playRunAnim = true;
                    }
                    if(Vector2.Distance(transform.position, waypoints[randomWaypointIndex].position) < 0.2f){
                        if(waitTime <= 0){
                            randomWaypointIndex = Random.Range(0, waypoints.Length);
                            waitTime = pauseMove;

                            if((waypoints[randomWaypointIndex].position.x - transform.position.x) > 0f){
                                //GetComponent<SpriteRenderer>().flipX = true;
                                photonView.RPC("FlipRight", RpcTarget.All);
                            }else{
                                GetComponent<SpriteRenderer>().flipX = false;
                                photonView.RPC("FlipLeft", RpcTarget.All);
                            }

                            if(ownerAnimator != null && playRunAnim){
                                ownerAnimator.SetBool("Run", false);
                                playRunAnim = false;
                            }
                        }else{
                            waitTime -= Time.deltaTime;
                        }
                    }

                    
                }else{
                    // if sequence
                    transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, moveSpeed * Time.deltaTime);
                    

                    if(Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.2f){
                        if(waitTime <= 0){
                            if(currentWaypointIndex == (waypoints.Length - 1)){
                                currentWaypointIndex = 0;
                            }else{
                                currentWaypointIndex += 1;
                            }
                            waitTime = pauseMove;

                            if((waypoints[currentWaypointIndex].position.x - transform.position.x) > 0f){
                                //GetComponent<SpriteRenderer>().flipX = true;
                                photonView.RPC("FlipRight", RpcTarget.All);
                            }else{
                                //GetComponent<SpriteRenderer>().flipX = false;
                                photonView.RPC("FlipLeft", RpcTarget.All);
                            }

                            if(ownerAnimator != null && !playRunAnim){
                                ownerAnimator.SetBool("Run", true);
                                playRunAnim = true;
                            }

                        }else{
                            waitTime -= Time.deltaTime;

                            if(ownerAnimator != null && playRunAnim){
                                ownerAnimator.SetBool("Run", false);
                                playRunAnim = false;
                            }

                        }
                    }
                    
                }
            } // end waypoint length

        } // end isMoving
    }

    [PunRPC]
    public void FlipRight(){
        if(!changeSpriteOnFlip){
            GetComponent<SpriteRenderer>().flipX = true;
        }else{
            GetComponent<SpriteRenderer>().sprite = rightSprite;
        }
    }

    [PunRPC]
    public void FlipLeft(){
        if(!changeSpriteOnFlip){
            GetComponent<SpriteRenderer>().flipX = false;
        }else{
            GetComponent<SpriteRenderer>().sprite = leftSprite;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Police") || other.gameObject.CompareTag("Robber") && GameManager.instance.gameStarted){
            if(canMove){
                canMove = false;
                Invoke("ResetCanMove", hitPlayerPause);

                if(ownerAnimator != null && playRunAnim){
                    ownerAnimator.SetBool("Run", false);
                    playRunAnim = false;
                }
            }
        }
    }

    void ResetCanMove(){
        if(!canMove){
            canMove = true;
            print("Resume");

            if(ownerAnimator != null && !playRunAnim){
                ownerAnimator.SetBool("Run", true);
                playRunAnim = true;
            }
        }
    }
}
