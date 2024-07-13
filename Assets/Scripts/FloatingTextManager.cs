using TMPro;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager instance;
    private float cloneDeathTimer = 1f;

    [SerializeField] GameObject player;

    private Canvas canvas;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventManager.OnReplenishWood += ReplenishWood;
        EventManager.OnReplenishStone += ReplenishStone;
        EventManager.OnEnterCatapult += EnterCatapult;
        EventManager.OnCatapultFire += GetLaunched;

        canvas = gameObject.transform.parent.GetComponent<Canvas>();
    }

    private void OnDestroy()
    {
        EventManager.OnReplenishWood -= ReplenishWood;
        EventManager.OnReplenishStone -= ReplenishStone;
        EventManager.OnEnterCatapult -= EnterCatapult;
        EventManager.OnCatapultFire -= GetLaunched;
    }

    private void ReplenishWood(int i)
    {
        DoFloatingText("Wood Refilled!");
    }

    private void ReplenishStone(int i)
    {
        DoFloatingText("Stone Refilled!");
    }

    private void EnterCatapult(Transform catapultBowl)
    {
        DoFloatingText("Set!");
    }

    private void GetLaunched(Vector3[] path, int vertexCount)
    {
        DoFloatingText("Weee!");
    }

    public void DoFloatingText(string str)
    {
        Vector3 offset = new Vector3(-80, 70, 0);

        GameObject duplicatedGameObject = Instantiate(gameObject.transform.GetChild(0).gameObject, transform);
        //Destroy(duplicate, cloneDeathTimer);

        //duplicate.transform.parent = gameObject.transform;
        duplicatedGameObject.SetActive(true);

        GameObject textObject = duplicatedGameObject.transform.GetChild(0).gameObject;

        textObject.GetComponent<TextMeshProUGUI>().SetText(str);
        textObject.GetComponent<TextMeshProUGUI>().enabled = true;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(player.transform.position);

        //Debug.Log(screenPosition);

        //duplicatedGameObject.GetComponent<RectTransform>().position = screenPosition + offset;
        //duplicatedGameObject.GetComponent<RectTransform>().position.z = 0;
        //textObject.GetComponent<RectTransform>().position = Vector2.zero;

        //float scaleFactor = canvas.scaleFactor;
        //screenPosition /= scaleFactor;

        //rectTransform.anchoredPosition = screenPosition;

        duplicatedGameObject.transform.localPosition = screenPosition + offset;
        textObject.transform.localPosition = Vector3.zero;
        textObject.GetComponent<Animator>().enabled = true;
    }
}
