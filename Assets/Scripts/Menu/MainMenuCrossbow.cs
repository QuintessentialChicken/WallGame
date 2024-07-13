using Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuCrossbow : MonoBehaviour
{
    public float targetRotation = 0;
    public float bop = 0.2f;
    public float swingSpeed = 0.8f;
    public Animator wallther;
    public TargetProjectile bolt;
    [Space]
    public GameObject schild1tutorial;
    public GameObject schild2freeplay;
    public GameObject schild3quit;

    [Space]
    public Transform crossbow;
    public Transform bopParent;

    private Quaternion initialRotation;

    public void OnAim(InputAction.CallbackContext context)
    {
        
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x > 0.1)
        {
            targetRotation = 20;
        } else if (input.x < -0.1)
        {
            targetRotation = -15;
        } else
        {
            targetRotation = 0;
        }
    }

    bool ready = true;

    public void OnSelect()
    {
        if (!ready) return;
        ready = false;
        
        
        wallther.SetTrigger("Dodge");
        TargetProjectile spawned = Instantiate(bolt, crossbow.transform.position, crossbow.transform.rotation);
        spawned.SetDestination(transform.position + crossbow.forward * 3.5f);
        spawned.Fire();
        float floatingX = crossbow.localRotation.eulerAngles.y;
        
        if (currentSelection == 2)
        {
            StartCoroutine(nameof(DelayedQuit));
        } else if (currentSelection == 0)
        {
            StartCoroutine(nameof(DelayedTutorialStart));
        } else {
            StartCoroutine(nameof(DelayedFreePlayStart));
        }

        StartCoroutine(nameof(Reloading));
    }

    public IEnumerator DelayedQuit()
    {
        yield return new WaitForSeconds(0.1f); 
        schild3quit.transform.parent.Rotate(-5, 0, 0);
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    public IEnumerator DelayedTutorialStart()
    {
        yield return new WaitForSeconds(0.1f);
        schild1tutorial.transform.parent.Rotate(-5, 0, 0);
        yield return new WaitForSeconds(0.5f);
        DayNightManager.instance.RequestChangeToTutorial();
    }

    public IEnumerator DelayedFreePlayStart()
    {
        yield return new WaitForSeconds(0.1f);
        schild2freeplay.transform.parent.Rotate(-5, 0, 0);
        yield return new WaitForSeconds(0.5f);
        DayNightManager.instance.RequestChangeToDay();
    }

    IEnumerator Reloading()
    { 
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            crossbow.transform.localPosition = new Vector3 (0, -0.41f, -0.5f*(t-1)*(t-1));
            yield return null;
        }
        ready = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        initialRotation = bopParent.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetRotation == 20)
        {
            schild1tutorial.SetActive(false);
            schild2freeplay.SetActive(false);
            schild3quit.SetActive(true);
            currentSelection = 2;
        } else if (targetRotation == -15)
        {
            schild1tutorial.SetActive(true);
            schild2freeplay.SetActive(false);
            schild3quit.SetActive(false);
            currentSelection = 0;
        } else
        {
            schild1tutorial.SetActive(false);
            schild2freeplay.SetActive(true);
            schild3quit.SetActive(false);
            currentSelection = 1;
        }
            //Debug.Log(crossbow.localRotation.eulerAngles.y);
        bopParent.transform.localRotation = initialRotation;
        bopParent.transform.Rotate(bop * Mathf.Sin(Time.time), 0, 0);

        crossbow.localRotation = Quaternion.Lerp(crossbow.localRotation, Quaternion.Euler(0, targetRotation, 0), swingSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, targetRotation/2, 0), swingSpeed * Time.deltaTime);
    }

    private int currentSelection = 0;
}
