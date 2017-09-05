//-----------------------------------------------------------------------
//Author: Richard Hoagland 
//Portfolio: https://pjchardt.github.io/ 
//
//MIT License
//-----------------------------------------------------------------------

using GoogleARCore;
using GoogleARCore.HelloAR;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for polygardens. Borrows from HelloARController example.
/// </summary>

[System.Serializable]
public class SpawnItem
{
    public int weight;
    public float positionPadding;
    public Vector3 scaleMin, scaleMax;
    public GameObject prefab;
    public Material[] materials;
   
    public GameObject DoSetup(Vector3 pos, Transform t)
    {
        GameObject o = GameObject.Instantiate(prefab, pos, Quaternion.identity, t);
        o.transform.localScale = Vector3.Lerp(scaleMin, scaleMax, Random.Range(0f, 1f));
        if (materials.Length > 0)
            o.GetComponentInChildren<Renderer>().material = materials[Random.Range(0, materials.Length)];
        return o;
    }
}

public class SpawnedItem
{
    public float positionPadding;
    public GameObject obj;
}

public class GardenController : MonoBehaviour
{
    /// <summary>
    /// The first-person camera being used to render the passthrough camera.
    /// </summary>
    public Camera m_firstPersonCamera;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject m_trackedPlanePrefab;

    /// <summary>
    /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    public GameObject m_searchingForPlaneUI;

    private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();
    private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

    public SpawnItem[] spawnPrefabs;
    List<SpawnedItem> spawnedObjects = new List<SpawnedItem>(); //TODO: optimize by placing objects in cells and only checking current cell and neighbors
    bool tapMode;

    //Variables For testing
    float lastSample;
    public Text hitPointText, closestGrassText, grassObjectsCount, randomGrassLocation;

    private void Start()
    {
        lastSample = Time.timeSinceLevelLoad;
        tapMode = false;
    }

    void Update ()
    {
#if UNITY_EDITOR
        SimulateAR();
        return;
#endif

        _QuitOnConnectionErrors();

        // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
        if (Frame.TrackingState != FrameTrackingState.Tracking)
        {
            const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
            Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Frame.GetNewPlanes(ref m_newPlanes);

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        for (int i = 0; i < m_newPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                transform);
            planeObject.GetComponent<TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);
            planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
        }

        // Disable the snackbar UI when no planes are valid.
        bool showSearchingUI = true;
        Frame.GetAllPlanes(ref m_allPlanes);
        for (int i = 0; i < m_allPlanes.Count; i++)
        {
            if (m_allPlanes[i].IsValid)
            {
                showSearchingUI = false;
                break;
            }
        }

        m_searchingForPlaneUI.SetActive(showSearchingUI);

        if (Input.touches.Length > 0)
        {
            TouchPhase t = Input.touches[0].phase; //just look at first touch until we decide how to handle multitouch
           
            if (t == TouchPhase.Ended)
            {
                tapMode = false;
            }

            Ray ray = CameraSingleton.Instance.GetActiveCamera().ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (t == TouchPhase.Began)
                {
                    TouchObject o = hit.collider.gameObject.GetComponent<TouchObject>();
                    if (o)
                    {
                        o.OnTouch();
                        tapMode = true;
                    }
                }
            }

            if (!tapMode && t != TouchPhase.Ended)
            {
                TrackableHit tHit;
                TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;
                Vector3 rotatedForward = Quaternion.Euler(Random.Range(-6f, 6f), Random.Range(-6f, 6f), 0f) * ray.direction;
                Ray rotatedRay = new Ray(ray.origin, rotatedForward);
                bool cancelOut = false;
                // Randomly select an item to spawn from the list
                SpawnItem item = WeightedSelection.RandomItem<SpawnItem>(spawnPrefabs, FindWeight);
                if (Session.Raycast(rotatedRay, raycastFilter, out tHit))
                {
                    for (int i = 0; i < spawnedObjects.Count; i++)
                    {
                        if (spawnedObjects[i] != null)
                        {
                            float d = Vector3.Distance(tHit.Point, spawnedObjects[i].obj.transform.position);
                            float padding = item.positionPadding + spawnedObjects[i].positionPadding;
                            if (d < padding)
                            {
                                cancelOut = true;
                                break;
                            }
                        }
                        else
                        {
                            randomGrassLocation.text = "Found a null grass object! Removing";
                            spawnedObjects.Remove(spawnedObjects[i]);
                        }
                    }

#if TESTING
                    if (Time.timeSinceLevelLoad > lastSample + 1f)
                    {
                        hitPointText.text = "Hit point: " + tHit.Point.ToString();
                        closestGrassText.text = "No longer calculating";
                        grassObjectsCount.text = "Grass count: " + spawnedObjects.Count.ToString();
                        if (spawnedObjects.Count > 0)
                        {
                            randomGrassLocation.text = spawnedObjects[Random.Range(0, spawnedObjects.Count)].transform.position.ToString();
                        }
                        lastSample = Time.timeSinceLevelLoad;
                    }
#endif

                    if (cancelOut)
                        return;

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = Session.CreateAnchor(tHit.Point, Quaternion.identity);

                    GameObject newObj = item.DoSetup(tHit.Point, anchor.transform);
                    SpawnedItem newItem = new SpawnedItem();
                    newItem.obj = newObj; newItem.positionPadding = item.positionPadding;
                    spawnedObjects.Add(newItem);

                    newObj.transform.rotation = Quaternion.Euler(0.0f,
                        Random.Range(-360f, 360f), 0f);

                    // Use a plane attachment component to maintain y-offset from the plane (occurs after anchor updates).
                    newObj.GetComponent<PlaneAttachment>().Attach(tHit.Plane);
                }
            }
        }
    }

    /// <summary>
    /// Quit the application if there was a connection error for the ARCore session.
    /// </summary>
    private void _QuitOnConnectionErrors()
    {
        // Do not update if ARCore is not tracking.
        if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
        {
            _ShowAndroidToastMessage("This device does not support ARCore.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            Application.Quit();
        }
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    /// <param name="length">Toast message time length.</param>
    private static void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

    public int FindWeight(SpawnItem s)
    {
        return s.weight;
    }

    void SimulateAR()
    {
        if (Input.GetMouseButton(0))
        {
            bool touchedObject = false;
            Ray ray = CameraSingleton.Instance.GetActiveCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                TouchObject o = hit.collider.gameObject.GetComponent<TouchObject>();
                if (o)
                {
                    o.OnTouch();
                    touchedObject = true;
                }
            }

            if (!touchedObject)
            {
                RaycastHit tHit;
                Vector3 rotatedForward = Quaternion.Euler(Random.Range(-6f, 6f), Random.Range(-6f, 6f), 0f) * ray.direction;
                Ray rotatedRay = new Ray(ray.origin, rotatedForward);
                bool cancelOut = false;
                // Randomly select an item to spawn from the list
                SpawnItem item = WeightedSelection.RandomItem<SpawnItem>(spawnPrefabs, FindWeight);
                if (Physics.Raycast(rotatedRay, out tHit))
                {
                    for (int i = 0; i < spawnedObjects.Count; i++)
                    {
                        if (spawnedObjects[i] != null)
                        {
                            float d = Vector3.Distance(tHit.point, spawnedObjects[i].obj.transform.position);
                            float padding = item.positionPadding + spawnedObjects[i].positionPadding;
                            if (d < padding)
                            {
                                cancelOut = true;
                                break;
                            }
                        }
                        else
                        {
                            randomGrassLocation.text = "Found a null grass object! Removing";
                            spawnedObjects.Remove(spawnedObjects[i]);
                        }
                    }

#if TESTING
                    if (Time.timeSinceLevelLoad > lastSample + 1f)
                    {
                        hitPointText.text = "Hit point: " + tHit.Point.ToString();
                        closestGrassText.text = "No longer calculating";
                        grassObjectsCount.text = "Grass count: " + spawnedObjects.Count.ToString();
                        if (spawnedObjects.Count > 0)
                        {
                            randomGrassLocation.text = spawnedObjects[Random.Range(0, spawnedObjects.Count)].transform.position.ToString();
                        }
                        lastSample = Time.timeSinceLevelLoad;
                    }
#endif

                    if (cancelOut)
                        return;

                    GameObject newObj = item.DoSetup(tHit.point, GameObject.Find("TestingArea").transform);
                    SpawnedItem newItem = new SpawnedItem();
                    newItem.obj = newObj; newItem.positionPadding = item.positionPadding;
                    spawnedObjects.Add(newItem);

                    newObj.transform.rotation = Quaternion.Euler(0.0f,
                        Random.Range(-360f, 360f), 0f);
                }
            }
        }
    }
}

//Iterating over weighted list https://softwareengineering.stackexchange.com/questions/150616/return-random-list-item-by-its-weight#150618
public static class WeightedSelection
{
    public static T RandomItem<T>(this IEnumerable<T> enumerable, System.Func<T, int> weightFunc)
    {
        int totalWeight = 0; // this stores sum of weights of all elements before current
        T selected = default(T); // currently selected element
        foreach (var data in enumerable)
        {
            int weight = weightFunc(data); // weight of current element
            int r = Random.Range(0, totalWeight + weight); // random value
            if (r >= totalWeight) // probability of this is weight/(totalWeight+weight)
                selected = data; // it is the probability of discarding last selected element and selecting current one instead
            totalWeight += weight; // increase weight sum
        }

        return selected; // when iterations end, selected is some element of sequence. 
    }
}