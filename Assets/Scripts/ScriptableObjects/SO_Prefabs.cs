using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Prefabs File", menuName = "Database/Prefabs")]
public class SO_Prefabs : ScriptableObject
{
    [Header("UI Prefab")]
    public GameObject modalReconnectGame;
    public GameObject modalLoadingScene;
    public GameObject manualRoleSelect;
    public GameObject p_CharacterSelect;
    public GameObject p_PetSelect;
    public GameObject p_PoliceWinUI, p_PoliceLoseUI, p_RobberWinUI, p_RobberLoseUI;
    public GameObject ui_GameplayFeeds;

    [Header("Button")]
    public GameObject btnCharacterSelectAvatar;
    public GameObject btnPetSelectAvatar;

    [Header("Photon Prefab Pooling")]
    public List<C_PhotonPrefabAttributes> characterPrefabs = new List<C_PhotonPrefabAttributes>();
    public List<C_PhotonPrefabAttributes> propsPrefabs = new List<C_PhotonPrefabAttributes>();
    public List<C_PhotonPrefabAttributes> particlePrefabs = new List<C_PhotonPrefabAttributes>();
}
