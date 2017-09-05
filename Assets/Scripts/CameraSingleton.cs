using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    public static CameraSingleton Instance;

    public Camera ARCamera;
    public Camera TestCamera;

    Camera activeCamera;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
#if !UNITY_EDITOR
        activeCamera = ARCamera;
#else
        activeCamera = TestCamera;
#endif
    }

    public Camera GetActiveCamera()
    {
        return activeCamera;
    }
}
