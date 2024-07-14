using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference drumStartMusic { get; private set; }
    [field: SerializeField] public EventReference nightAmbientMusic { get; private set; }
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference battleAmbience { get; private set; }


    [field: Header("SFX")]

    // Wallther
    [field: SerializeField] public EventReference walltherFootSteps { get; private set; }
    [field: SerializeField] public EventReference walltherLand { get; private set; }
    [field: SerializeField] public EventReference walltherGasp { get; private set; }
    [field: SerializeField] public EventReference walltherGrunt { get; private set; }
    [field: SerializeField] public EventReference walltherRallyingTroops { get; private set; }

    // Interactions
    [field: SerializeField] public EventReference skillUpgrade { get; private set; }
    [field: SerializeField] public EventReference fixWood { get; private set; }
    [field: SerializeField] public EventReference fixStone { get; private set; }
    [field: SerializeField] public EventReference pickStone { get; private set; }
    [field: SerializeField] public EventReference pickWood { get; private set; }
    [field: SerializeField] public EventReference mountCatapult { get; private set; }
    [field: SerializeField] public EventReference exitCatapult { get; private set; }

    // Enemy
    [field: SerializeField] public EventReference shootTrebuchet { get; private set; }
    [field: SerializeField] public EventReference shootArrow { get; private set; }
    [field: SerializeField] public EventReference barrelBreaking { get; private set; }
    // Damage
    [field: SerializeField] public EventReference damageWall { get; private set; }
    [field: SerializeField] public EventReference burnScaffolding { get; private set; }
    [field: SerializeField] public EventReference enemyArrowWhoosh { get; private set; }

    // Crossbowmen
    // [field: SerializeField] public EventReference crossbowmanSpawn { get; private set; }
    [field: SerializeField] public EventReference crossbowmanDie { get; private set; }
    [field: SerializeField] public EventReference crossbowmanScream { get; private set; }



    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}