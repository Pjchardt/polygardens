using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdOfParadiseBug : TouchObject
{
    enum BugState { Landed, Flying, Landing};
    BugState currentState;

    public AudioClip[] touchAudioClips;

    public ParticleSystem p;
    Animation a;
    public Transform flightCenter;
    Transform targetLand;
    Vector3 targetFlightSpot;
    float timeToWait;

    private void Awake()
    {
        a = GetComponent<Animation>();
    }

    void Start ()
    {
        currentState = BugState.Landed;
        timeToWait = 3f;
	}
	
	void Update ()
    {
        Vector3 dir;

        switch (currentState)
        {
            case BugState.Landed:
                timeToWait -= Time.deltaTime;
                if (timeToWait < 0)
                {
                    currentState = BugState.Flying;
                    GetNewFlightPath();
                }
                break;
            case BugState.Flying:
                dir = targetFlightSpot - transform.position;
                if (dir.magnitude < .01)
                    GetNewFlightPath();
                else
                {
                    transform.position += dir.normalized * Time.deltaTime * .025f + Random.insideUnitSphere * .0005f;
                    transform.LookAt(dir);
                }
                break;
            case BugState.Landing:
                if (targetLand == null)
                {
                    GetNewFlightPath();
                    return;
                }
                dir = targetLand.position - transform.position;
                if (dir.magnitude < .01)
                    Land();
                else
                {
                    transform.position += dir.normalized * Time.deltaTime * .025f + Random.insideUnitSphere * .0005f;
                    transform.LookAt(dir);
                }
                break;
        }
	}

    void Land()
    {
        currentState = BugState.Landed;
        timeToWait = Random.Range(3f, 20f);
        transform.position = targetLand.position;
        transform.localRotation = targetLand.localRotation;
        transform.parent = targetLand;
    }

    void GetNewFlightPath()
    {
        int rand = Random.Range(0, 10);
        if (rand < 3)
        {
            currentState = BugState.Landing;
            targetLand = BugFlightManager.Instance.GetLandSpot();
            return;
        }
        else
        {
            targetFlightSpot = flightCenter.position + Random.insideUnitSphere * .2f;
        }
        transform.parent = null;
    }

    public override void OnTouch()
    {
        p.Emit(Random.Range(5,10));
        GameObject o = new GameObject();
        o.transform.position = transform.position;
        AudioSource aS = o.AddComponent<AudioSource>();
        aS.clip = touchAudioClips[Random.Range(0, touchAudioClips.Length)];
        aS.Play();
        a.Play();
        Destroy(o, 3f);
    }
}
