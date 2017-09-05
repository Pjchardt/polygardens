using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedFlowerPedal : MonoBehaviour
{
    Quaternion closedRotation;
    public Vector3 openRotationEuler;
    Quaternion openRotation;

    float openSpeed = 2f, closeSpeed = 5f;
    float angleMax;

	void Start ()
    {
        openRotation = Quaternion.Euler(openRotationEuler);
        closedRotation = transform.localRotation;
        angleMax = Quaternion.Angle(closedRotation, openRotation);
	}
	
    public void OpenPetals()
    {
        StopAllCoroutines();
        float time = Quaternion.Angle(closedRotation, transform.localRotation) / angleMax;
        StartCoroutine(RotatePetals(time, openSpeed, closedRotation, openRotation));
    }

    public void ClosePetals()
    {
        StopAllCoroutines();
        float time = Quaternion.Angle(openRotation, transform.localRotation) / angleMax;
        StartCoroutine(RotatePetals(time, closeSpeed, openRotation, closedRotation));
    }

    IEnumerator RotatePetals(float t, float speed, Quaternion start, Quaternion end)
    {
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Lerp(start, end, t);
            yield return new WaitForEndOfFrame();
        }

        transform.localRotation = end;
    }


}
