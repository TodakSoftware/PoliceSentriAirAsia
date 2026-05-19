
using UnityEngine;
using UnityEngine.UI;

public class DeleteData : MonoBehaviour
{
    public Button deleteButton;
    public void ClearAllPlayerPrefs()
    {
        UserDataManager.instance.DeleteFromFirebase(deleteButton);
    }
}
