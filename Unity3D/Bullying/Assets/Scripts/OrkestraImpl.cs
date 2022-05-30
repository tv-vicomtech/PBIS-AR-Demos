using OrkestraLib;
using OrkestraLib.Exceptions;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrkestraImpl : Orkestra
{

    public Action<ActMessages> AnimRemoteCharacterFunc { get; internal set; }
    public Action RestartFunc { get; internal set; }

    public static OrkestraImpl orkestraInstance;
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (orkestraInstance == null)
        {
            orkestraInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {        
        if (PlayerPrefs.GetInt("TimesTried") == 0)
            ConnectOrkestra();
        Debug.Log(PlayerPrefs.GetInt("TimesTried"));
        DontDestroyOnLoad(this);
    }

    /**
      * --------------------- Orkestra ---------------------
      */

    /// <summary>
    /// Establish the Orkestra Connection
    /// </summary>
    private void ConnectOrkestra()
    {
        //Subscribe to Application Events so we receive the events from the Application Channel
        ApplicationEvents += AppEventSubscriber;

        //Room name
        this.room = PlayerPrefs.GetString("room");
        //Agent id
        this.agentId = PlayerPrefs.GetString("SelectedCharacter").ToString();
        //Orkestra server URL
        this.url = "https://cloud.flexcontrol.net";

        // true to erease the events of the room
        ResetRoomAtDisconnect = true;

        // Register the particular messages of the application 
        RegisterEvents(new Type[]{
            typeof(ActMessage),
            typeof(TryAgainMessage)
        });

        // Use Orkestra SDK with HSocketIOClient
        OrkestraWithHSIO.Install(this, (graceful, message) =>
        {
            Events.Add(() =>
            {
                if (!graceful) { Debug.Log(message); }
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


    /// <summary>
    /// Subscriber to the orkestra application channel
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="evt">Event receive</param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        //Check the type of the message receive
        if (evt.IsEvent(typeof(ActMessage)))
        {
            ActMessage actMessage = new ActMessage(evt.value);

            if (!actMessage.sender.Equals(agentId))
            {
                Debug.Log("Answer receive: " + actMessage.response);
                Events.Add(() =>
                {
                    AnimRemoteCharacterFunc(actMessage.response);
                });
            }
        }
        else if (evt.IsEvent(typeof(TryAgainMessage)))
        {
            TryAgainMessage tryAgainMessage = new TryAgainMessage(evt.value);
            if (!tryAgainMessage.sender.Equals(agentId))
            {
                RestartFunc();
            }
        }
    }

  
}
