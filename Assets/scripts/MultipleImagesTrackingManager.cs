using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class MultipleImagesTrackingManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();

    private ARTrackedImageManager _trackedImageManager;
    private Dictionary<string, GameObject> _arObjects = new Dictionary<string, GameObject>();

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        _trackedImageManager.trackablesChanged.AddListener(OnImagesTrackedChanged);
        SetupSceneElements();
    }

    private void OnDisable()
    {
        _trackedImageManager.trackablesChanged.RemoveListener(OnImagesTrackedChanged);
    }

    private void SetupSceneElements()
    {
        foreach (var prefab in prefabsToSpawn)
        {
            if (prefab == null) continue;

            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            arObject.name = prefab.name;
            arObject.SetActive(false);
            _arObjects[arObject.name] = arObject;
        }
    }

    private void OnImagesTrackedChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            string imageName = trackedImage.Value.referenceImage.name; // ✅ trackedImage.Value 是 ARTrackedImage
            if (_arObjects.TryGetValue(imageName, out var obj))
            {
                obj.SetActive(false);
            }
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null || trackedImage.referenceImage == null) return;

        string imageName = trackedImage.referenceImage.name;
        if (!_arObjects.TryGetValue(imageName, out var arObject)) return;

        if (trackedImage.trackingState == TrackingState.None || trackedImage.trackingState == TrackingState.Limited)
        {
            arObject.SetActive(false);
            return;
        }

        arObject.SetActive(true);
        arObject.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
    }
}
