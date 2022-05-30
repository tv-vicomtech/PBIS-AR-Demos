## About / Synopsis
* "Stand up for the other" example developed with Orkestralib-unity 
<p align="center">

</p>
 
## Table of contents
  * [About / Synopsis](#about--synopsis)
  * [Table of contents](#table-of-contents)
  * [Minimum requirements](#minimum-requirements)
  * [Usage](#usage)
  * [Assets / Scenes](#assets--scenes)
  * [Assets / Scripts](#assets--scripts)
  * [Assets / Messages](#assets--messages)

## Minimum requirements
- Unity 2020.3.19f 
  - Android module / IOS module
- Android 7.0 (API level 24)  / IOS 11 

## Usage

Clone the project from the repository: 
```
 git clone  https://username@github.com/tv-vicomtech/ARETE.git
 git checkout origin/feature/standUpForOther
```

Download the latest version of the OrkestraLib and H.Socket.IO package (SocketIOClient package is not necessary for this example) from the OrkestraLib-Unity [releases](https://github.com/tv-vicomtech/orkestralib-unity/releases/).

![Releases](https://user-images.githubusercontent.com/25354672/142617936-c0d2a0c1-6521-4c0d-849e-76a34e038d0a.PNG)

Open with Unity the ARETE/Unity3D/Bullying folder.

Open the OrkestraLib.unitypackage and the H.Socket.IO.unitypackage.

![image](https://user-images.githubusercontent.com/25354672/142626321-3ee12ed1-83ee-404b-b7b4-bfcc0c3af402.png)

Everything is ready!

## Assets / Scenes

There are 2 scenes in the application. The first one is the one used to provide session data, while the other is the AR scene.

### ChooseCharacter
The **Room** field is used to identify the session. All the users that connect using the same value for Room will be sharing the same AR experience.

### GameScene
Scene with AR Foundation.

## Assets / Scripts
### [CharacterMovement.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/standUpForOther/Unity3D/Bullying/Assets/Scripts/CharacterMovement.cs)

Control the characters animation and augmented reality.

The most important steps are the following:

##### Start animation sequence with Orkestra

```csharp
 void StartSequence()
    {
        StartCoroutine("BullyWhisper");

        Bullied.gameObject.transform.LookAt(Helpers[0].transform);

        BulliedAnim.SetBool("Walk", true);

        HelpersAnims[0].SetBool("TalkFront", true);
    }
```

##### Animate the characters and send the info to the other users

```csharp
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

```


##### When the image is tracked the characters are rendered and positioned above the image

```csharp
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
```

### [OrkestraImpl.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/standUpForOther/Unity3D/Bullying/Assets/Scripts/OrkestraImpl.cs)

##### Inits the Orkestra Connection.
```csharp
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
```
##### Subscribe to the Application Channel so the app will receive the events sent by other users.
```csharp
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
```



## Assets / Messages
Store the information that the Application will send through Orkestra.
Messages should implement the OrkestraLib.Message interface.

### [ActMessage.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/standUpForOther/Unity3D/Bullying/Assets/Messages/ActMessage.cs)

```csharp
 [Serializable]
        public class ActMessage : Message
        {
            public ActMessages response;

            public ActMessage(string json) : base(json) { }

            public ActMessage(string sender, ActMessages response) :
              base(typeof(ActMessage).Name, sender)
            {
                this.response = response;
            }

            public ActMessages GetActs()
            {
                return response;
            }

            public override string FriendlyName()
            {
                return typeof(ActMessage).Name;
            }
        }
```


### [TryAgainMessage.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/standUpForOther/Unity3D/Bullying/Assets/Messages/TryAgainMessage.cs)

```csharp
   [Serializable]
        public class TryAgainMessage : Message
        {
            public bool retry;

            public TryAgainMessage(string json) : base(json) { }

            public TryAgainMessage(string sender, bool retry) :
              base(typeof(TryAgainMessage).Name, sender)
            {
                this.retry = retry;
            }

            public bool GetRetry()
            {
                return retry;
            }

            public override string FriendlyName()
            {
                return typeof(TryAgainMessage).Name;
            }
        }
```
