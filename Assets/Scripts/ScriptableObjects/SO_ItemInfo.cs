using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ItemDetails{
    public string name;
    public Sprite itemIcon;
    public float effectDuration;
}


[CreateAssetMenu(fileName = "New Item Info", menuName = "Database/Item Info")]
public class SO_ItemInfo : ScriptableObject
{
    public List<ItemDetails> itemsDetails;
    
}
