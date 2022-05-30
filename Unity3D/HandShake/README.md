## About / Synopsis
* HandShake example developed with Orkestralib-unity 
<p align="center">

<img src="https://user-images.githubusercontent.com/25354672/153558207-510553ff-600a-4f55-8ba8-1ec79fb17aa0.gif" alt="login" align="center">
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
 git checkout origin/feature/handShake
```

Download the latest version of the OrkestraLib and H.Socket.IO package (SocketIOClient package is not necessary for this example) from the OrkestraLib-Unity [releases](https://github.com/tv-vicomtech/orkestralib-unity/releases/).

![Releases](https://user-images.githubusercontent.com/25354672/142617936-c0d2a0c1-6521-4c0d-849e-76a34e038d0a.PNG)

Open with Unity the ARETE/Unity3D/HandShake folder.

Open the OrkestraLib.unitypackage and the H.Socket.IO.unitypackage.

![image](https://user-images.githubusercontent.com/25354672/142626321-3ee12ed1-83ee-404b-b7b4-bfcc0c3af402.png)

Everything is ready!

## Assets / Scenes

There are 2 scenes in the application. The first one is the one used to provide session data, while the other is the AR scene.

### ChooseCharacter
The **Room** field is used to identify the session. All the users that connect using the same value for Room will be sharing the same AR experience.

<img src="https://user-images.githubusercontent.com/25354672/153192552-217b4ace-56f4-44b1-bc79-067533cf5b80.jpg" alt="login" width="400" height="650">


### GameScene
Scene with AR Foundation.

<img src="https://user-images.githubusercontent.com/25354672/153192677-75d0af2f-307d-40d5-b9e5-c1753fd5db87.jpg" alt="arscene" width="400" height="650">

## Assets / Scripts
### [CharacterControl.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/handShake/Unity3D/HandShake/Assets/Scripts/CharacterControl.cs)

Control the characters animation and establish the Orkestra Connection.

The most important steps are the following:

##### Connection with Orkestra

```csharp
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
```

##### Animate the characters and send the info to the other users

```csharp
    public void Animate(string HandShake, bool remote)
    {
        if (remote)
        {
            remoteAnimator.SetTrigger(HandShake);
            answerReceive = HandShake;

            animEvents.Enqueue(() =>
            {
                if (selectedCharacter.Equals(CharacterTypes.Woman))
                {
                    uim.Label.text = Text.RESPONDSTUDENT;
                    uim.EnableButtons();
                }
                else
                {
                    GameEnds();
                }
            });
        }
        else
        {            
            animator.SetTrigger(HandShake);
            answerEmitted = HandShake;
            Dispatch(Channel.Application, new HandShakeMessage(agentId, HandShake));
            animEvents.Enqueue(() =>
            {
                uim.Label.text = Text.EMPTY;

                if (selectedCharacter.Equals(CharacterTypes.Woman))
                {
                    GameEnds();
                }
            });
        }
    }
```

##### Send information to a all users

```csharp
 Dispatch(Channel.Application, new HandShakeMessage(agentId, HandShake));
 ```

##### Subscribe to the Application Channel so the app will receive the events sent by other users.

```csharp
 private void ConnectOrkestra()
    {
        //Subscribe to Application Events so we receive the events from the Application Channel
        ApplicationEvents += AppEventSubscriber;
        .
        .
        .
    }

void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        //Check the type of the message receive
        if (evt.IsEvent(typeof(HandShakeMessage)))
        {
            HandShakeMessage handShakeMessage = new HandShakeMessage(evt.value);

            
            if (!handShakeMessage.sender.Equals(agentId))
            {
                Animate(handShakeMessage.handShake,true);
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
```


### [ARManager.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/handShake/Unity3D/HandShake/Assets/Scripts/ARManager.cs)

This is the main script with the behaviour of the Augmented reality scene.

##### When the image is tracked the characters are rendered and positioned above the image

```csharp
private void Awake()
    {
        renders=trackedImagePrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        RendererState(false);
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }
```

```csharp
void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
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
```


## Assets / Messages
Store the information that the Application will send through Orkestra.
Messages should implement the OrkestraLib.Message interface.

### [HandShakeMessage.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/handShake/Unity3D/HandShake/Assets/Messages/HandShakeMessage.cs)

```csharp
[Serializable]
        public class HandShakeMessage : Message
        {
            public string handShake;

            public HandShakeMessage(string json) : base(json) { }

            public HandShakeMessage(string sender, string handShake) :
              base(typeof(HandShakeMessage).Name, sender)
            {
                this.handShake = handShake;
            }

            public string GetHandShake()
            {
                return handShake;
            }

            public override string FriendlyName()
            {
                return typeof(HandShakeMessage).Name;
            }
        }
```


### [TryAgainMessage.cs](https://github.com/tv-vicomtech/ARETE/blob/feature/handShake/Unity3D/HandShake/Assets/Messages/TryAgainMessage.cs)

```csharp
    [Serializable]
        public class TryAgainMessage : Message
        {
            public bool retry;

            public TryAgainMessage(string json) : base(json) { }

            public TryAgainMessage(string sender,bool retry) :
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
                return typeof(HandShakeMessage).Name;
            }
        }
```
