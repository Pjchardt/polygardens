using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugFlightManager : MonoBehaviour
{
    public static BugFlightManager Instance;

    List<Transform> landingSpots = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddSpot(Transform t)
    {
        landingSpots.Add(t);
    }

    public void RemoveSpot(Transform t)
    {
        landingSpots.Remove(t);
    }

    public Transform GetLandSpot()
    {
        if (landingSpots.Count < 1)
            return null;
        else
            return landingSpots[Random.Range(0, landingSpots.Count)];
    }
}
