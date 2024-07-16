using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wall;
using Random = UnityEngine.Random;
using FMODUnity;
using Player;

namespace Enemies
{
    public class ArmyController : MonoBehaviour
    {
        public static ArmyController instance;
        public int enemyCount = 100;

        // Start is called before the first frame update
        [Header("_______________ Gameplay Relevant _______________")] [SerializeField]
        
        public int day = 1;
        public DifficultySettings currentDifficulty;
        [Space]
        public DifficultySettings difficultyDay1;
        public DifficultySettings difficultyDay7;
        public int tarBarrelsFromDay = 2;
        public int flourBarrelsFromDay = 3;
        

        [SerializeField]
        [Tooltip("RANDOM\nThe Columns are picket randomly\n\n" +
                 "SUCCESSION\nThe Columns are picket left to right\n\n" +
                 "RANDOMEQUAL\nThe Columns are picket randomly and all wall columns are hit equally often\n\n" +
                 "RANDOMEQUAL_ANIM\nThe Columns are picket randomly and all wall columns are hit equally often. Trebuchets that have finished their reload animation are preferred in selection.\n\n" +
                 "RANDOMEQUAL_SWITCH\nThe Columns are picket randomly and all wall columns are hit equally often. No Trebuchet fires twice in a row.")]
        private TargetingScheme targetingScheme = TargetingScheme.Random;

        [Header("_______________ Projectiles _______________")]
        [Header(" ___ Trebuchet Stones ___")]
        public TargetProjectile trebuchetStonePrefab;
        public ProjectileSettings trebuchetStoneSettings;
        [Header(" ___ Barrels ___ ")]
        public TargetProjectile tarBarrelPrefab;
        public TargetProjectile flourBarrelPrefab;
        public ProjectileSettings barrelSettings;
        
        public Vector3 barrelArea;
        [Header(" ___ Fire Arrows ___ ")]
        public TargetProjectile fireArrowPrefab;
        public ProjectileSettings fireArrowSettings;

        [Space]
        [Header("_______________ Look & Spawn Areas _______________")]

        [SerializeField]
        private TrebuchetSettings trebuchets;
        [SerializeField]
        private FootsoldierSettings footsoldiers;

        [Space]

        [SerializeField]
        private BowmenSettings bowmen;

        [Header("_______________ Debug Area _______________")]
        public bool consoleOutput = true;

         [Space] public bool debugFireArrows;

         public bool debugTrebuchetLaunch;
         public bool debugKillTrebuchet;
         public bool debugLaunchTarBarrel;

        public List<bool> columnsHit;
        private Transform _bowmenParent;

        [SerializeField]
        private int lastWallIndexHit = -1;

        private Transform _footsoldierParent;
        private Transform _graveyardParent;
        private Transform _trebuchetParent;
        private List<int> _arrowTargets;
        private int _columns;

        private int _footsoldiersForefeit;

        private int _lastCount;
        private Vector2 _lastLowerLeft = Vector2.zero;

        private int _lastTrebuchet = -1;
        private Vector2 _lastUpperRight = Vector2.zero;

        private int _trebuchetCount = 5;

        private List<float> _trebuchetPositions; // vertical positions [0..1] relative to avaliable space

        private List<Trebuchet> _trebuchets;

        private WallManager _wallManager;

        private void Awake()
        {
            instance = this;
        }

        public void OnValidate()
        {
            currentDifficulty = GetDifficulty(day);
        }

        private void Start()
        {
            _columns = WallManager.instance.wallColumns;
            /*if (_trebuchets.projectile.flightTime >= _trebuchetCooldown)
            {
                Debug.LogError("trebuchet projectile flight time should never be lower than trebuchet cooldown! flight time will now be set to " + (_trebuchetCooldown + 0.1f));
                _trebuchets.projectile.flightTime = _trebuchetCooldown + 0.1f;
            }*/
            currentDifficulty = GetDifficulty(day);
            CreateEmptyParents();
            SpawnArmy();
            SpawnTrebuchets();
            SpawnBowmen();

            Invoke(nameof(LaunchTrebuchet), currentDifficulty.trebuchetCooldownOffset);
            if (currentDifficulty.fireArrowsCooldown != 0) Invoke(nameof(LaunchFireArrows), currentDifficulty.fireArrowsCooldownOffset);
            //if (currentDifficulty.flourBarrelCooldown != 0) Invoke(nameof(LaunchFlourBarrel), currentDifficulty.flourBarrelCooldownOffset);
            //if (currentDifficulty.tarBarrelCooldown != 0) Invoke(nameof(LaunchTarBarrel), currentDifficulty.tarBarrelCooldownOffset);
            StartCoroutine(nameof(BarrelCoroutine));

            _wallManager = FindObjectOfType<WallManager>();
            columnsHit = new List<bool>();
            _columns = WallManager.instance.wallColumns;
            for (var i = 0; i < _trebuchetCount; i++) columnsHit.Add(false);

            RatingSystem.Instance.SetRatingTime();
        }

        private void Update()
        {
            if (debugFireArrows)
            {
                LaunchFireArrows();
                debugFireArrows = false;
            }

            if (debugTrebuchetLaunch)
            {
                LaunchTrebuchet();
                debugTrebuchetLaunch = false;
            }

            if (debugKillTrebuchet)
            {
                KillTrebuchet();
                debugKillTrebuchet = false;
            }

            if (debugLaunchTarBarrel)
            {
                LaunchTarBarrel();
                debugLaunchTarBarrel = false;
            }
        }

        

        private bool SomethingChanged()
        {
            return _lastCount != _trebuchetCount || _lastLowerLeft != trebuchets.spawnAreaLowerLeft ||
                   _lastUpperRight != trebuchets.spawnAreaUpperRight;
        }

        private void CreateEmptyParents()
        {
            _footsoldierParent = new GameObject("Footsoldier Parent").transform;
            _footsoldierParent.parent = transform;
            _trebuchetParent = new GameObject("Trebuchet Parent").transform;
            _trebuchetParent.parent = transform;
            _bowmenParent = new GameObject("Bowmen Parent").transform;
            _bowmenParent.parent = transform;

            _graveyardParent = new GameObject("Graveyard").transform;
            _graveyardParent.parent = transform;
        }
        
        private void LaunchTarBarrel()
        {
            float worldX = Random.Range(-barrelArea.x/2, barrelArea.x/2);

            // Select Trebuchet
            int trebuchetIndex = Mathf.FloorToInt(((worldX + barrelArea.x) / (barrelArea.x*2)) * WallManager.instance.wallColumns);

            // Launch
            Vector3 target = new(worldX, 0, barrelArea.z);
            _trebuchets[trebuchetIndex].RequestLaunch(ProjectileType.TarBarrel, target);

            // Restart cooldown
            //Invoke(nameof(LaunchTarBarrel), currentDifficulty.barrelCooldown);
            StartCoroutine(InvokeAfterDelay(3, LaunchTrebuchetSound, target));
            StartCoroutine(InvokeAfterDelay(3.8f, LaunchBarrelBreakingSound, target));
        }

        private void LaunchFlourBarrel()
        {
            float worldX = Random.Range(-barrelArea.x / 2, barrelArea.x / 2);

            // Select Trebuchet
            int trebuchetIndex = Mathf.FloorToInt(((worldX + barrelArea.x) / (barrelArea.x * 2)) * WallManager.instance.wallColumns);

            // Launch
            Vector3 target = new(worldX, 0, barrelArea.z);
            _trebuchets[trebuchetIndex].RequestLaunch(ProjectileType.FlourBarrel, target);

            // Restart cooldown
            //Invoke(nameof(LaunchFlourBarrel), currentDifficulty.flourBarrelCooldown);
            StartCoroutine(InvokeAfterDelay(3, LaunchTrebuchetSound, target));
            StartCoroutine(InvokeAfterDelay(3.8f, LaunchBarrelBreakingSound, target));
        }

        private void LaunchTrebuchet()
        {
            if (targetingScheme == TargetingScheme.HoldFire)
            {
                Invoke(nameof(LaunchTrebuchet), currentDifficulty.trebuchetCooldown);
                return;
            }

            int chosenWallIndex = -1;
            List<WallSegment> segments = WallManager.instance.GetWallSegments();

            if (targetingScheme == TargetingScheme.Succession)
            {
                chosenWallIndex = (lastWallIndexHit + 1) % segments.Count; 
            }
            else
            {   // Picking a random Wall segment with varying probabilities
                float sum = segments.Sum(x => x.probabilityModifier);
                if (targetingScheme == TargetingScheme.Random_NoWallTwice && lastWallIndexHit != -1) sum -= segments.ElementAt(lastWallIndexHit).probabilityModifier;
                List<float> normalizedProbabilities = segments.ConvertAll<float>(x => x.probabilityModifier / sum);
                if (targetingScheme == TargetingScheme.Random_NoWallTwice && lastWallIndexHit != -1) normalizedProbabilities[lastWallIndexHit] = 0;


                float choice = Random.Range(0.0f, 1.0f);

                float walkingSum = 0;

                for (int i = 0; i < normalizedProbabilities.Count; i++)
                {
                    walkingSum += normalizedProbabilities.ElementAt(i);
                    if (walkingSum >= choice)
                    {
                        chosenWallIndex = i;
                        break;
                    }
                }
            }
            lastWallIndexHit = chosenWallIndex;

            // Picking correct trebuchet
            int trebuchetIndex = chosenWallIndex % WallManager.instance.wallColumns;

            _trebuchets[trebuchetIndex].RequestLaunch(ProjectileType.Stone, segments[chosenWallIndex].transform.position, chosenWallIndex);

            // Restart cooldown
            StartCoroutine(InvokeAfterDelay(2, LaunchTrebuchetSound, segments[chosenWallIndex].transform.position));
            Invoke(nameof(LaunchTrebuchet), currentDifficulty.trebuchetCooldown);
        }

        private void LaunchTrebuchetSound(Vector3 destination)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shootTrebuchet, destination);
        }

        public void LaunchFireArrows()
        {
            if (targetingScheme == TargetingScheme.HoldFire)
            {
                Invoke(nameof(LaunchFireArrows), currentDifficulty.fireArrowsCooldown);
                return;
            }
            var parts = bowmen.count / _columns;
            var remainder = bowmen.count % _columns;
            var horizSpacing = (bowmen.spawnAreaUpperRight.x - bowmen.spawnAreaLowerLeft.x) / bowmen.count;
            for (var i = 0; i < _columns; i++)
            {
                
                if (i == _columns - 1) parts += remainder;
                if (Random.Range(0f, 1f) > currentDifficulty.fireArrowsDestruction) continue;
                for (var j = 0; j < parts; j++)
                {
                    var worldStart = transform.position
                                     //+ new Vector3(_bowmen.spawnAreaLowerLeft.x + i * horizSpacing, 0, _bowmen.spawnAreaUpperRight.y)
                                     + new Vector3(
                                         Random.Range(bowmen.spawnAreaLowerLeft.x, bowmen.spawnAreaUpperRight.x), 0,
                                         Random.Range(-0.5f, 0.5f));
                    var fireArrow = Instantiate(fireArrowPrefab, worldStart, Quaternion.identity, _bowmenParent);
                    fireArrow.SetDestination(_wallManager.GetWallSegmentPosition(i) +
                                             new Vector3(Random.Range(-0.6f, 0.6f), 0.7f, Random.Range(-0.3f, 0.3f)));
                    
                    fireArrow.SetUp(fireArrowSettings);
                    fireArrow.AddDelay(Random.Range(0, 0.5f));
                    StartCoroutine(InvokeAfterDelay(1.8f, LaunchFireArrowsSound, fireArrow.GetDestination()));
                }
                StartCoroutine(InvokeAfterDelay(3, EventManager.RaiseOnScaffoldingHit, i));
            }

            Invoke(nameof(LaunchFireArrows), currentDifficulty.fireArrowsCooldown);
        }

        private void LaunchFireArrowsSound(Vector3 arrowDestination)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyArrowWhoosh, arrowDestination);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.burnScaffolding, arrowDestination);

        }

        private void LaunchBarrelBreakingSound(Vector3 arrowDestination)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.barrelBreaking, arrowDestination);
        }

        public void SetTargetingScheme(TargetingScheme scheme)
        {
            targetingScheme = scheme;
        }

        private static IEnumerator InvokeAfterDelay<T>(float delay, Action<T> method, T parameter)
        {
            yield return new WaitForSeconds(delay);
            method(parameter);
        }

        public Vector3 GetFootsoldierPosition()
        {
            if (_footsoldiersForefeit < _footsoldierParent.childCount)
                return _footsoldierParent.GetChild(_footsoldiersForefeit++).transform.position + new Vector3(0, 0.6f, 0);
            Debug.LogWarning(
                "More footsoldiers forefeit than left. returning dummy value. If this was close to winning the game you can ignore this warning.");
            return new Vector3(0, 0, 50);

        }

        public void BoltArrives()
        {
            Kill(1);
        }

        public void BombArrives()
        {
            Kill(5);
        }


        private bool lastBarrelTar = false;
        private IEnumerator BarrelCoroutine()
        {
            while (enemyCount > 5)
            {
                yield return new WaitForSeconds(currentDifficulty.barrelCooldown);
                lastBarrelTar = !lastBarrelTar;
                if (lastBarrelTar && day > flourBarrelsFromDay) LaunchFlourBarrel();
                else if (day > tarBarrelsFromDay) LaunchTarBarrel();
            }
        }

        private void SpawnArmy()
        {
            enemyCount = currentDifficulty.enemySoldiers;
            float vertDistance = (footsoldiers.spawnAreaUpperRight.y - footsoldiers.spawnAreaLowerLeft.y) / enemyCount;
            for (var i = 0; i < enemyCount; i++)
            {
                var soldier =
                    Instantiate(footsoldiers.prefab,
                        transform.position + new Vector3(
                            Random.Range(footsoldiers.spawnAreaLowerLeft.x, footsoldiers.spawnAreaUpperRight.x),
                            0,
                            footsoldiers.spawnAreaLowerLeft.y + i*vertDistance),
                        Quaternion.identity,
                        _footsoldierParent);
                soldier.SetUp(footsoldiers.vibing, footsoldiers.ecstasy, footsoldiers.hatred);

                if (i % 10 == 0)
                {
                    GameObject fahne = Instantiate(footsoldiers.flagge, soldier.transform.position, Quaternion.Euler(0, 64, 0), soldier.transform);
                }
            }
        }

        private void SpawnTrebuchets()
        {
            AssignNewTrebuchetPositions();
            _trebuchets = new List<Trebuchet>();
            var horizSpacing = (trebuchets.spawnAreaUpperRight.x - trebuchets.spawnAreaLowerLeft.x) /
                               Mathf.Max(1, _trebuchetCount - 1);
            var vertSpace = trebuchets.spawnAreaUpperRight.y - trebuchets.spawnAreaLowerLeft.y;
            for (var i = 0; i < _trebuchetCount; i++)
            {
                var trebuchet = Instantiate(trebuchets.prefab, transform.position
                                                                + new Vector3(trebuchets.spawnAreaLowerLeft.x, 0,
                                                                    trebuchets.spawnAreaLowerLeft.y)
                                                                + new Vector3(i * horizSpacing, 0,
                                                                    _trebuchetPositions[i] * vertSpace),
                    Quaternion.identity, _trebuchetParent);
                trebuchet.transform.LookAt(Vector3.zero);
                trebuchet.transform.Rotate(0,180,0);
                trebuchet.SetReloadSpeed(trebuchets.reloadSpeed);
                _trebuchets.Add(trebuchet);
            }
        }

        private void SpawnBowmen()
        {
            for (var i = 0; i < bowmen.count; i++)
            {
                var bowman = Instantiate(bowmen.prefab,
                    transform.position +
                    new Vector3(Random.Range(bowmen.spawnAreaLowerLeft.x, bowmen.spawnAreaUpperRight.x), 0,
                        Random.Range(bowmen.spawnAreaLowerLeft.y, bowmen.spawnAreaUpperRight.y)), Quaternion.identity,
                    _bowmenParent);
            }
        }

        private void AssignNewTrebuchetPositions()
        {
            _trebuchetCount = FindObjectOfType<WallManager>().wallColumns;
            _trebuchetPositions = new List<float>();
            for (var i = 0; i < _trebuchetCount; i++)
                _trebuchetPositions.Add(Random.Range(0.0f, 1.0f));
        }

        private void Kill(int count)
        {
            if (enemyCount <= 0) return;
            for (var i = 0; i < count; i++)
            {
                var victim = _footsoldierParent.GetChild(0);
                victim.SetParent(_graveyardParent);
                victim.GetComponent<Footsoldier>().StartCoroutine(nameof(Footsoldier.Die));
                enemyCount--;
                _footsoldiersForefeit = Mathf.Max(_footsoldiersForefeit - 1, 0);
                if (enemyCount <= 0)
                {
                    DayNightManager.instance.RequestChangeTo(DayNightManager.TimeOfDay.Evening_Upgrades);
                    RatingSystem.Instance.SetEndTime();
                }
            }
        }

        private DifficultySettings GetDifficulty(int day)
        {
            float lerpValue = (day - 1.0f) / 6.0f;
            return new DifficultySettings(
                difficultyDay1.enemySoldiers + (day-1) * 10,
                Mathf.LerpUnclamped(difficultyDay1.trebuchetCooldown, difficultyDay7.trebuchetCooldown, lerpValue),
                Mathf.LerpUnclamped(difficultyDay1.fireArrowsCooldown, difficultyDay7.fireArrowsCooldown, lerpValue),
                Mathf.LerpUnclamped(difficultyDay1.fireArrowsDestruction, difficultyDay7.fireArrowsDestruction, lerpValue),
                day <= tarBarrelsFromDay? 0 : Mathf.LerpUnclamped(difficultyDay1.barrelCooldown, difficultyDay7.barrelCooldown, lerpValue)
                );
        }

        private void KillTrebuchet()
        {
            _trebuchets[_trebuchetCount - 1].Kill();
            _trebuchets[_trebuchetCount - 1].transform.SetParent(_graveyardParent);
            _trebuchets.RemoveAt(_trebuchetCount - 1);
            _trebuchetCount--;
            var newRowsHit = new List<bool>();
            for (var i = 0; i < _trebuchetCount; i++) newRowsHit.Add(columnsHit[i]);
            columnsHit = newRowsHit;
        }

        private void GizmoDrawTargetArea(Color color, Vector2 lowerLeft, Vector2 upperRight, bool localPos = true)
        {
            Gizmos.color = color;
            Gizmos.DrawLine((localPos ? transform.position : Vector3.zero) +
                new Vector3(lowerLeft.x, 0, lowerLeft.y),
                (localPos ? transform.position : Vector3.zero) + new Vector3(lowerLeft.x, 0, upperRight.y));
            Gizmos.DrawLine((localPos ? transform.position : Vector3.zero) +
                new Vector3(lowerLeft.x, 0, lowerLeft.y),
                (localPos ? transform.position : Vector3.zero) + new Vector3(upperRight.x, 0,lowerLeft.y));
            Gizmos.DrawLine((localPos ? transform.position : Vector3.zero) +
                new Vector3(upperRight.x, 0, upperRight.y),
                (localPos ? transform.position : Vector3.zero) + new Vector3(lowerLeft.x, 0, upperRight.y));
            Gizmos.DrawLine((localPos ? transform.position : Vector3.zero) +
                new Vector3(upperRight.x, 0, upperRight.y),
                (localPos ? transform.position : Vector3.zero) + new Vector3(upperRight.x, 0, lowerLeft.y));
            Gizmos.DrawCube((localPos ? transform.position : Vector3.zero) +
                new Vector3(lowerLeft.x, 0, lowerLeft.y),
                new Vector3(1, 1, 1));
            Gizmos.DrawCube((localPos ? transform.position : Vector3.zero) +
                new Vector3(upperRight.x, 0, upperRight.y), 
                new Vector3(1, 1, 1));
        }

        public void OnDrawGizmosSelected()
        {
            // Drawing Footsodier Spawn Area
            GizmoDrawTargetArea(Color.green, footsoldiers.spawnAreaLowerLeft, footsoldiers.spawnAreaUpperRight);

            // Drawing Tar Target Area
            Gizmos.color = new Color(0, 0, 0, 0.5f);
            Gizmos.DrawCube(new Vector3(0, 0, barrelArea.z), new Vector3(barrelArea.x, 1, barrelArea.y));

            // Drawing Bowmen Spawn Area
            GizmoDrawTargetArea(Color.yellow, bowmen.spawnAreaLowerLeft, bowmen.spawnAreaUpperRight);

            // Drawing Trebuchet Spawn Area
            GizmoDrawTargetArea(Color.blue, trebuchets.spawnAreaLowerLeft, trebuchets.spawnAreaUpperRight);

            // Drawing Trebuchet Cubes
            var updatePositions = SomethingChanged();
            if (updatePositions)
            {
                _lastCount = _trebuchetCount;
                _lastLowerLeft = trebuchets.spawnAreaLowerLeft;
                _lastUpperRight = trebuchets.spawnAreaUpperRight;
                AssignNewTrebuchetPositions();
            }

            var horizSpacing = (trebuchets.spawnAreaUpperRight.x - trebuchets.spawnAreaLowerLeft.x) /
                               Mathf.Max(1, _trebuchetCount - 1);
            var vertSpace = trebuchets.spawnAreaUpperRight.y - trebuchets.spawnAreaLowerLeft.y;

            //if (!Application.IsPlaying(this))
            /*
            for (int i = 0; i < trebuchetCount; i++)
            {
                Gizmos.color = Color.blue;
                Vector3 trebuchetPos = transform.position
                    + new Vector3(_trebuchets.spawnAreaLowerLeft.x, 0, _trebuchets.spawnAreaLowerLeft.y)
                    + new Vector3(i * horizSpacing, 2.5f, trebuchetPositions[i] * vertSpace);

                Gizmos.DrawCube(trebuchetPos, new Vector3(1.5f, 5f, 2.6f));

                Vector3 parabolaPeak = trebuchetPos / 2 + new Vector3(0, _trebuchets.projectile.parabolaHeight + 6, 0);

                Gizmos.color = new Color(0, 0, 0.5f, 0.5f);
                Gizmos.DrawLine(trebuchetPos, parabolaPeak);
                Gizmos.DrawLine(parabolaPeak, new Vector3((i - trebuchetCount/2)*1.5f, 0, 0));
            }*/
        }
    }

    [Serializable]
    internal class EnemyTroop
    {
        public Vector2 spawnAreaLowerLeft;
        public Vector2 spawnAreaUpperRight;
    }

    [Serializable]
    internal class BowmenSettings : EnemyTroop
    {
        public GameObject prefab;
        public int count = 20;
    }

    [Serializable]
    internal class TrebuchetSettings : EnemyTroop
    {
        public Trebuchet prefab;
        public float reloadSpeed;
    }

    [Serializable]
    internal class FootsoldierSettings : EnemyTroop
    {
        public Footsoldier prefab;

        public GameObject flagge;

        //public Color flagColor = Color.white;

        [Tooltip("Defines the Oscillation height (Or footstep height) of the foot soldiers.")]
        public float vibing;

        [Tooltip("Defines the speed at which the foot soldiers oscillate")]
        public float ecstasy;

        [Tooltip("Defines how fast the footsoldiers approach the Wall.")]
        public float hatred;
    }

    
    public enum TargetingScheme
    {
        Random,
        /* The target wall gets picket randomly */

        Succession,
        /* The target wall gets picket left to right */

        Random_NoWallTwice,
        /* The target wall gets picket randomly, but no wall piece is targetet twice in a row. */

        HoldFire
    }

    [System.Serializable]
    public class DifficultySettings
    {
        public DifficultySettings(int _enemyCount, 
            float _trebuchetCooldown, 
            float _firearrowCooldown,
            float _fireArrowDestruction,
            float _barrelCooldown) { 
            enemySoldiers = _enemyCount;
            trebuchetCooldown = _trebuchetCooldown;
            trebuchetCooldownOffset = trebuchetCooldown;

            fireArrowsCooldown = _firearrowCooldown;
            fireArrowsDestruction = _fireArrowDestruction;
            fireArrowsCooldownOffset = _firearrowCooldown / 2;

            barrelCooldown = _barrelCooldown;
            barrelCooldownOffset = barrelCooldown / 2;
        }
        public int enemySoldiers;
        [Space]
        public float trebuchetCooldown;
        public float trebuchetCooldownOffset { get; private set; }
        [Space]
        public float fireArrowsCooldown;
        public float fireArrowsCooldownOffset { get; private set; }
        public float fireArrowsDestruction;
        [Space]
        public float barrelCooldown;
        public float barrelCooldownOffset { get; private set; }
        
     
    }
}