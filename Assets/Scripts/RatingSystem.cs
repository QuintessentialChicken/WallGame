using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingSystem : MonoBehaviour
{
    public static RatingSystem Instance;

    [SerializeField] private float timeStar1;
    [SerializeField] private float timeStar2;

    private float startTime;
    private float playTime;

    private void Awake()
    {
        Instance = this;
    }

    public void SetStartTime()
    {
        startTime = Time.unscaledTime;
    }

    public void SetEndTIme()
    {
        playTime = Time.unscaledTime - startTime;
    }

    public int GetRating()
    {
        int i;
        if (playTime < timeStar1)
            i = 1;
        else if (playTime < timeStar2)
            i = 2;
        else
            i = 3;

        return i;
    }
}
