using System.Collections.Generic;
using UnityEngine;

//structure to hold rating relevant information for each day
public class RatingTime
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
    private List<RatingTime> ratingTimes = new List<RatingTime>();

    private float startCriticalTime = 0f;
    private float endCriticalTime = 0f;

    [SerializeField] private float twoStarRatingThreshold = 0.3f;
    [SerializeField] private float threeStarRatingThreshold = 0.7f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private RatingTime GetLatestRatingTime()
    {
        int lastIndex = ratingTimes.Count - 1;
        return ratingTimes[lastIndex];
    }

    //create the tuple for current day and set start time
    public void SetRatingTime()
    {
        float startTime = Time.fixedTime;
        RatingTime myTuple = new RatingTime(startTime);
        ratingTimes.Add(myTuple);
        Debug.Log(ratingTimes.Count);
    }

    //set endtime for current day
    public void SetEndTime()
    {
        float endTime = Time.fixedTime;

        int lastIndex = ratingTimes.Count - 1;

        Debug.Log(lastIndex);

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
    public void SetEndCriticalTime(bool gameOver = false)
    {
        endCriticalTime = Time.fixedTime;
        AddCriticalEndTime(gameOver);
        ResetCriticalTimes();
    }

    //add to the total critical end time for current day
    private void AddCriticalEndTime(bool gameOver = false)
    {
        float criticalTime;

        if (gameOver)
            criticalTime = Time.fixedTime - startCriticalTime;
        else
            criticalTime = endCriticalTime - startCriticalTime;
        
        int lastIndex = ratingTimes.Count - 1;

        ratingTimes[lastIndex].criticalTime += criticalTime;
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

        float percentageCritical = ratingTime.criticalTime / playTime;

        if (percentageCritical > threeStarRatingThreshold)
            return 3;

        if (percentageCritical > twoStarRatingThreshold)
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
