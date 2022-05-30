using OrkestraLib;
using OrkestraLib.Exceptions;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : Orkestra
{
    public string selectedCharacter;
    public string remoteSelectedCharacter;
    
    private GameObject character;
    private GameObject remoteCharacter;

    //private Dictionary<string, Quaternion> rotations = new Dictionary<string, Quaternion>();
     
    private Animator animator;
    private Animator remoteAnimator;

    public Canvas connectingCanvas;

    internal string answerReceive;
    internal string answerEmitted;

    private string orkestraSesion;
    
    UIManager uim;

    private Queue<Action> animEvents=new Queue<Action>();

    private bool isAnim=false;
    private bool allAnswered = false;

    /// <summary>
    /// Store gameObjects and animators component from the characters
    /// Init Orkestra connection
    /// </summary>
    private void Start()
    {
        uim = GameObject.Find("UIDocument").GetComponent<UIManager>();

        selectedCharacter = PlayerPrefs.GetString("Character");
        
        if(selectedCharacter.Equals(CharacterTypes.Boy))
            remoteSelectedCharacter = CharacterTypes.Woman;        
        else
            remoteSelectedCharacter = CharacterTypes.Boy;

        remoteCharacter = GameObject.Find(remoteSelectedCharacter);
        remoteAnimator = remoteCharacter.GetComponent<Animator>();


        character = GameObject.Find(selectedCharacter);
        animator = character.GetComponent<Animator>();


        orkestraSesion = PlayerPrefs.GetString("OrkestraSesion");
        ConnectOrkestra();        
    }


    /// <summary>
    /// Reset the answers
    /// </summary>
    internal void ResetSession()
    {   
        this.answerEmitted = null;
        this.answerReceive = null;
        this.allAnswered = false;
    }
    /// <summary>
    /// Send the reset message through Orkestra
    /// </summary>
    internal void SendReset()
    {
        Dispatch(Channel.Application, new TryAgainMessage(agentId, true));
    }
    internal void SendHandshake(string handShake)
    {
        answerEmitted = handShake;
        Dispatch(Channel.Application, new HandShakeMessage(agentId, handShake));
    }

    /// <summary>
    /// Establish the Orkestra Connection
    /// </summary>
    private void ConnectOrkestra()
    {
        //Subscribe to Application Events so we receive the events from the Application Channel
        ApplicationEvents += AppEventSubscriber;

        //Room name
        this.room = orkestraSesion;
        //Agent id
        this.agentId = selectedCharacter;
        //Orkestra server URL
        this.url = "https://cloud.flexcontrol.net";

        // true to erease the events of the room
        ResetRoomAtDisconnect = true;

        // Register the particular messages of the application 
        RegisterEvents(new Type[]{
                typeof(HandShakeMessage),               
                typeof(TryAgainMessage)               
        });

        // Use Orkestra SDK with HSocketIOClient
        OrkestraWithHSIO.Install(this, (graceful, message) =>
        {
            Events.Add(() =>
            {
                if (!graceful) { Debug.Log(message); connectingCanvas.enabled = false; }
                else
                {
                    Debug.LogError(message);
                }
            });
        });

        try
        {
            // Start Orkestra
            Connect(() =>
            {
                Debug.Log("All stuff is ready");
            });

        }
        catch (ServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void Update()
    {
        if (answerEmitted != null && answerReceive != null && !allAnswered)
        {
            allAnswered = true;

            Events.Add(() => 
            {
                Animate(answerReceive, true); 
                Animate(answerEmitted, false);
                uim.Label.text = Text.EMPTY;
            });

            animEvents.Enqueue(() =>
            {
                GameEnds();
            });
        }


        bool localAnim = animator.GetCurrentAnimatorStateInfo(0).IsName(answerEmitted);
        bool remoteAnim = remoteAnimator.GetCurrentAnimatorStateInfo(0).IsName(answerReceive);
        bool anims = localAnim || remoteAnim;

        if (anims)
        {
            isAnim = true;
        }
        else if (isAnim)
        {
            isAnim = false;
            if (animEvents.Count > 0)
            {
                animEvents.Dequeue().Invoke();
                Debug.Log("Invoke");
            }
        }

    }

    /// <summary>
    /// Trigger the character's animation. Also establish when the game should restart
    /// </summary>
    /// <param name="HandShake">Type of Hand shake</param>
    /// <param name="remote">if its true, its receive from the other devices. 
    /// If its false the animation is trigger by the local player </param>
    public void Animate(string HandShake, bool remote)
    {
        if (remote)
        {
            remoteAnimator.SetTrigger(HandShake);            
        }
        else
        {            
            animator.SetTrigger(HandShake);                       
        }
       
    }
 

    /// <summary>
    /// Show the Result
    /// Show the try again button
    /// </summary>
    private void GameEnds()
    {
        uim.DisableButtons();
        if (answerReceive != null && answerEmitted != null)
        {
            if (answerReceive.Equals(HandShakesTypes.STRETTAMANO) && answerEmitted.Equals(HandShakesTypes.STRETTAMANO))
            {
                uim.Label.text = Text.BOTHANSWERSCORRECT;
                return;
            }
            else if (answerReceive.Equals(HandShakesTypes.STRETTAMANO))
            {
                if (selectedCharacter.Equals(CharacterTypes.Woman))
                {
                    uim.Label.text = Text.STUDENTANSWERCORRECT;
                }
                else
                {
                    uim.Label.text = Text.TEACHERANSWERCORRECT;
                }
            }
            else if (answerEmitted.Equals(HandShakesTypes.STRETTAMANO))
            {
                if (selectedCharacter.Equals(CharacterTypes.Woman))
                {
                    uim.Label.text = Text.TEACHERANSWERCORRECT;
                }
                else
                {
                    uim.Label.text = Text.STUDENTANSWERCORRECT;
                }
            }
            else
            {
                uim.Label.text = Text.BOTHANSWERSINCORRECT;
            }
        }
        uim.ShowTryAgain();
        
    }


    /// <summary>
    /// Subscriber to the orkestra application channel
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="evt">Event receive</param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        //Check the type of the message receive
        if (evt.IsEvent(typeof(HandShakeMessage)))
        {
            HandShakeMessage handShakeMessage = new HandShakeMessage(evt.value);
            
            if (!handShakeMessage.sender.Equals(agentId))
            {
                Debug.Log("Answer receive: " + handShakeMessage.handShake);
                answerReceive = handShakeMessage.handShake;

            }
        }
        else if (evt.IsEvent(typeof(TryAgainMessage)))
        {
            TryAgainMessage tryAgainMessage = new TryAgainMessage(evt.value);

            if (!tryAgainMessage.sender.Equals(agentId))
            {
                if(tryAgainMessage.retry)
                {
                    ResetSession();
                    uim.StartUI();
                }
            }
        }

    }
}

public static class CharacterTypes
{
    public const string Boy = "boy_B";
    public const string Woman= "woman_C";
}

public static class HandShakesTypes
{
    public const string HIGHFIVE = "HighFive";
    public const string PUGNETTO = "Pugnetto";
    public const string SALUTO = "Saluto";
    public const string STRETTAMANO = "StrettaMano";
}