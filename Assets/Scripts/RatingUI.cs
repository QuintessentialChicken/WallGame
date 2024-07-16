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

        Test();
    }
    
    private void Test()
    {
        Debug.Log(RatingSystem.Instance.GetRatingTimeStamps());
        Debug.Log(RatingSystem.Instance.GetTotalRatingTimeStamps());
    }

    private void DisplayRating()
    {
        int i = 4 - RatingSystem.Instance.GetRating();
        //int i = RatingSystem.Instance.GetTotalRating();
        int j;

        for (j =0; j < i; j++)
        {
            stars[j].SetActive(true);
        }

        for (; j < 3; j++)
        {
            stars[j].SetActive(false);
        }
    }
}
