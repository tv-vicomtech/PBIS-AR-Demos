using OrkestraLib;
using OrkestraLib.Exceptions;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using static OrkestraLib.Orkestra;

public class CharacterMovement : MonoBehaviour
{
    public GameObject Characters;
    public GameObject[] Bullies;
    public GameObject[] Helpers;
    public GameObject Bullied;

    public ARTrackedImageManager arTrackedImageManager;
    private ARSession arSession;
    public Canvas canvas;
    public Canvas scoreCanvas;
    public Text scoreText;

    private List<Animator> BulliesAnims = new List<Animator>();
    private List<Animator> HelpersAnims = new List<Animator>();
    private Animator BulliedAnim;
    
    List<SkinnedMeshRenderer> renders= new List<SkinnedMeshRenderer>();

    private Button[] Buttons;
    public Button Button_TryAgain;

    private OrkestraImpl orkestra;

    private int SelectedCharacter = 0;
    private int OtherCharacter = 1;
    private const int SCORE_EACH_GAME=1;
    private bool LocalResponse = false, RemoteResponse=false;
    private bool Win_LocalResponse = false, Win_RemoteResponse = false;
    private Action LocalResponseAction;
    private Action RemoteResponseAction;

    public bool IsGameEnded{ get; private set; }

    private void Awake()
    {
        renders.AddRange(Bullied.GetComponentsInChildren<SkinnedMeshRenderer>());
        Bullies.ToList().ForEach((e) => { renders.AddRange(e.GetComponentsInChildren<SkinnedMeshRenderer>()); });
        Helpers.ToList().ForEach((e) => { renders.AddRange(e.GetComponentsInChildren<SkinnedMeshRenderer>()); });
#if !UNITY_EDITOR
        RendererState(false);
#endif
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        arSession = FindObjectOfType<ARSession>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        IsGameEnded= false;
        orkestra = GameObject.Find("Orkestra").GetComponent<OrkestraImpl>();
        orkestra.RestartFunc = Restart;
        orkestra.AnimRemoteCharacterFunc = AnimRemoteCharacter;

        SelectedCharacter = PlayerPrefs.GetInt("SelectedCharacter");
        
        OtherCharacter = 0;
        if(SelectedCharacter==0)
        {
            OtherCharacter = 1;
        }
        
        foreach(GameObject b in Bullies)
        {
            BulliesAnims.Add(b.GetComponent<Animator>());
        }
        
        foreach(GameObject b in Helpers)
        {
            HelpersAnims.Add(b.GetComponent<Animator>());
        }

        BulliedAnim = Bullied.GetComponent<Animator>();

        Buttons = GameObject.Find("Buttons").GetComponentsInChildren<Button>();
        Button_TryAgain.gameObject.SetActive(false);
        canvas.enabled = false;
        scoreCanvas.enabled = false;

#if UNITY_EDITOR
       StartSequence();
#endif

    }



    // --------------------- Character Control ---------------------
    void StartSequence()
    {
        StartCoroutine("BullyWhisper");

        Bullied.gameObject.transform.LookAt(Helpers[0].transform);

        BulliedAnim.SetBool("Walk", true);

        HelpersAnims[0].SetBool("TalkFront", true);
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log("TimesTried: " + PlayerPrefs.GetInt("TimesTried"));

        foreach (GameObject a in Bullies)
        {
            a.transform.LookAt(Bullied.transform);
        }

        if(RemoteResponse&&LocalResponse&&!IsGameEnded)
        {
            LocalResponseAction.Invoke();
            RemoteResponseAction.Invoke();
            IsGameEnded= true;  
            EndGame();
        }
    }

    private void AnimRemoteCharacter(ActMessages response)
    {
        switch (response)
        {
            case ActMessages.LAUGH:
                RemoteResponseAction = () =>
                {
                    AnimLaugh(OtherCharacter);
                };
                break;
            case ActMessages.HUG:
                RemoteResponseAction = () =>
                {
                    AnimHugTheVictim(OtherCharacter);
                };
                Win_RemoteResponse = true;
                break;
            case ActMessages.IGNORE:
                RemoteResponseAction = () =>
                {
                    AnimIgnore(OtherCharacter);
                };
                break;
            case ActMessages.STOP:
                RemoteResponseAction = () =>
                {
                    AnimAskThemToStop(OtherCharacter);
                };
                Win_RemoteResponse = true;
                break;
        }
        RemoteResponse = true;
    }

    private void EndGame()
    {
        int TimesTried = PlayerPrefs.GetInt("TimesTried");
        TimesTried++;

        PlayerPrefs.SetInt("TimesTried", TimesTried);
        
        int score = CalcScore();

        if (TimesTried >= 3)
        {            
            canvas.enabled = false;
            scoreCanvas.enabled = true;
            scoreText.text = "Score: " + score + "/" + SCORE_EACH_GAME * 3 * 2;
        }
        else
        {
            StartCoroutine("EndGameWait");
        }
    }

    private int CalcScore()
    {
        int score= PlayerPrefs.GetInt("Score");
        if (Win_LocalResponse)
        {
            score += SCORE_EACH_GAME;
        }
        if (Win_RemoteResponse)
        {
            score += SCORE_EACH_GAME;
        }
        PlayerPrefs.SetInt("Score", score);
        return score;
    }

    IEnumerator EndGameWait()
    {
        yield return new WaitForSecondsRealtime(5);
        ShowTryAgain();
    }

    private void ShowTryAgain()
    {
        Button_TryAgain.gameObject.SetActive(true);
    }

    public void TryAgain()
    {
        orkestra.Dispatch(Channel.Application, new TryAgainMessage(orkestra.agentId, true));
        Restart();
    }

    private void Restart()
    {
        orkestra.Events.Add(() =>{                                      
            Scene activeScene = SceneManager.GetActiveScene();
            string nameScene = activeScene.name;
            SceneManager.LoadScene("RestartScene",LoadSceneMode.Single);
            arSession.Reset();        
        });
    }

    IEnumerator BullyWhisper()
    {        
        BulliesAnims[1].SetTrigger("Whisper");

        yield return new WaitForSecondsRealtime(3.3f);

        BulliesAnims[0].SetBool("FrontStand", true);

        BulliesAnims[1].SetBool("FrontStand", true);

        BulliesAnims[2].SetBool("FrontStand", true);
                
        BulliedAnim.SetBool("Walk", false);
        
        BulliedAnim.SetBool("HeadBowed", true);

        yield return new WaitForSecondsRealtime(1);

        HelpersAnims[0].SetBool("TalkFront", false);

        foreach (GameObject a in Helpers)
        {
            a.transform.LookAt(Bullied.transform);
        }

        canvas.enabled = true;        
    }

    public void HelperLaugh()
    {
        LocalResponseAction = () =>
        {
            AnimLaugh(SelectedCharacter);
        };
        orkestra.Dispatch(Channel.Application, new ActMessage(orkestra.agentId, ActMessages.LAUGH));
        LocalResponse = true;
        HideButtons();
    }

    private void AnimLaugh(int character)
    {
        HelpersAnims[character].transform.LookAt(Bullied.transform);
        HelpersAnims[character].SetBool("FrontStand", true);        
    }

    public void HelperIgnore()
    {
        LocalResponseAction = () =>
        {
            AnimIgnore(SelectedCharacter);
        };
        orkestra.Dispatch(Channel.Application, new ActMessage(orkestra.agentId, ActMessages.IGNORE));
        HideButtons();
        LocalResponse = true;
    }

    private void AnimIgnore(int character)
    {
        HelpersAnims[character].transform.Rotate(0, 180, 0);
        HelpersAnims[character].SetBool("Walk", true);
    }

    public void HelperAskThemToStop()
    {
        LocalResponseAction = () =>
        {
            AnimAskThemToStop(SelectedCharacter);
        };
        orkestra.Dispatch(Channel.Application, new ActMessage(orkestra.agentId, ActMessages.STOP));
        HideButtons();
        LocalResponse = true;
        Win_LocalResponse = true;
    }

    private void AnimAskThemToStop(int character)
    {
        HelpersAnims[character].transform.LookAt(Bullies[0].transform);
        HelpersAnims[character].SetBool("Walk", true);
        StartCoroutine(GoToBullies(character));
    }

    IEnumerator GoToBullies(int character)
    {
        yield return new WaitForSecondsRealtime(2f);
        HelpersAnims[character].SetBool("Walk", false);
        HelpersAnims[character].transform.LookAt(Bullies[1].transform);
        HelpersAnims[character].SetBool("No", true);
    }

    public void HelperHugTheVictim()
    {
        LocalResponseAction = () =>
        {
            AnimHugTheVictim(SelectedCharacter);
        };
        orkestra.Dispatch(Channel.Application, new ActMessage(orkestra.agentId, ActMessages.HUG));
        HideButtons();
        LocalResponse = true;
        Win_LocalResponse = true;
    }
   
    private void AnimHugTheVictim(int character)
    {
        HelpersAnims[character].SetBool("Walk", true);
        StartCoroutine(HughBullied(character));
    }

    IEnumerator HughBullied(int character)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        HelpersAnims[character].transform.LookAt(Bullied.transform);
        Bullied.transform.LookAt(HelpersAnims[character].transform);
        HelpersAnims[character].SetBool("Walk", false);
        HelpersAnims[character].SetBool("Apoggia", true);
    }

    private void HideButtons()
    {
        foreach(Button b in Buttons)
        {
            b.interactable = false;
        }
    }

    /**
     * --------------------- AR IMAGE TRACKER ---------------------
    */

    /// <summary>
    /// Subscribe to the Ar tracked image change method
    /// </summary>
    private void OnEnable()
    {
        arTrackedImageManager.trackedImagesChanged += ImageChanged;
    }

    /// <summary>
    /// Unsubscribe to the tracked image change method and disconnect from orkestra
    /// </summary>
    private void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged -= ImageChanged;
       
    }

    /// <summary>
    /// When the tracked Image is detected/updated the update image method is called
    /// The game object active value is set to false when the tracked image isnt being the detected by the device
    /// </summary>
    /// <param name="eventArgs">Events of the AR Tracked image manager</param>
    void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
            RendererState(true);
            StartSequence();
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RendererState(false);
        }
    }

    /// <summary>
    /// Update the rotation of the game object if the marker changes it's rotation
    /// </summary>
    /// <param name="trackedImage">marker tracked by the AR App</param>
    private void UpdateImage(ARTrackedImage trackedImage)
    {        
        Characters.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
    }

    /// <summary>
    /// Enable or disable renders components from the characters
    /// </summary>
    /// <param name="active"></param>
    void RendererState(bool active)
    {
        foreach (SkinnedMeshRenderer a in renders)
        {
            a.enabled = active;
        }
    }  
}

public enum ActMessages
{
   IGNORE, LAUGH, STOP, HUG
}