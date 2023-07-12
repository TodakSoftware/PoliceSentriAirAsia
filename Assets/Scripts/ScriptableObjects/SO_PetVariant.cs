 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct petAnimatorInfo{
    public string name;
    public string code;
    public Sprite iconHead;
    public Sprite idlePose;
    public RuntimeAnimatorController runTimeAnimController;
    public bool flyPet;
    public bool temporaryDisable;
    public E_ValueType type;
    public float kupang;
    public float airAsiaPoints;
}


[CreateAssetMenu(fileName = "New Pet Variant", menuName = "Database/Pet Variant")]
public class SO_PetVariant : ScriptableObject
{
    public List<petAnimatorInfo> animatorLists;
    
}
