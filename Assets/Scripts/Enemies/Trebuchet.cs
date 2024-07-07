using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Wall;

namespace Enemies
{
    [RequireComponent(typeof(Animator))]
    public class Trebuchet : MonoBehaviour
    {
        [Range(0.01f, 2f)] public float reloadSpeed = 1;

        public Transform releasePoint;

        public Transform targetPoint;

        public bool ready = true;

        private Queue<EnqueuedProjectile> orderedProjectiles;

        private int _animIDDeath;

        private int _animIDReadyWait;
        private int _animIDReloadSpeed;
        private int _animIDLaunch;

        public List<string> debug_Queue;

        private Animator _anim;

        private void Start()
        {
            _anim = GetComponent<Animator>();

            _animIDReadyWait = Animator.StringToHash("ReadySpeed");
            _animIDReloadSpeed = Animator.StringToHash("RewindSpeed");
            _animIDDeath = Animator.StringToHash("Death");
            _animIDLaunch= Animator.StringToHash("Launch");

            orderedProjectiles = new Queue<EnqueuedProjectile>();  

            _anim.SetFloat(_animIDReadyWait, UnityEngine.Random.Range(0.5f, 1.5f));

            debug_Queue = new List<string>();   
        }

        public void SetReloadSpeed(float reloadAnimSpeed)
        {
            reloadSpeed = reloadAnimSpeed;
            Invoke(nameof(SetReloadSpeed), 0.1f);
        }

        public void Kill()
        {
            _anim.SetTrigger(_animIDDeath);
        }

        private void SetReloadSpeed()
        {
            _anim.SetFloat(_animIDReloadSpeed, reloadSpeed);
        }

        public void AnimEvent_DoneReloading()
        {
            ready = true;
        }

        public void RequestLaunch(ProjectileType type, Vector3 target, int targetedWallPiece = -1)
        {
            orderedProjectiles.Enqueue(new EnqueuedProjectile(type, target, targetedWallPiece));
        }

        public void AnimEvent_Launch()
        {
            EnqueuedProjectile nextUp = orderedProjectiles.Dequeue();
            TargetProjectile projectile;
            switch (nextUp._projectileType)
            {
                case ProjectileType.Stone:
                    projectile = Instantiate(ArmyController.instance.trebuchetStonePrefab, releasePoint.position, Quaternion.identity);
                    projectile.SetWallPieceIndex(nextUp._index);
                    projectile.SetDestination(WallManager.instance.GetWallSegments().ElementAt(nextUp._index).transform.position);
                    projectile.SetUp(ArmyController.instance.trebuchetStoneSettings);
                    break;
                case ProjectileType.TarBarrel:
                    projectile = Instantiate(ArmyController.instance.tarBarrelPrefab, releasePoint.position, Quaternion.identity);
                    projectile.SetDestination(nextUp._targetWorldPos);
                    projectile.SetUp(ArmyController.instance.barrelSettings);
                    break;
                case ProjectileType.FlourBarrel:
                    projectile = Instantiate(ArmyController.instance.flourBarrelPrefab, releasePoint.position, Quaternion.identity);
                    projectile.SetDestination(nextUp._targetWorldPos);
                    projectile.SetUp(ArmyController.instance.barrelSettings);
                    break;
            }

            ready = true;
        }

        public void Update()
        {
            if (ready && orderedProjectiles.Count != 0)
            {    
                _anim.SetTrigger(_animIDLaunch);
                ready = false;
            }

            debug_Queue.Clear();
            foreach (var e in orderedProjectiles)
            {
                debug_Queue.Add(e._projectileType + " aimed at " + e._targetWorldPos);
            }
        }
    }

    public class EnqueuedProjectile
    {
        public EnqueuedProjectile(ProjectileType type, Vector3 worldPos, int wallIndex = -1)
        {
            _index = wallIndex;
            _targetWorldPos = worldPos;
            _projectileType = type;
        }
        public int _index;
        public Vector3 _targetWorldPos;
        public ProjectileType _projectileType;
    }


}