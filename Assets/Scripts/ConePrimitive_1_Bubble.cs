using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConePrimitive_1_Bubble : TouchObject
{
    public AudioClip[] PopAudioClips;
    public GameObject PopParticlePrefab;

    float timeElapsed;
    public Vector3 startScale, endScale;

    private void Start()
    {
        transform.localScale = startScale;
        StartCoroutine(WaitToDestroy());
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        transform.localScale = Vector3.Lerp(startScale, endScale, timeElapsed * .1f);

        transform.position += Vector3.up * Time.deltaTime * .05f;
        transform.position += Random.insideUnitSphere * Time.deltaTime * .05f;
    }

    public override void OnTouch()
    {
        GameObject o = Instantiate(PopParticlePrefab) as GameObject;
        o.transform.position = transform.position;
        o.transform.LookAt(CameraSingleton.Instance.GetActiveCamera().transform);
        AudioSource aS = o.AddComponent<AudioSource>();
        aS.clip = PopAudioClips[Random.Range(0, PopAudioClips.Length)];
        aS.Play();
        Destroy(o, 1f);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
