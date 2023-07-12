using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_ValueType{
    FREE, PREMIUM
}

[System.Serializable]
public struct animatorInfo{
    public string name;
    public string code;
    public Sprite iconHead;
    public Sprite idlePose;
    public RuntimeAnimatorController runTimeAnimController;
    public bool temporaryDisable;
    public E_ValueType type;
    public float kupang;
    public float airAsiaPoints;
}


[CreateAssetMenu(fileName = "New Animator Variant", menuName = "Database/Animator Variant")]
public class SO_AnimatorVariant : ScriptableObject
{
    public new string name;
    public List<animatorInfo> animatorLists;
    
}
