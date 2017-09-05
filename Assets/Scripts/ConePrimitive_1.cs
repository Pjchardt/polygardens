using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConePrimitive_1 : MonoBehaviour
{
    GameObject coneLid;
    GameObject coneBase;

    //ParticleSystem p;
    //ParticleSystem.EmissionModule e;
    ConeFlowerBubbleEmitter c;

    float closedAngle = 0f, openAngle = -135f;
    float openSpeed = .5f, closeSpeed = 3f;

    private void Awake()
    {
        coneLid = transform.Find("ConeLid").gameObject;
        coneBase = transform.Find("ConeBase").gameObject;
        //p = transform.Find("ConeParticleSystem").GetComponent<ParticleSystem>();
        //e = p.emission;
        //e.enabled = false;
        c = transform.Find("ConeParticleSystem").GetComponent<ConeFlowerBubbleEmitter>();
    }

    private void Start()
    {
        StartCoroutine(InitialDelay());
    }

    IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(8f);
        StartCoroutine(OpenLid());
    }

    IEnumerator OpenLid()
    {
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            coneLid.transform.localRotation = Quaternion.Euler(Mathf.Lerp(closedAngle, openAngle, t), 0f, 0f);
            yield return new WaitForEndOfFrame();
        }
        coneLid.transform.localRotation = Quaternion.Euler(openAngle, 0f, 0f);

        //c.EnableEmission(true);
        c.EmitOne();

        yield return new WaitForSeconds(5f);

        StartCoroutine(CloseLid());
    }

    IEnumerator CloseLid()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * closeSpeed;
            coneLid.transform.localRotation = Quaternion.Euler(Mathf.Lerp(openAngle, closedAngle, t), 0f, 0f);
            yield return new WaitForEndOfFrame();
        }
        coneLid.transform.localRotation = Quaternion.Euler(closedAngle, 0f, 0f);

        //c.EnableEmission(false);

        yield return new WaitForSeconds(20f);

        StartCoroutine(OpenLid());
    }
}
