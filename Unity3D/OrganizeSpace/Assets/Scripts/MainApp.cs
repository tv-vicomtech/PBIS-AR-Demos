using OrkestraLib.Message;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(OrkestraImpl))]
[RequireComponent(typeof(SpawnableManager))]
public class MainApp : MonoBehaviour
{
    private OrkestraImpl m_OrkestraImpl;
    private SpawnableManager m_SpawnableManager;
    public bool isLocalUserReady = false;
    public bool isRemoteUserReady = false;

    private const float MessageDispatchRate = 0.01f;
    public int untidyAnswer;
    public int itemsForceAnswer;

    private Canvas FirstExerciseCanvas;
    private Canvas FirstQuestionCanvas;
    private Canvas Question2Canvas;
    private Canvas FinalExerciseCanvas;
    private Canvas EndCanvas;

    public Button Button_NextTurn;
    public bool isPlaying;
    private DetectDrawerCollision DrawerCollisionDetecter;
    public TMP_Text YourTurn_Text;
    private bool isCanvasQuestionDisplayed;
    private bool LocalQuestionsAnswered = false;
    internal bool FinalExercise;
    internal bool RemoteQuestionsAnswered = false;

    internal List<Image> Buttons_ItemsForce = new List<Image>();
    private int tidyAnswer;

    //internal BoxCollider colliderIn;

    // Start is called before the first frame update
    void Start()
    {
        //Save Orkestra component to send messages through it
        m_OrkestraImpl = GetComponent<OrkestraImpl>();

        //Save spawnable manager component to access 3d objects and game properties
        m_SpawnableManager = GetComponent<SpawnableManager>();

        //Get Text to enable/disable it depending on your turn
        YourTurn_Text = GameObject.Find("YourTurn_Text").GetComponent<TMP_Text>();
        YourTurn_Text.enabled = false;

        //Save the canvas so we can enable/disable them
        // Disable all canvas at the beginning until the drawer is placed 
        var firstExercise = GameObject.Find("FirstExerciseCanvas");
        if (firstExercise != null)
        {
            FirstExerciseCanvas = firstExercise.GetComponent<Canvas>();
            FirstExerciseCanvas.enabled = false;
        }

        var firstQuestion = GameObject.Find("FirstQuestionCanvas");
        if (firstQuestion != null)
        {
            FirstQuestionCanvas = firstQuestion.GetComponent<Canvas>();
            FirstQuestionCanvas.enabled = false;
        }

        var secondQuestion = GameObject.Find("Question2Canvas");
        if (secondQuestion != null)
        {
            Question2Canvas = secondQuestion.GetComponent<Canvas>();
            Question2Canvas.enabled = false;
        }

        var finalExerciseObj = GameObject.Find("FinalExerciseCanvas");
        if (finalExerciseObj)
        {
            FinalExerciseCanvas = finalExerciseObj.GetComponent<Canvas>();
            FinalExerciseCanvas.enabled = false;
        }

        var endCanvasObj = GameObject.Find("EndCanvas");
        if (endCanvasObj)
        {
            EndCanvas = endCanvasObj.GetComponent<Canvas>();
            EndCanvas.enabled = false;
        }

        //Init next turn button with its listener, making it disable until local turn is active
        Button_NextTurn = FirstExerciseCanvas.GetComponentInChildren<Button>();
        Button_NextTurn.onClick.AddListener(() => { NextTurn(); });
        Button_NextTurn.interactable = false;

        FinalExercise = false;
    }



    private void LateUpdate()
    {
        //If drawer is ar placed
        if (DrawerCollisionDetecter != null)
        {
            // If nothing is touching the drawer collider and the first question canvas isn`t displayed yet
            // Deactivate the controls and enable the first question canvas
            // First exercise is finish now the user have to answer the questions
            if (!DrawerCollisionDetecter.IsSomethingCollidin && !isCanvasQuestionDisplayed)
            {
                //When the questions starts you cant move any object
                DeactivateControls();

                FirstExerciseCanvas.enabled = false;
                FirstQuestionCanvas.enabled = true;
                isCanvasQuestionDisplayed = true;
            }
            // The final exercise its finish, the game ends
            // All objects except the math book are inside
            else if (FinalExercise && DrawerCollisionDetecter.AllObjectsInside)
            {
                DeactivateControls();
                FinalExerciseCanvas.enabled = false;
                EndCanvas.enabled = true;
            }
        }
    }

    // Reset the variables so we reuse them for the next exercise
    private void DeactivateControls()
    {
        isRemoteUserReady = false;
        isLocalUserReady = false;
        SetMyTurn(false);
    }


    /**
     *  When the local user is ready send the info to other users and if both users are ready enable exercise canvas.
     *  The first user to be ready starts with the first turn
     */
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

    // When the remote user is ready we wait until the local user is ready
    internal void RemoteUserReady()
    {
        isRemoteUserReady = true;
        if (!isLocalUserReady)
        {
            Debug.Log("Not my turn");
            SetMyTurn(false);
        }
        else
        {
            // Everything is ready
            EnableCanvas();
        }
    }

    // Enable the first or last canvas dependin on what game stage the user is
    void EnableCanvas()
    {
        if (!FinalExercise)
        {
            EnableFirstExerciseCanvas();
        }
        else
        {
            EnableFinalExerciseCanvas();
        }
    }

    // If both users are ready the exercise can start
    void EnableFirstExerciseCanvas()
    {
        if (isLocalUserReady && isRemoteUserReady)
        {
            Debug.Log("Canvas enabled");
            FirstExerciseCanvas.enabled = true;
        }
    }

    // Disable previous canvas and starts the final exercise
    void EnableFinalExerciseCanvas()
    {
        if (isLocalUserReady && isRemoteUserReady)
        {
            TMP_Text[] textes = FinalExerciseCanvas.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text t in textes)
            {
                if (t.gameObject.name.Equals("YourTurn_Text"))
                    YourTurn_Text = t;
            }

            YourTurn_Text.enabled = false;

            Debug.Log("Canvas enabled");

            Question2Canvas.enabled = false;
            FinalExerciseCanvas.enabled = true;
        }
    }


    internal void SetMyTurn(bool v)
    {
        m_SpawnableManager.IsMyTurn = v;
    }


    public void NextTurn()
    {
        Debug.Log("Next Turn");
        YourTurn_Text.enabled = false;
        m_SpawnableManager.IsMyTurn = false;
        Button_NextTurn.interactable = false;
        m_OrkestraImpl.SendOtherUserTurn();
    }

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

    public void Answer_TheDrawerWasUntidy(int answer)
    {
        untidyAnswer = answer;
        FirstQuestionCanvas.enabled = false;
        Question2Canvas.enabled = true;
        SaveButtonsOfItemsForceToTakeOut();
    }

    private void SaveButtonsOfItemsForceToTakeOut()
    {
        //Save the buttons to change the color
        Buttons_ItemsForce.Add(GameObject.Find("Button_Noneofthem").GetComponent<Image>());
        Buttons_ItemsForce.Add(GameObject.Find("Button_Pencil").GetComponent<Image>());
        Buttons_ItemsForce.Add(GameObject.Find("Button_MathsNotebook").GetComponent<Image>());
        Buttons_ItemsForce.Add(GameObject.Find("Button_HistoryBook").GetComponent<Image>());
        Buttons_ItemsForce.Add(GameObject.Find("Button_MathsBook").GetComponent<Image>());
    }

    public void Answer_TheDrawerWastidy(int answer)
    {
        tidyAnswer = answer;
        FirstQuestionCanvas.enabled = false;
        EndCanvas.enabled = true;
    }

    // Change colors of options of items force to take out
    public void Answer_ItemsForceToTakeOut(int answer)
    {
        itemsForceAnswer = answer;

        if (answer == (int)ItemsForceToTakeOut.NONEOFTHEM)
        {
            for (int i = 0; i < Buttons_ItemsForce.Count; i++)
            {
                if (i != answer)
                    Buttons_ItemsForce[i].color = Color.white;
            }
        }
        else
            Buttons_ItemsForce[(int)ItemsForceToTakeOut.NONEOFTHEM].color = Color.white;

        Color color = Buttons_ItemsForce[answer].color;

        if (color == Color.green)
            color = Color.white;
        else
            color = Color.green;

        Buttons_ItemsForce[answer].color = color;

    }

    // Finish the questions and start the last exercise
    public void GoToPutElementsAgainInDrawer()
    {
        foreach (Button b in Question2Canvas.GetComponentsInChildren<Button>())
        {
            b.interactable = false;
        }

        Question2Canvas.GetComponentInChildren<TMP_Text>().text = "Wait...";

        LocalQuestionsAnswered = true;

        FinalExercise = true;
        m_OrkestraImpl.SendAllAnswersFinished();
        LocalUserReady();
    }

    /// <summary>
    /// Dispatchs camera messages at constant rate (fps): <code>60 / <see cref="MessageDispatchRate"/></code>
    /// </summary>
    internal IEnumerator UpdateRemoteObject()
    {
        while (m_SpawnableManager.selectedObject != null)
        {
            Debug.Log("Sending: " + m_SpawnableManager.selectedObject.name);

            m_OrkestraImpl.SendObjectCoords(m_SpawnableManager.selectedObject);

            yield return new WaitForSeconds(MessageDispatchRate);
        }
        m_OrkestraImpl.StopObjectCoords(m_SpawnableManager.lastSharedObject);
    }
}

public enum AnswersTidyDrawer
{
    MATERIAL_I_DIDNT_NEED = 0, SPENT_LITTLE_TIME = 1, SPENT_MORE_TIME = 2, I_TOOK_OUT_ONLY_WHAT_I_NEEDED = 3
}

public enum ItemsForceToTakeOut
{
    NONEOFTHEM = 0, PENCIL = 1, MATHS_NOTEBOOK = 2, HISTORY_BOOK = 3, MATHS_BOOK = 4
}