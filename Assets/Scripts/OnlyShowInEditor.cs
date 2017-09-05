using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyShowInEditor : MonoBehaviour
{
#if !UNITY_EDITOR
    void Awake ()
    {
        gameObject.SetActive(false);
    }
#endif
}
