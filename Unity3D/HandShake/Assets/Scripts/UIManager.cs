using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public Button HandShake { get; private set; }
    public Button Hello { get; private set; }
    public Button HelloPunch { get; private set; }
    public Button HighFive { get; private set; }
    public Button Reset { get; private set; }
    public Label Label { get; private set; }

    public VisualElement topCanvas;

    List<Button> buttons=new List<Button>();
    CharacterControl cc;

    // Start is called before the first frame update
    /// <summary>
    /// Store the ui elements in objects, give them an event listener
    /// </summary>
    void Start()
    {
        
        cc = GameObject.Find("Characters").GetComponent<CharacterControl>();

        var uiDocument = GetComponent<UIDocument>().rootVisualElement;
        topCanvas= uiDocument.Q<VisualElement>("topCanvas");

        uiDocument.Q<Button>("Back").clicked += delegate { SceneManager.LoadScene("ChooseCharacter"); };

        HandShake = uiDocument.Q<Button>("HandShake");
        HandShake.clicked += delegate{ cc.SendHandshake(HandShakesTypes.STRETTAMANO); DisableButtons();};

        Reset = uiDocument.Q<Button>("Reset");
        Reset.clicked+= delegate {cc.ResetSession(); StartUI(); cc.SendReset(); };

        Hello = uiDocument.Q<Button>("Hello");
        Hello.clicked += delegate{  cc.SendHandshake(HandShakesTypes.SALUTO); DisableButtons(); };

        HelloPunch = uiDocument.Q<Button>("HelloPunch");
        HelloPunch.clicked += delegate { cc.SendHandshake(HandShakesTypes.PUGNETTO); DisableButtons();};

        HighFive = uiDocument.Q<Button>("HighFive");
        HighFive.clicked += delegate { cc.SendHandshake(HandShakesTypes.HIGHFIVE); DisableButtons(); };

        Label = uiDocument.Q<Label>("QuestionLabel");

        AddButtons(HandShake, Hello, HelloPunch, HighFive);

        StartUI();

    }

    /// <summary>
    /// Start state of the UI Text 
    /// </summary>
    public void StartUI()
    {
        topCanvas.Remove(Reset);

        if (cc.selectedCharacter.Equals(CharacterTypes.Boy))
        {
            Label.text = Text.RESPONDTEACHER;
            EnableButtons();
        }
        else if (cc.selectedCharacter.Equals(CharacterTypes.Woman))
        {
            Label.text = Text.RESPONDSTUDENT;
            EnableButtons();
        }
    }
    /// <summary>
    /// Show try again button
    /// </summary>
    public void ShowTryAgain()
    {
        topCanvas.Add(Reset);
    }

    /// <summary>
    /// Disable all buttons
    /// </summary>
    public void DisableButtons()
    {
        this.buttons.ForEach((button) => { button.SetEnabled(false); });
    }
    /// <summary>
    /// Enable all buttons
    /// </summary>
    public void EnableButtons()
    {
        this.buttons.ForEach((button) => { button.SetEnabled(true); });
    }

    /// <summary>
    /// Store all the buttons in a list
    /// </summary>
    /// <param name="buttons"></param>
    void AddButtons(params Button[] buttons)
    {
        this.buttons.AddRange(buttons);
    }

}
public static class Text
{
    public const string RESPONDTEACHER = "How do you respond to the Teacher?";
    public const string RESPONDSTUDENT = "How do you respond to the Student?";
    public const string BOTHANSWERSCORRECT = "Both Answers were correct! Congratulations!";
    public const string STUDENTANSWERCORRECT= "Only the student answer was correct...";
    public const string TEACHERANSWERCORRECT = "Only the teacher answer was correct...";
    public const string BOTHANSWERSINCORRECT = "Both answers were incorrect :'( ... ";
    public const string EMPTY = "";

}


