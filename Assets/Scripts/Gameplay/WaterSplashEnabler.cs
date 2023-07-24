using UnityEngine;

public class WaterSplashEnabler : MonoBehaviour
{    
    public void SpawnFromPool()
    {
        switch(transform.parent.tag){
            case "Police":
                if(transform.parent.GetComponent<Police>().isOnWater){
                    // Get an object from the pool and set its position and rotation.
                    GameObject obj = ObjectPoolManager.Instance.GetObjectFromPool();
                    obj.transform.position = transform.position;
                    obj.transform.rotation = transform.rotation;
                }
            break;

            case "Robber":
                if(transform.parent.GetComponent<Robber>().isOnWater){
                    // Get an object from the pool and set its position and rotation.
                    GameObject obj = ObjectPoolManager.Instance.GetObjectFromPool();
                    obj.transform.position = transform.position;
                    obj.transform.rotation = transform.rotation;
                }
            break;

            default:
            break;
        }
        
    }
}
