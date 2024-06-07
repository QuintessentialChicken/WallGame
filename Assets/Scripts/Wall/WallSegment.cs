using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wall
{
    public class WallSegment : MonoBehaviour
    {
        public GameObject wallPiece;
        public GameObject scaffoldingPiece;
        public FriendlySoldier soldier;

        public int health = 2;
        public bool isScaffoldingIntact = true;
        public bool soldierRequested = false;
        public bool isSoldierPresent;
        public Mesh normalWall;
        public Mesh damagedWall;
        public Mesh destroyedWall;
        public int level; //To be used to request soldier at the correct level
        public bool chosenOne;

        private MeshFilter _meshFilter;
        private void Start()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            normalWall = _meshFilter.mesh;
            // UpdateSoldierState();
        }

        public void RepairWall()
        {
            health = Mathf.Min(2, health + 1);
            ChangeWallState();
        }

        public void DamageWall()
        {
            health = Mathf.Max(0, health - 1);
            ChangeWallState();
        }

        private void ChangeWallState()
        {
            _meshFilter.mesh = health switch
            {
                0 => destroyedWall,
                1 => damagedWall,
                2 => normalWall, 
                _ => _meshFilter.mesh
            };
        }

        public void RepairScaffolding()
        {
            isScaffoldingIntact = true;
            scaffoldingPiece.SetActive(true);
            WallManager.instance.RequestSoldier(this);
            // UpdateSoldierState();
        }

        private GUIStyle _style = new GUIStyle();
        private void OnDrawGizmos()
        {
            _style.fontSize = 32;
            if (chosenOne) Handles.Label(transform.position + new Vector3(0, 3, 0), "Wall Health: " + health, _style);
        }

        public void DamageScaffolding()
        {
            isScaffoldingIntact = false;
            scaffoldingPiece.SetActive(false);
            if (isSoldierPresent) soldier.Die();
            Invoke("RepairScaffolding", 1);
            // UpdateSoldierState();
        }

        // private void UpdateSoldierState()
        // {
        //     if (isScaffoldingIntact && _isSoldierPresent)
        //     {
        //         soldier.SetActive(true);
        //     }
        //     else
        //     {
        //         soldier.SetActive(false);
        //     }
        // }

        public void AssignSoldier(FriendlySoldier incomingSoldier)
        {
            soldier = incomingSoldier;
            isSoldierPresent = true;
            soldier.transform.SetParent(transform);
        }
    }
}