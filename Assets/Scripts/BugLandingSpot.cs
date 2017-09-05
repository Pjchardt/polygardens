using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugLandingSpot : MonoBehaviour
{
    private void Start()
    {
        BugFlightManager.Instance.AddSpot(this.transform);
    }

    private void OnDestroy()
    {
        BugFlightManager.Instance.RemoveSpot(this.transform);
    }
}
