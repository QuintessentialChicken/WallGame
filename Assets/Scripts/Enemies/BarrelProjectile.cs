using Enemies;
using Input;
using Player;
using UnityEngine;

public class BarrelProjectile : TargetProjectile
{
    [Header("Warning Circle")]
    public bool circleEnabled = false;
    [Range(0,1.0f)]
    public float rebounceSweetSpot = 0.8f;
    public SpriteRenderer warningSpritePrefab;
    public SpriteRenderer goldenCirclePrefab;
    private SpriteRenderer warningSprite;
    private SpriteRenderer goldenCircle;

    private void Awake()
    {
        Inputs.YPressed += TryRebouncing;
    }

    private void OnDestroy()
    {
        Destroy(goldenCircle.gameObject);
        Destroy(warningSprite.gameObject);
        Inputs.YPressed -= TryRebouncing;
    }

    new void Start()
    {
        base.Start();
        if (circleEnabled)
        {
            warningSprite = Instantiate(warningSpritePrefab);
            warningSprite.transform.position = _destination + new Vector3(0, 0.2f, 0);
            warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 0);
            
            goldenCircle = Instantiate(goldenCirclePrefab);
            goldenCircle.transform.position = _destination + new Vector3(0, 0.25f, 0);
            goldenCircle.transform.localScale = Vector3.zero;
            goldenCircle.color = new Color(goldenCircle.color.r, goldenCircle.color.g, goldenCircle.color.b, 0);
        }
    }

    new void Update()
    {
        base.Update();
        if (circleEnabled)
        {
            if (t < 1)
            {
                warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, t);
            }
            else
            {
                warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 0);
            }
        }
        if (t >= rebounceSweetSpot)
        {
            float skewedT = (t - rebounceSweetSpot) / (1 - rebounceSweetSpot);
            goldenCircle.transform.localScale = Vector3.Lerp(goldenCircle.transform.localScale, 0.5f * Vector3.one, skewedT);
            goldenCircle.color = new Color(goldenCircle.color.r, goldenCircle.color.g, goldenCircle.color.b, skewedT);
        }
    }

    private void TryRebouncing()
    {
        if (t >= 0.8f &&
            Vector3.Distance(FindObjectOfType<ThirdPersonController>().transform.position, _destination) < 1)
        {
            _startOfLife = Time.time;
            _arrivalTime = _startOfLife + settings.flightTime;
            Vector3 temp = _destination + new Vector3(0,1.5f,0);
            _destination = new Vector3(_releasePoint.x, 0, _releasePoint.z);
            _releasePoint = temp;

            warningSprite.transform.position = _destination + new Vector3(0, 0.2f, 0);
            goldenCircle.transform.position = _destination + new Vector3(0, 0.25f, 0);
        }
    }
}