using UnityEngine;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager instance;
    public GameObject keyboardPrefab;
    [SerializeField] TMP_InputField usernameInputTextBox;
    [SerializeField] TMP_InputField findInputTextBox;
    [SerializeField] TMP_InputField roomInputTextBox;
    public bool usernameInputHighlight;
    public bool findInputHighlight;
    public bool roomInputHighlight;

    private void Start()
    {
        instance = this;
        usernameInputTextBox.text = "";
        findInputTextBox.text = "";
    }

    public void FocusUsername(bool _focus){
        if(_focus){
            usernameInputHighlight = true;
        }else{
            usernameInputHighlight = false;
        }
    }

    public void FocusFindCode(bool _focus){
        if(_focus){
            findInputHighlight = true;
        }else{
            findInputHighlight = false;
        }
    }

    public void ClearFocus(){
        findInputHighlight = false;
        usernameInputHighlight = false;
        roomInputHighlight = false;
        keyboardPrefab.SetActive(false);
    }

    public void FocusRoom(bool _focus){
        if(_focus){
            roomInputHighlight = true;
        }else{
            roomInputHighlight = false;
        }
    }

    public void DeleteLetter()
    {
        if(usernameInputHighlight){
            if(usernameInputTextBox.text.Length != 0) {
                usernameInputTextBox.text = usernameInputTextBox.text.Remove(usernameInputTextBox.text.Length - 1, 1);
            }
        }

        if(findInputHighlight){
            if(findInputTextBox.text.Length != 0) {
                findInputTextBox.text = findInputTextBox.text.Remove(findInputTextBox.text.Length - 1, 1);
            }
        }

        if(roomInputHighlight){
            if(roomInputTextBox.text.Length != 0) {
                roomInputTextBox.text = roomInputTextBox.text.Remove(roomInputTextBox.text.Length - 1, 1);
            }
        }
    }

    public void AddLetter(string letter)
    {
        if(usernameInputHighlight){
            if(usernameInputTextBox.text.Length < usernameInputTextBox.characterLimit){
                usernameInputTextBox.text = usernameInputTextBox.text + letter;
            }
        }

        if(findInputHighlight){
            if(findInputTextBox.text.Length < findInputTextBox.characterLimit){
                findInputTextBox.text = findInputTextBox.text + letter;
            }
        }

        if(roomInputHighlight){
            if(roomInputTextBox.text.Length < roomInputTextBox.characterLimit){
                roomInputTextBox.text = roomInputTextBox.text + letter;
            }
        }
    }

    public void SubmitWord()
    {
        findInputHighlight = false;
        usernameInputHighlight = false;
        roomInputHighlight = false;
        keyboardPrefab.SetActive(false);
        //textBox.text = "";
        // Debug.Log("Text submitted successfully!");
    }
}
