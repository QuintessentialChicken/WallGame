using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonkeyEars : MonoBehaviour
{
    public GameObject donkeyEarsPrefab;

    public Transform playerHead;

    private CharacterController player;

    private GameObject ears;
    
    void Start()
    {
        ears = Instantiate(donkeyEarsPrefab);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hallo!!");
        if (other.GetComponent<ThirdPersonController>() == null) return;
        
        player = other.GetComponent<CharacterController>();
        player.enabled = false;
        other.gameObject.transform.position = new Vector3(0, 10, -8);

        ears.transform.parent = playerHead;
        ears.transform.localRotation = Quaternion.identity;
        ears.transform.localPosition = Vector3.zero;
        ears.transform.localScale = Vector3.one;

        player.enabled = true;
    }


}
