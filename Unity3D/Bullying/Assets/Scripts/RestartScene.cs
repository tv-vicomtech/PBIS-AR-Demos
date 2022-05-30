using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RestartGame());
    }

    IEnumerator RestartGame()
    {
       yield return new WaitForSecondsRealtime(0.2f);
       
       SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
 
}
