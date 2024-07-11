using System.Collections.Generic;
using UnityEngine;

//structure to hold rating relevant information for each day
public struct RatingTime
{
    public float startTime;
    public float endTime;
    public float criticalTime;

    public RatingTime(float startTime_)
    {
        startTime = startTime_;
        endTime = 0f;
        criticalTime = 0f;
    }
}

public class RatingSystem : MonoBehaviour
{
    public static RatingSystem Instance;

    //list to hold the structure for the whole game
    private List<RatingTime> ratingTimes;

    private float startCriticalTime = 0f;
    private float endCriticalTime = 0f;

    [SerializeField] private float twoStarRatingPercentage = 0f;
    [SerializeField] private float threeStarRatingPercentage = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ratingTimes = new List<RatingTime>();
    }

    private RatingTime GetLatestRatingTime()
    {
        int lastIndex = ratingTimes.Count - 1;
        return ratingTimes[lastIndex];
    }

    private void SetLatestRatingTime(RatingTime ratingTime)
    {
        int lastIndex = ratingTimes.Count - 1;
        ratingTimes[lastIndex] = ratingTime;
    }

    //create the tuple for current day and set start time
    public void SetRatingTime()
    {
        float startTime = Time.fixedTime;
        RatingTime myTuple = new RatingTime(startTime);
        ratingTimes.Add(myTuple);
    }

    //set endtime for current day
    public void SetEndTime()
    {
        float endTime = Time.fixedTime;

        int lastIndex = ratingTimes.Count - 1;

        RatingTime ratingTime = GetLatestRatingTime();
        ratingTime.endTime = endTime;

        ratingTimes[lastIndex] = ratingTime;
    }

    //set critical start time
    public void SetStartCritialTime()
    {
        startCriticalTime = Time.fixedTime;
    }

    //set critical end time
    public void SetEndCriticalTime()
    {
        endCriticalTime = Time.fixedTime;
        AddCriticalEndTime();
        ResetCriticalTimes();
    }

    //add to the total critical end time for current day
    private void AddCriticalEndTime()
    {
        float criticalTime = startCriticalTime - endCriticalTime;
        
        int lastIndex = ratingTimes.Count - 1;

        RatingTime ratingTime = ratingTimes[lastIndex];
        ratingTime.criticalTime += criticalTime;

        ratingTimes[lastIndex] = ratingTime;
    }

    //reset critical time
    private void ResetCriticalTimes()
    {
        startCriticalTime = 0f;
        endCriticalTime = 0f;
    }

    //get rating for current day
    public int GetRating()
    {
        RatingTime ratingTime = GetLatestRatingTime();
        return GetRating(ratingTime);
    }

    private int GetRating(RatingTime ratingTime)
    {
        float playTime = ratingTime.endTime - ratingTime.startTime;

        float percentageCritical = ratingTime.criticalTime / playTime * 100;

        if (percentageCritical > threeStarRatingPercentage)
            return 3;

        if (percentageCritical > twoStarRatingPercentage)
            return 2;

        return 1;
    }

    //get rating for whole game
    public int GetTotalRating()
    {
        RatingTime ratingTime;
        int currentRating = 0;

        for (int i = 0; i < ratingTimes.Count; i++) 
        {
            ratingTime = ratingTimes[i];
            currentRating += GetRating(ratingTime);
        }

        currentRating /= ratingTimes.Count;

        return currentRating;
    }
}
