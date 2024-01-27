using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource audioPlayer;

    public void play(int id)
    {
        audioPlayer.clip = audioClips[id];
        audioPlayer.Play();
        audioPlayer.loop = false;
    }

    public bool isPlaying()
    {
        return audioPlayer.isPlaying;
    }

    public void stopAudio()
    {
        audioPlayer.Stop();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
