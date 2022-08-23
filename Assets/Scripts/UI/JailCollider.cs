using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JailCollider : MonoBehaviour
{
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public Robber robber;
    [HideInInspector] public PlayerAbilities playerAbilities;

    void Start(){
        playerController = transform.parent.GetComponent<PlayerController>();
        robber = transform.parent.GetComponent<Robber>();
        playerAbilities = transform.parent.GetComponent<PlayerAbilities>();
    }
}
