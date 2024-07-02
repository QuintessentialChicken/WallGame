using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    public class TargetProjectile : MonoBehaviour
    {
        ProjectileSettings settings;
        [Header("Warning Circle")]
        public bool circleEnabled = false;
        public SpriteRenderer warningSpritePrefab;
        private SpriteRenderer warningSprite;

        [Header("Refernces")]

        [SerializeField]
        [Tooltip(
            "This MeshRenderer is deactivated on impact. The invisible GameObject will linger for 1 second and wait for the trail to finish.")]
        private MeshRenderer meshRenderer;

        [Header("On Impact")]
        public bool damageWallPieceOnHit = false;
        [SerializeField]
        private int wallPieceToHit = -1;

        public bool spawnParticlesOnHit;

        [SerializeField] private GameObject onHitParticles;

        public bool particlesSpawned;
        private float _arrivalTime;

        private Vector3 _destination;
        private Vector3 _lastPosition;
        private Vector3 _releasePoint;

        private float _startOfLife = -1;

        public void Start()
        {
            _releasePoint = transform.position;
            _lastPosition = transform.position - transform.forward;
            if (circleEnabled)
            {
                warningSprite = Instantiate(warningSpritePrefab);
                warningSprite.transform.position = _destination + new Vector3(0, 0.2f, 0);
                warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 0);
            }
        }

        public void Update()
        {
            if (Mathf.Approximately(_startOfLife, -1)) return;

            var t = (Time.time - _startOfLife) / (_arrivalTime - _startOfLife);
            if (t >= 1)
            {
                meshRenderer.enabled = false;
                if (spawnParticlesOnHit && !particlesSpawned)
                {
                    if (damageWallPieceOnHit) EventManager.RaiseOnWallPieceHit(wallPieceToHit);
                    GameObject particles = Instantiate(onHitParticles, _destination, Quaternion.identity);
                    particlesSpawned = true;
                }
                Destroy(gameObject);
            }
            else
            {
                transform.position = Vector3.Lerp(_releasePoint, _destination, t) +
                                     new Vector3(0, settings.parabolaHeight * (-Mathf.Pow(2 * t - 1, 2) + 1), 0);
                if (settings.rotationSpeed != 0) transform.LookAt(transform.position + (transform.position - _lastPosition));
            }
            if (circleEnabled)
            {
                if (t < 1) { 
                    warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, t);
                } else
                {
                    warningSprite.color = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 0);
                }
            }

            transform.Rotate(settings.rotationSpeed * Time.deltaTime, 0, 0);

            _lastPosition = transform.position;
        }

        public void SetWallPieceIndex(int index)
        {
            wallPieceToHit = index;
        }

        public void SetDestination(Vector3 position)
        {
            _destination = position;
        }

        private void StartFlightTimer()
        {
            _startOfLife = Time.time;
            _arrivalTime = _startOfLife + settings.flightTime;
        }

        public void AddDelay(float time)
        {
            _startOfLife += time;
            _arrivalTime += time;
        }

        public void SetUp(ProjectileSettings newSettings)
        {
            this.settings = newSettings;
            StartFlightTimer();
        }
    }

    [Serializable]
    public class ProjectileSettings
    {
        public ProjectileType type;

        public float flightTime;
        public float parabolaHeight;
        public float rotationSpeed;
    }

    public enum ProjectileType
    {
        Stone = 0,
        TarBarrel = 1,

        FireArrow = 10,

        CrossbowBolt = 20
    }
}