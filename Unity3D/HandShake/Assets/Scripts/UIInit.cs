using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIInit : MonoBehaviour
{
    public GameObject sesionName_Input;
    private string sesionName;
    public GameObject boy_Button;
    public GameObject woman_Button;
    
    public void sesionChange()
    {
        sesionName = sesionName_Input.GetComponent<InputField>().text;
    }

    public void Student()
    {
        PlayerPrefs.SetString("Character", CharacterTypes.Boy);
        OrkestraSesion();
    }
    public void Teacher()
    {
        PlayerPrefs.SetString("Character", CharacterTypes.Woman);
        OrkestraSesion();
    }
    public void OrkestraSesion()
    {
        Debug.Log(sesionName);
        PlayerPrefs.SetString("OrkestraSesion", sesionName);
        SceneManager.LoadScene("GameScene");
    }


}
