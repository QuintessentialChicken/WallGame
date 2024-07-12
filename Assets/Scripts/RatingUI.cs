using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingUI : MonoBehaviour
{
    [SerializeField] private GameObject[] stars = new GameObject[3];

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
    }
}
