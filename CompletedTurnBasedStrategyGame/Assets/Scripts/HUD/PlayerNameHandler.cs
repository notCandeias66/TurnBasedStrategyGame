using UnityEngine;
using UnityEngine.UI;

public class PlayerNameHandler : MonoBehaviour
{
    public TMPro.TMP_InputField nameInputField;
    public TMPro.TMP_Text nameDisplayText;

    public void OnNameInputChanged()
    {
        string playerName = nameInputField.text;
        nameDisplayText.text = playerName;
    }
}
