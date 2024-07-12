using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenBarrel : MonoBehaviour
{
    public float despawnAfter = 5.0f;

    public float lifetime = 0.0f;

    private float spawnTime = 0.0f;

    private bool despawning = false;

    protected void Start()
    {
        spawnTime = Time.time;
    }

    // Update is called once per frame
    protected void Update()
    {
        lifetime = Time.time - spawnTime;
        if (lifetime >= despawnAfter)
        {
            despawning = true;
            StartCoroutine(nameof(Vanish));
        }
    }

    private IEnumerator Vanish()
    {
        while (transform.localPosition.y >= -5)
        {
            transform.position -= new Vector3(0, Time.deltaTime, 0);
            yield return null;
        }
        Destroy(gameObject);
        yield return null;
    }
}
