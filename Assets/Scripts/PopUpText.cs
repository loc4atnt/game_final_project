using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpText : MonoBehaviour
{
    long lastActive;
    long duration = 500;
    bool activated = false;

    // Start is called before the first frame update
    long getTime()
    {
        DateTime currentTime = DateTime.UtcNow;
        long milisecond = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
        return milisecond;
    }
    void Start()
    {
        activated = true;
        lastActive = getTime();
    }

    // Update is called once per frame
    void Update()
    {
        if (getTime() - lastActive > 500)
        {
            gameObject.SetActive(false);
        }
    }
}
