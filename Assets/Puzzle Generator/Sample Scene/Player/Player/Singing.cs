using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singing : MonoBehaviour
{
    [Header("SongProperties")]
    public AudioSource SongSource;
    public AudioClip SongClip;

    [Space(10)]
    [Range(0.0f, 1.0f)]
    public float[] SongPartPercentages;

    private float StopAtTime;

    [Header("Controls")]
    public KeyCode SingButton;


    [Header("Debug")]
    private float SongTimeStamp;
    private int CurrentSongPart;

    bool IsSinging = false;

    void Awake()
    {
        // Make sure there is a audio clip
        if (!SongClip) { Debug.LogWarning("Player has no AudioClip for song"); Debug.Break();}
    }


    // Update is called once per frame
    void Update()
    {
        // Set the current position of the current song
        SongTimeStamp = SongSource.time;
        
        if (Input.GetKeyDown(SingButton))
        {
            IsSinging = true;
            // Make sure song isn't already playing
            if (!SongSource.isPlaying)
            {
                SongSource.Play();

                // Set the timestamp to stop the song at
                StopAtTime = SongSource.clip.length * SongPartPercentages[CurrentSongPart];
                CurrentSongPart++;

                // Loop the song parts back
                if (CurrentSongPart >= SongPartPercentages.Length)
                {
                    CurrentSongPart = 0;
                }
            }
            
        }
        else
        {
            IsSinging = false;
        }

        // Stop the audio when it reaches a certain duration
        if (SongTimeStamp >= StopAtTime)
        {
            SongSource.Pause();
        }
    }

    // To make the song part percentages dummy resistant (Because someone will still find a way to break it)
    private void OnValidate()
    {
        // Get the value of the first element
        float prev = SongPartPercentages[0];

        // Loop through array
        for (int i = 0; i < SongPartPercentages.Length; i++)
        {
            // Clamp the current value's minimum to the value of the previous element
            if (SongPartPercentages[i] <= prev)
            {
                SongPartPercentages[i] = prev;
            }

            // Set new previous value
            prev = SongPartPercentages[i];
        }
    }

    public bool GetSinging()
    {
        return IsSinging;
    }
}
