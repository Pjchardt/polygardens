using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedFlower : TouchObject
{
    bool flowerOpen = false;

    public AudioSource TapSoundSource;
    public AudioClip[] tapSounds;
    public RedFlowerPedal[] petals;

    public override void OnTouch()
    {
        TapSoundSource.PlayOneShot(tapSounds[Random.Range(0, tapSounds.Length)]);
        
        for (int i = 0; i < petals.Length; i++)
        {
            if (flowerOpen)
                petals[i].ClosePetals();
            else
                petals[i].OpenPetals();
        }

        flowerOpen = !flowerOpen;
    }
}
