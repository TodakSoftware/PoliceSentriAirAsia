using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorEffect : MonoBehaviour
{

    public E_FloorEffectType type;
    private int randomSlide; 
    public bool potHoleOccupied;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
            randomSlide = Random.Range(0, 2);
            switch(type){
                case E_FloorEffectType.SLIDE:
                    if((other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot) || (other.CompareTag("Police") && !other.GetComponent<Police>().isBot)){
                        if(other.GetComponent<PlayerController>().isFalling != false){
                            other.GetComponent<PlayerController>().isSlide = true;
                        }else{
                            other.GetComponent<PlayerController>().isSlide = false;
                            other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                        }
                    }else{
                        // bot Slide
                        /* if(other.CompareTag("Robber") && other.GetComponent<Robber>().isBot){
                            if(other.GetComponent<AIRobber>().isFalling){
                                other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                            }
                        }else if(other.CompareTag("Police") && other.GetComponent<Police>().isBot){
                            if(other.GetComponent<AIPolice>().isFalling){
                                other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                            }
                        } */
                    }
                break;

                case E_FloorEffectType.SLOW:
                    if((other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot) || (other.CompareTag("Police") && !other.GetComponent<Police>().isBot)){
                        if(!other.GetComponent<PlayerController>().isSlow){
                            if(other.CompareTag("Robber") && other.GetComponent<Robber>().isCaught){
                                // Do Nothing
                            }else{ // else if police
                                if(!other.GetComponent<PlayerController>().isDashing){
                                    other.GetComponent<PlayerController>().EnableSlowMovement();
                                    other.GetComponent<PlayerController>().isSlow = true;
                                }
                            }
                        }
                    }else{
                        // bot slow movement
                        if(other.CompareTag("Robber") && other.GetComponent<Robber>().isBot){
                            other.GetComponent<AIRobber>().SlowMovement(true);
                        }else if(other.CompareTag("Police") && other.GetComponent<Police>().isBot){
                            other.GetComponent<AIPolice>().SlowMovement(true);
                        }
                    }
                break;

                case E_FloorEffectType.FALL:
                    
                break;
            } // end switch
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
            switch(type){
                case E_FloorEffectType.SLIDE:
                    if((other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot) || (other.CompareTag("Police") && !other.GetComponent<Police>().isBot)){
                        if(other.GetComponent<PlayerController>().isFalling == false){
                            if(randomSlide == 0){
                                other.GetComponent<Rigidbody2D>().gravityScale = 20f;
                            }else{
                                other.GetComponent<Rigidbody2D>().gravityScale = -20f;
                            }
                        }else{
                            other.GetComponent<PlayerController>().isSlide = false;
                            other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                        }
                    }else{
                        // Bot related
                        // bot Slide
                        /* if(other.CompareTag("Robber") && other.GetComponent<Robber>().isBot){
                            if(!other.GetComponent<AIRobber>().isFalling){
                                if(randomSlide == 0){
                                    other.GetComponent<Rigidbody2D>().gravityScale = 20f;
                                }else{
                                    other.GetComponent<Rigidbody2D>().gravityScale = -20f;
                                }
                            }else{
                                other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                            }
                        }else if(other.CompareTag("Police") && other.GetComponent<Police>().isBot){
                            if(!other.GetComponent<AIPolice>().isFalling){
                                if(randomSlide == 0){
                                    other.GetComponent<Rigidbody2D>().gravityScale = 20f;
                                }else{
                                    other.GetComponent<Rigidbody2D>().gravityScale = -20f;
                                }
                            }else{
                                other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                            }
                        } */
                    }
                break;

                case E_FloorEffectType.SLOW:
                    
                break;

                case E_FloorEffectType.FALL:
                    
                break;
            } // end switch
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Robber") || other.CompareTag("Police")){
            switch(type){
                case E_FloorEffectType.SLIDE:
                    if((other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot) || (other.CompareTag("Police") && !other.GetComponent<Police>().isBot)){
                        other.GetComponent<PlayerController>().isSlide = false;
                        other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                    }else{
                        /* // bot related
                        if(other.CompareTag("Robber") && other.GetComponent<Robber>().isBot){
                            other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                        }else if(other.CompareTag("Police") && other.GetComponent<Police>().isBot){
                            other.GetComponent<Rigidbody2D>().gravityScale = 0f;
                        } */
                    }
                break;

                case E_FloorEffectType.SLOW:
                if((other.CompareTag("Robber") && !other.GetComponent<Robber>().isBot) || (other.CompareTag("Police") && !other.GetComponent<Police>().isBot)){
                    if(other.GetComponent<PlayerController>().isSlow){
                        other.GetComponent<PlayerController>().DisableSlowMovement();
                        other.GetComponent<PlayerController>().isSlow = false;
                    }
                }else{
                    // bot related
                    if(other.GetComponent<Robber>().isBot){
                        other.GetComponent<AIRobber>().SlowMovement(false);
                    }else if(other.GetComponent<Police>().isBot){
                        other.GetComponent<AIPolice>().SlowMovement(false);
                    }
                }
                break;

                case E_FloorEffectType.FALL:
                    
                break;
            } // end switch
        }
    }
}
