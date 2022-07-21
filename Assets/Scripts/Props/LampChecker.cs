using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LampChecker : MonoBehaviour
{
    public GameObject spotlight;
    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Map07"){
            spotlight.SetActive(true);
        }else{
            spotlight.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
