using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    [RequireComponent(typeof(Animator))]
    public class Trebuchet : MonoBehaviour
    {
        [Range(0.01f, 2f)] public float reloadSpeed = 1;

        public ProjectileSettings projectileSettings;

        public Transform releasePoint;

        public Transform targetPoint;

        public bool ready = true;
        private int _animIDDeath;

        private int _animIDReadyWait;
        private int _animIDReloadSpeed;
        private int _animIDLaunch;

        private Animator _anim;
        
        private TargetProjectile _projectile;

        public Queue<TargetWallSegmentTuple> targetedWallSegments;

        public Queue<int> indicesLaunchedAt;

        private void Start()
        {
            _anim = GetComponent<Animator>();

            _animIDReadyWait = Animator.StringToHash("ReadySpeed");
            _animIDReloadSpeed = Animator.StringToHash("RewindSpeed");
            _animIDDeath = Animator.StringToHash("Death");
            _animIDLaunch= Animator.StringToHash("Launch");
            
            targetedWallSegments = new Queue<TargetWallSegmentTuple>();
            indicesLaunchedAt = new Queue<int>();   

            _anim.SetFloat(_animIDReadyWait, Random.Range(0.5f, 1.5f));
        }

        public void SetUp(ProjectileSettings projectileSettings, float reloadAnimSpeed)
        {
            this.projectileSettings = projectileSettings;
            reloadSpeed = reloadAnimSpeed;
            Invoke(nameof(SetReloadSpeed), 0.1f);
        }

        public void SetFlightTime(float flightTime)
        {
            projectileSettings.flightTime = flightTime;
        }

        public void SetParabolaHeight(float parabolaHeight)
        {
            projectileSettings.parabolaHeight = parabolaHeight;
        }

        public void Kill()
        {
            _anim.SetTrigger(_animIDDeath);
        }

        private void SetReloadSpeed()
        {
            _anim.SetFloat(_animIDReloadSpeed, reloadSpeed);
        }

        public void SetSelection(Vector3 target, int index)
        {
            targetedWallSegments.Enqueue(new TargetWallSegmentTuple(index, target));
        }

        public void AnimEvent_DoneReloading()
        {
            ready = true;
        }

        public bool Launch()
        {
            _anim.SetTrigger(_animIDLaunch);
            ready = false;
            return true;
        }

        public void AnimEvent_Launch()
        {
            var projectile = Instantiate(projectileSettings.prefab, releasePoint.position, Quaternion.identity);

            projectile.SetDestination(targetedWallSegments.Peek()._worldPos);
            projectile.SetSettings(projectileSettings);


            indicesLaunchedAt.Enqueue(targetedWallSegments.Dequeue()._index);
            Invoke(nameof(WallPieceHit), projectileSettings.flightTime);
        }

        private void WallPieceHit()
        {
            //Debug.Log("Wall piece " + lastSelected + " hit!");
            EventManager.RaiseOnWallPieceHit(indicesLaunchedAt.Dequeue());
        }
    }

    public class TargetWallSegmentTuple
    {
        public TargetWallSegmentTuple(int index, Vector3 worldPos)
        {
            _index = index;
            _worldPos = worldPos;
        }
        public int _index;
        public Vector3 _worldPos;
    }
}