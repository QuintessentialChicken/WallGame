using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarPit : MonoBehaviour
{
    public GameObject Tar;

    public float despawnAfter = 5.0f;

    public float lifetime = 0.0f;

    private float spawnTime = 0.0f;

    private bool despawning = false;

    public float tarpitScale = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        Tar.transform.localScale = new Vector3(0.5f, 2, 0.5f);
        spawnTime = Time.time;  
    }

    private void OnTriggerStay(Collider other)
    {
        other.gameObject.GetComponent<ThirdPersonController>()?.GetStickyFeet();
    }

    private void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<ThirdPersonController>()?.LeavingTarpit();
    }

    // Update is called once per frame
    void Update()
    {
        if (despawning) return;
        Tar.transform.localScale = Vector3.Lerp(Tar.transform.localScale, tarpitScale * Vector3.one, Time.deltaTime);
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
