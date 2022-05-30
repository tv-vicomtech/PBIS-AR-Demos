using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseCharacter : MonoBehaviour
{
    public InputField inputField;
    
    public Button boy, girl;

    public GameObject girlCharacter, boyCharacter;

    public void Start()
    {
        girlCharacter.transform.position.Set(girl.transform.position.x, 
            girlCharacter.transform.position.y, girlCharacter.transform.position.z);

        boyCharacter.transform.position.Set(boy.transform.position.x,
            boy.transform.position.y, boy.transform.position.z);
    }


    public void CharacterChosen(int character)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("SelectedCharacter", character);
        PlayerPrefs.SetInt("TimesTried", 0);
        PlayerPrefs.SetString("room","BULLYROOM_"+inputField.text);
        PlayerPrefs.SetInt("Score",0);
        SceneManager.LoadScene("GameScene");
    }
}
