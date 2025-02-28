using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerIDInput : MonoBehaviour
{
    public InputField playerIDInput;

    // Call this method on your submit button's OnClick event.
    public void OnSubmitID()
    {
        string id = playerIDInput.text;
        // Save the player's college id to PlayerPrefs
        PlayerPrefs.SetString("PlayerID", id);
        // Optionally disable the panel rather than destroying it
       
    }
}
