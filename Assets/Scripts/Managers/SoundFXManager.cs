using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void playSoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume) 
    {  
        // spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // spawn in audioClip
        audioSource.clip = audioClip;

        // assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        // get length of sound clip
        float clipLength = audioSource.clip.length;

        // destroy gameObject after clip has ended
        Destroy(audioSource.gameObject, clipLength);
    }
}
