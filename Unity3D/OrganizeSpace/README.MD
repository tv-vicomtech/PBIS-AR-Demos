## About / Synopsis
* Keep your space organize example developed with Orkestralib-unity 
<p align="center">
<img src="https://user-images.githubusercontent.com/25354672/166896867-6557f8f2-5550-49d9-a49f-af519cceb35b.gif" alt="organize" align="center">
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



Everything is ready!

## Assets / Scenes

There are 2 scenes in the application. The first one is the one used to provide session data, while the other is the AR scene.


### EnterOrkestra

The **Room** field is used to identify the session. All the users that connect using the same value for Room will be sharing the same AR experience.

The **User** field is used to identify the user.

<img src="https://user-images.githubusercontent.com/25354672/166904590-a19ad9f7-825d-4f10-ac1f-907c6054dc89.jpg" alt="arscene" width="400" height="650">


### UnTidyDrawer
Scene with AR Foundation where the objective is to take out the math book with an untidy drawer.

<img src="https://user-images.githubusercontent.com/25354672/166904604-baef2dcf-3fd3-4395-810a-21b3f022c5ae.jpg" alt="arscene" width="400" height="650">

### TidyDrawer
Scene with AR Foundation where the objective is to take out the math book with a tidy drawer.

<img src="https://user-images.githubusercontent.com/25354672/166906061-bf55faf9-6b4c-4aab-a653-b035b292bc77.png" alt="arscene" width="400" height="650">


## Assets / Scripts
### [OrkestraImpl.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Scripts/OrkestraImpl.cs)

Control the characters animation and establish the Orkestra Connection.

The most important steps are the following:

##### Connection with Orkestra

```csharp
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
```

##### Send information to the other users

```csharp
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
```


### [MainApp.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Scripts/MainApp.cs)

This is the main script with the behaviour of the game.

##### When the user is ready send the info to other user

```csharp
    internal void LocalUserReady()
    {
        DrawerCollisionDetecter = GameObject.Find("DrawerColliderIn").GetComponent<DetectDrawerCollision>();

        isLocalUserReady = true;
        m_OrkestraImpl.SendReadyToOtherUsers();

        if (!isRemoteUserReady)
        {
            Debug.Log("My turn");
            SetMyTurn(true);
        }
        else
        {
            // Everything is ready
            EnableCanvas();
        }

    }
```
##### Change turn to other user
```csharp
    public void NextTurn()
    {
        Debug.Log("Next Turn");
        YourTurn_Text.enabled = false;
        m_SpawnableManager.IsMyTurn = false;
        Button_NextTurn.interactable = false;
        m_OrkestraImpl.SendOtherUserTurn();
    }

```
##### Control objects Movement
```csharp
internal void MoveObjects(ObjCoords oC)
    {
        m_OrkestraImpl.Events.Add(() =>
        {
            GameObject ObjectToMove = GameObject.Find(oC.name);
            Rigidbody rb = ObjectToMove.GetComponent<Rigidbody>();
            //Disable gravity so we can move the object without fallin
            rb.useGravity = false;
            // Change object position
            oC.Update(ObjectToMove.transform);
        });
    }

    internal void StopObjects(ObjCoords oC)
    {
        m_OrkestraImpl.Events.Add(() =>
        {
            GameObject ObjectToMove = GameObject.Find(oC.name);
            if (ObjectToMove == null)
            {
                Debug.LogError("Object not found");
            }
            else
            {
                Rigidbody rb = ObjectToMove.GetComponent<Rigidbody>();
                rb.useGravity = true;
            }
        });
    }
```

### [DetectDrawerCollision.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Scripts/DetectDrawerCollision.cs)
##### Detect the collision with the drawer in order to detect which objects are in and which objects are out
```csharp
  void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collidin with " + collision.gameObject.name);

        if (AvoidCollisions(collision) && NumberOfObjectsCollidin < 5)
        {
            ObjectsInside.Add(collision.gameObject.name);
            NumberOfObjectsCollidin++;
            IsSomethingCollidin = true;
            Debug.Log("Enter Collidin " + NumberOfObjectsCollidin);
        }
    }
```
```csharp
    private bool AvoidCollisions(Collider collision)
    {
        return !(collision.gameObject.name.Equals("Plane") || collision.gameObject.name.Equals("Drawer") ||       collision.gameObject.name.Equals("DrawerColliderIn"));
    }
```
```csharp
    void OnTriggerExit(Collider collision)
    {
        if (AvoidCollisions(collision))
        {
            ObjectsInside.Remove(collision.gameObject.name);
            NumberOfObjectsCollidin--;

            Debug.Log(collision.gameObject.name + " exit the drawer");
            Debug.Log("Number of objects left: " + NumberOfObjectsCollidin);

            if (tidy)
            {
                if (collision.gameObject.name == "Math_Book")
                {
                    NumberOfObjectsCollidin = 0;
                }
            }

            if (NumberOfObjectsCollidin == 0)
            {
                LastObjectOut = collision.gameObject.GetComponent<Rigidbody>();
                Debug.Log("All out!");
            }
        }
    }
```

## Assets / Messages
Store the information that the Application will send through Orkestra.
Messages should implement the OrkestraLib.Message interface.

### [AnswersFinished.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Messages/AnswersFinished.cs)

```csharp
[Serializable]
        public class AnswersFinished : Message
        {

            public bool value;


            public AnswersFinished(string json) : base(json) { }

            public AnswersFinished(string userId, bool value) :
                base(typeof(AnswersFinished).Name, userId)
            {

                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(AnswersFinished).Name;
            }
        }
```


### [ObjCoords.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Messages/ObjCoords.cs)

```csharp
  [Serializable]
        public class ObjCoords : Message
        {
            public float posX, posY, posZ;
            public float scaleX, scaleY, scaleZ;
            public float rotX, rotY, rotZ;
            public string name;

            public bool sharing;

            public ObjCoords(string json) : base(json) { }

            public ObjCoords(GameObject obj) : this("", obj, true) { }

            public ObjCoords(string sender, GameObject obj, bool sharing) :
                base(typeof(ObjCoords).Name, sender)
            {
                this.sharing = sharing;

                UnityEngine.Transform t = obj.transform;

                posX = 0;
                posY = 0;
                posZ = 0;

                name = t.name;

                if (Vector3.Distance(Vector3.zero, t.localPosition) > 0.00001f)
                {
                    posX = t.localPosition.x;
                    posY = t.localPosition.y;
                    posZ = t.localPosition.z;
                }

                scaleX = t.localScale.x;
                scaleY = t.localScale.y;
                scaleZ = t.localScale.z;

                rotX = t.localEulerAngles.x;
                rotY = t.localEulerAngles.y;
                rotZ = t.localEulerAngles.z;
            }

            public void Update(UnityEngine.Transform obj)
            {
                obj.localPosition = new Vector3(posX, posY, posZ);
                obj.localScale = new Vector3(scaleX, scaleY, scaleZ);
                obj.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
            }

            public override string FriendlyName()
            {
                return typeof(ObjCoords).Name;
            }
        }
```

### [ReadyToStart.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Messages/ReadyToStart.cs)

```csharp
  [Serializable]
        public class ReadyToStart : Message
        {

            public bool value;


            public ReadyToStart(string json) : base(json) { }

            public ReadyToStart(string userId, bool value) :
                base(typeof(ReadyToStart).Name, userId)
            {

                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(ReadyToStart).Name;
            }
        }
```
### [YourTurn.cs](https://github.com/tv-vicomtech/ARETE/blob/AnimationDemos/Unity3D/OrganizeSpace/Assets/Messages/YourTurn.cs)

```csharp
 public class YourTurn : Message
        {
            public bool value;

            public YourTurn(string json) : base(json) { }

            public YourTurn(string userId, bool value) :
                base(typeof(YourTurn).Name, userId)
            {
                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(YourTurn).Name;
            }
        }
```
