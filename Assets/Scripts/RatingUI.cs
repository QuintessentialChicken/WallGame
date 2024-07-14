using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RatingUI : MonoBehaviour
{
    [SerializeField] private GameObject[] stars = new GameObject[3];

    [SerializeField] private TextMeshProUGUI totalTimeText;
    
    [SerializeField] private TextMeshProUGUI criticalTimeText;

    // Start is called before the first frame update
    private void Start()
    {
        for (int j = 0; j < 3; j++)
        {
            stars[j].SetActive(false);
        }

        DisplayRating();
    }

    private void DisplayRating()
    {
        int i = RatingSystem.Instance.GetRating();
        //int i = RatingSystem.Instance.GetTotalRating();
        int j = 0;

        for (j = 0; j < i; j++)
        {
            stars[j].SetActive(true);
        }

        for (; j < 3; j++)
        {
            stars[j].SetActive(false);
        }

        totalTimeText.text = RatingSystem.Instance.GetRatingTimeStamps().Item1;
        criticalTimeText.text = RatingSystem.Instance.GetRatingTimeStamps().Item2;
    }
}
