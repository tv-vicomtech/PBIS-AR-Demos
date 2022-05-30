using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/**
 * Manage 3d Objects and their interactions
 **/
public class SpawnableManager : MonoBehaviour
{
    [SerializeField]
    ARRaycastManager m_RaycastManager;

    private MainApp m_MainApp;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    Camera arCam;
    internal GameObject lastSharedObject;
    internal GameObject selectedObject;
    Rigidbody selectedRigidBody;
    public bool IsMyTurn = true;
    private float dist;
    private Vector3 v3;

    // Start is called before the first frame update
    void Start()
    {
        selectedObject = null;
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_MainApp = GetComponent<MainApp>();
    }

    // Update is called once per frame
    void Update()
    {
        //When its the user turn and both users are ready the user can start moving the objects
        if (IsMyTurn && m_MainApp.isLocalUserReady && m_MainApp.isRemoteUserReady)
        {
            m_MainApp.YourTurn_Text.enabled = true;

            m_MainApp.Button_NextTurn.interactable = true;

            if (Input.touchCount == 0)
                return;

            Touch touchZero = Input.GetTouch(0);

            Ray ray;
            ray = arCam.ScreenPointToRay(touchZero.position);


            if (m_RaycastManager.Raycast(touchZero.position, m_Hits))
            {
                if (touchZero.phase == TouchPhase.Began && selectedObject == null)
                {
                    InitTouchValues(ray, touchZero.position);
                }
                else if (touchZero.phase == TouchPhase.Moved && selectedObject != null)
                {
                    MoveObject();
                }
                if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled)
                {
                    StopObjectControl();
                }
            }
        }
    }

    private void InitTouchValues(Ray ray, Vector3 pos)
    {
        RaycastHit hit;

        //ignore those layers
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~Physics.IgnoreRaycastLayer))
        {
            if (hit.collider.gameObject)
            {
                // Detect the object hit by collider that will be moved or scale by the user
                selectedObject = hit.collider.gameObject;

                //Get the rigidbody to modify it later
                selectedRigidBody = hit.collider.gameObject.GetComponent<Rigidbody>();

                //Send information to other users
                StartCoroutine(m_MainApp.UpdateRemoteObject());

                ////Distance between the camera and the object trying to move
                //dist = hit.transform.position.z - Camera.main.transform.position.z;
                //v3 = new Vector3(pos.x, pos.y, dist);
                //v3 = Camera.main.ScreenToWorldPoint(v3);
            }
        }
    }

    private void MoveObject()
    {
        // Remove gravity so the object won`t fall when its moving
        selectedRigidBody.useGravity = false;
        int i = 0;

        Vector3 v = new Vector3(m_Hits[i].pose.position.x, m_Hits[i].pose.position.y + 0.08f, m_Hits[i].pose.position.z);
        selectedObject.transform.position = v;
    }

    private void StopObjectControl()
    {
        if (selectedRigidBody != null)
        {
            selectedRigidBody.useGravity = true;
            selectedRigidBody = null;
        }

        lastSharedObject = selectedObject;
        selectedObject = null;
    }

}
