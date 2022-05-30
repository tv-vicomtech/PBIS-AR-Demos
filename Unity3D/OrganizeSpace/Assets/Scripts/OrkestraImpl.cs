using OrkestraLib;
using OrkestraLib.Exceptions;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using System;
using UnityEngine;

public class OrkestraImpl : Orkestra
{
    public static OrkestraImpl orkestraInstance;
    private MainApp m_MainApp;

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
        m_MainApp = GetComponent<MainApp>();

        ConnectOrkestra();
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
        this.agentId = PlayerPrefs.GetString("agentID");
        //Orkestra server URL
        this.url = "https://cloud.flexcontrol.net";

        // true to erease the events of the room
        ResetRoomAtDisconnect = true;

        // Register the particular messages of the application 
        RegisterEvents(new Type[]{
            typeof(ReadyToStart),
            typeof(YourTurn),
            typeof(ObjCoords),
            typeof(AnswersFinished),
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

    internal void SendReadyToOtherUsers()
    {
        ReadyToStart rTS = new ReadyToStart(agentId, true);
        Dispatch(Channel.Application, rTS);
    }

    internal void SendOtherUserTurn()
    {
        YourTurn yourTurn = new YourTurn(agentId, true);
        Dispatch(Channel.Application, yourTurn);
    }

    internal void SendObjectCoords(GameObject objectToShare)
    {
        Dispatch(Channel.Application, new ObjCoords(agentId, objectToShare, true));
    }

    internal void StopObjectCoords(GameObject objectToShare)
    {
        Dispatch(Channel.Application, new ObjCoords(agentId, objectToShare, false));
    }

    internal void SendAllAnswersFinished()
    {
        Dispatch(Channel.Application, new AnswersFinished(agentId, true));
    }

    /// <summary>
    /// Subscriber to the orkestra application channel
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="evt">Event receive</param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        //Check the type of the message receive
        if (evt.IsEvent(typeof(ReadyToStart)))
        {
            ReadyToStart readyToStart = new ReadyToStart(evt.value);

            if (!readyToStart.sender.Equals(agentId))
            {
                m_MainApp.RemoteUserReady();
            }
        }
        else if (evt.IsEvent(typeof(YourTurn)))
        {
            YourTurn yT = new YourTurn(evt.value);

            if (!yT.sender.Equals(agentId))
            {
                m_MainApp.SetMyTurn(yT.value);
            }
        }
        else if (evt.IsEvent(typeof(ObjCoords)))
        {
            ObjCoords oC = new ObjCoords(evt.value);

            if (!oC.sender.Equals(agentId))
            {
                if (oC.sharing)
                    m_MainApp.MoveObjects(oC);
                else
                    m_MainApp.StopObjects(oC);
            }
        }
        else if (evt.IsEvent(typeof(AnswersFinished)))
        {
            AnswersFinished aF = new AnswersFinished(evt.value);

            if (!aF.sender.Equals(agentId))
            {
                m_MainApp.RemoteQuestionsAnswered = true;
            }


        }
    }


}