using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundTest : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shipHorn;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = shipHorn;
    }

}
