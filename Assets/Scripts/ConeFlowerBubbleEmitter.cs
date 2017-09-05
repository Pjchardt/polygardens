using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeFlowerBubbleEmitter : MonoBehaviour
{
    bool emitting = false;
    public GameObject BubblePrefab;

    public void EmitOne()
    {
        Instantiate(BubblePrefab, transform.position, Quaternion.identity);
    }

    public void EnableEmission(bool b)
    {
        if (b && !emitting)
            StartCoroutine(WaitAndEmit());
        else if (!b)
            StopAllCoroutines();

        emitting = b;
    }

    IEnumerator WaitAndEmit()
    {
        yield return new WaitForSeconds(1.5f);

        Instantiate(BubblePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(WaitAndEmit());
    }
}
