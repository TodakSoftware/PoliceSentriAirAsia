using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableThis : MonoBehaviour
{
    public void DisableObject(){
        gameObject.SetActive(false);
    }
}
