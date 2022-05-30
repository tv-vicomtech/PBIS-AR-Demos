using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(MainApp))]
public class ARTapToPlace : MonoBehaviour
{
    public Vector3 touchedPosition;
    public GameObject objectToPlace;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    ARRaycastManager m_arRayCastManager;
    ARPlaneManager m_planeManager;
    private MainApp m_MainApp;
    private GameObject spawnedObject;

    void Awake()
    {
        m_arRayCastManager = GetComponent<ARRaycastManager>();
        m_planeManager = GetComponent<ARPlaneManager>();
        m_MainApp = GetComponent<MainApp>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnedObject != null)
        {
            foreach (var plane in m_planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }

        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (m_arRayCastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                touchedPosition = hitPose.position;
                touchedPosition += new Vector3(0.08f, 0, 0);
                spawnedObject = Instantiate(objectToPlace, touchedPosition, Quaternion.identity);
                m_MainApp.LocalUserReady();
            }
        }
    }
}
