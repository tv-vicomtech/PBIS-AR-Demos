using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detect collisions on the drawer
/// </summary>
public class DetectDrawerCollision : MonoBehaviour
{
    public bool IsSomethingCollidin;
    private int NumberOfObjectsCollidin;
    internal Rigidbody LastObjectOut;
    private const int TotalObjects = 4; // All except the math book

    //internal bool FinalExercise;
    private string[] NamesOfObjectsThatShouldBeInside = { "Math_ExerciseBook", "Ruler",
        "History_Book", "Pencil" };

    private List<string> ObjectsInside = new List<string>();
    internal bool AllObjectsInside { get; private set; }

    public bool tidy;

    void Start()
    {
        NumberOfObjectsCollidin = 5;
        IsSomethingCollidin = true;
        ObjectsInside.AddRange(NamesOfObjectsThatShouldBeInside);
        ObjectsInside.Add("Math_Book");
    }

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

    private bool AvoidCollisions(Collider collision)
    {
        return !(collision.gameObject.name.Equals("Plane") || collision.gameObject.name.Equals("Drawer") || collision.gameObject.name.Equals("DrawerColliderIn"));
    }

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

    private void Update()
    {
        //If last objet using gravity then the movement has stopped
        if (LastObjectOut != null)
        {
            if (LastObjectOut.useGravity)
            {
                IsSomethingCollidin = false;
                LastObjectOut = null;
            }
        }
        else
        {
            AllObjectsInside = false;
            if (NumberOfObjectsCollidin == TotalObjects)
            {
                if (AreAllObjectsInside())
                {
                    Debug.Log("All objects are inside ");
                    AllObjectsInside = true;
                }
            }
        }
    }

    /// <summary>
    /// Detect if all objects are inside the drawer
    /// </summary>
    /// <returns></returns>
    private bool AreAllObjectsInside()
    {
        foreach (string name in NamesOfObjectsThatShouldBeInside)
        {
            if (!ObjectsInside.Contains(name))
            {
                return false;
            }
        }
        return true;
    }
}