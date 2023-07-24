using UnityEngine;

public class Water : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Police") || other.CompareTag("Robber")){
            other.GetComponent<PlayerController>().EnableWaterSlowMovement();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Police") || other.CompareTag("Robber")){
            if(!other.GetComponent<PlayerController>().isSlow){
                other.GetComponent<PlayerController>().EnableWaterSlowMovement();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Police") || other.CompareTag("Robber")){
            other.GetComponent<PlayerController>().DisableSlowMovement();
        }
    }
}
