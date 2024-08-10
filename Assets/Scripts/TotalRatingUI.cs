using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TotalRatingUI : MonoBehaviour
{
    [SerializeField] private GameObject[] stars = new GameObject[3];
    [SerializeField] private GameObject timeTake;
    [SerializeField] private GameObject criticalTime;

    // Start is called before the first frame update
    private void Start()
    {
        //for (int j = 0; j < 3; j++)
        //{
        //    stars[j].SetActive(false);
        //}

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
        (string, string) timeStamps = RatingSystem.Instance.GetTotalRatingTimeStamps();

        //Debug.Log(timeTake);
        //Debug.Log(timeTake.GetComponent<TextMeshProUGUI>());

        //timeTake.GetComponent<TextMeshPro>();

        timeTake.GetComponent<TextMeshProUGUI>().text = timeStamps.Item1;
        criticalTime.GetComponent<TextMeshProUGUI>().text = timeStamps.Item2;

        int i = 4 - RatingSystem.Instance.GetTotalRating();
        //int i = RatingSystem.Instance.GetTotalRating();
        int j = 0;

        Debug.Log(i);

        for (; j < i; j++)
        {
            stars[j].SetActive(true);
        }

        for (; j < 3; j++)
        {
            stars[j].SetActive(false);
        }
    }
}