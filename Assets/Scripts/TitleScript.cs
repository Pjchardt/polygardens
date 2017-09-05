using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour
{
    public GardenController g;
    public GameObject TitleRoot;
    //stuff to run off

    private void Update()
    {
        if (Input.touches.Length > 0 || Input.GetMouseButtonDown(0))
        {
            StartDemo();
        }
    }

    void StartDemo()
    {
        g.enabled = true;
        TitleRoot.SetActive(false);
    }
}
