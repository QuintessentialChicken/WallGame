using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenTarBarrel : BrokenBarrel
{
    public GameObject Tar;

    public float tarpitScale = 0.8f;

    // Start is called before the first frame update
    new void Start()
    {
        Tar.transform.localScale = new Vector3(0.5f, 2, 0.5f);
        base.Start();
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
    new void Update()
    {
        Tar.transform.localScale = Vector3.Lerp(Tar.transform.localScale, tarpitScale * Vector3.one, Time.deltaTime);
        base.Update();
    }
}
