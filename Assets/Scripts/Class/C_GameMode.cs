using UnityEngine;

// GAME MODE used in SO_GameSettings
[System.Serializable]
public class C_GameMode{ // gameModeIndex : 0 = Normal Hunt 3 v 1
    public string name; // for gamemode ref
    public int gameModeIndex;
    public string displayName; // same as name but maybe specific
    [TextArea] public string descriptions;
    
    [Header("Find Game Settings")]
    public int maxPolicePerGame; // Normal mode 6
    public int maxRobberPerGame; // Normal mode 4
    public float findGameTimeoutDuration; // <- 60 = 1:00

    [Header("Ingame Settings")]
    public int clockStartTime; // Clock time when the game START in integer. START : 3.26 = 206 (Hunt Mode)
    public int clockEndTime; // Clock time when the game END in integer. END : 3.33 = 213 (Hunt Mode)
}