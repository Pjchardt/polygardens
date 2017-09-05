using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedMushroom : TouchObject
{
    public AudioClip[] touchSounds;
    AudioSource a;
    Animation anim;

    private void Awake()
    {
        a = GetComponent<AudioSource>();
        anim = GetComponent<Animation>();
    }

    public override void OnTouch()
    {
        anim.Play("MushroomBounce");
        a.PlayOneShot(touchSounds[Random.Range(0, touchSounds.Length)]);
    }

}
