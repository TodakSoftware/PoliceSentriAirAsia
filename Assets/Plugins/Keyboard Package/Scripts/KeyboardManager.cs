using UnityEngine;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager instance;
    public GameObject keyboardPrefab;
    [SerializeField] TMP_InputField textBox;

    private void Start()
    {
        instance = this;
        textBox.text = "";
    }

    public void DeleteLetter()
    {
        if(textBox.text.Length != 0) {
            textBox.text = textBox.text.Remove(textBox.text.Length - 1, 1);
        }
    }

    public void AddLetter(string letter)
    {
        if(textBox.text.Length < textBox.characterLimit){
            textBox.text = textBox.text + letter;
        }
    }

    public void SubmitWord()
    {
        keyboardPrefab.SetActive(false);
        //textBox.text = "";
        // Debug.Log("Text submitted successfully!");
    }
}
