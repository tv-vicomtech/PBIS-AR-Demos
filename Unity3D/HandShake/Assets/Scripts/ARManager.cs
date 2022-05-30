using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARManager : MonoBehaviour
{
    public GameObject trackedImagePrefab;
    ARTrackedImageManager arTrackedImageManager;
    SkinnedMeshRenderer[] renders;

    /// <summary>
    /// Store renders component of the gameobjects
    /// Disable them until the image is tracked
    /// </summary>
    private void Awake()
    {
        renders=trackedImagePrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        RendererState(false);
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
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
        RendererState(true);
        trackedImagePrefab.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
    }


}
