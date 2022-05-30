using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Init orkestra session
/// </summary>
public class InitSession : MonoBehaviour
{
    private TMP_InputField roomField;
    private TMP_InputField userField;

    void Start()
    {
        userField = GameObject.Find("InputField_User").GetComponent<TMP_InputField>();
        roomField = GameObject.Find("InputField_Room").GetComponent<TMP_InputField>();
    }

    public void SaveValues()
    {
        Debug.Log(userField.text + " " + roomField.text);

        //Room name
        PlayerPrefs.SetString("room", roomField.text);
        //Agent id
        PlayerPrefs.SetString("agentID", userField.text);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
