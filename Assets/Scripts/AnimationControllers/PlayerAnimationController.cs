using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace AnimationControllers
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        public bool DEBUG_Falling = false;
        [Range(0.0f, 1.0f)] public float DEBUG_Speed = 0.0f;
        public bool DEBUG_Grab = false;
        public bool DEBUG_Repair = false;

        [Header("_____ Transforms ")]
        public Transform hammer;
        public Transform hipHolster;
        public Transform hand;

        [Header("_____ Inverse Kinematics ")]
        [Header("__________ General ")]
        public Transform leftHandIK;
        public TwoBoneIKConstraint ikConstraint;
        [Header("__________ Grabbing ")]
        public float grabRadius = 0.25f;
        public float grabSpeed = 2.0f;
        public float grabForward = 2.0f;
        [Header("__________ Repair ")]
        public float repairRadius = 0.25f;
        public float repairSpeed = 2.0f;
        public float repairForward = 2.0f;

        public bool falling { private set; get; }


        //private float _airTime;
        private int _animIDLanding;
        private int _animIDFalling;
        private int _animIDJump;
        private int _animIDSpeed;
        private int _animIDEnterCatapult;
        private Animator _anim;

        public bool Locked { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            _anim = GetComponent<Animator>();
            AssignAnimationIDs();
        }

        // Update is called once per frame
        private void Update()
        {
            
#if UNITY_EDITOR
            if (DEBUG_Falling ^ falling)
            {
                SetFalling(DEBUG_Falling);
            }
            SetSpeed(DEBUG_Speed);
            if (DEBUG_Grab)
            {
                GrabResource();
                DEBUG_Grab = false;
            }
            if (DEBUG_Repair)
            {
                Repair();
                DEBUG_Repair = false;
            }
#endif
        }

        private void AssignAnimationIDs()
        {
            _animIDJump = Animator.StringToHash("Jump");
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDFalling = Animator.StringToHash("Falling");
            _animIDEnterCatapult = Animator.StringToHash("EnterCatapult");
            _animIDLanding = Animator.StringToHash("Landing");
        }



        public void Launching(bool launching)
        {
            _anim.SetTrigger(launching ? "Launching" : "DoneLaunching");
        }

        public void EnterCatap()
        {
            _anim.SetTrigger(_animIDEnterCatapult);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.mountCatapult, this.transform.position);
        }

        private void AnimEvent_DoneAnimating()
        {
            Locked = false;
        }

        public void SetJump()
        {
            _anim.SetTrigger(_animIDJump);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.walltherGrunt, this.transform.position);
        }

        public void SetSpeed(float value)
        {
            _anim.SetFloat(_animIDSpeed, value);
            DEBUG_Speed = value;
        }

        public void SetFalling(bool value)
        {
            falling = value;
            _anim.SetBool(_animIDFalling, value);
            if (!value)
            {
                StartCoroutine(nameof(FallingCoroutine));
            }
            DEBUG_Falling = value;
        }

        public void GrabResource()
        {
            StopCoroutine(nameof(GrabResourceCoroutine));
            hammer.parent = hipHolster;
            hammer.transform.localPosition = Vector3.zero;
            hammer.transform.localRotation = Quaternion.identity;
            StartCoroutine(nameof(GrabResourceCoroutine));
        }

        private IEnumerator GrabResourceCoroutine()
        {
            for (float t = 0; t < 2 * 3.142f; t += Time.deltaTime * grabSpeed)
            {
                if (t <= 1.1f) ikConstraint.weight = Mathf.Clamp(t, 0, 1);
                if (t >= 5.2f) ikConstraint.weight = Mathf.Clamp((6.3f - t), 0, 1);
                leftHandIK.transform.localPosition = new Vector3(grabRadius * Mathf.Sin(-t), 0, grabForward + grabRadius * Mathf.Cos(t + 3.152f));
                leftHandIK.transform.LookAt(leftHandIK.transform.parent, new Vector3(0, 1, 0));
                leftHandIK.transform.Rotate(0, -90, 0);
                leftHandIK.transform.Rotate(0, 0, 90);
                yield return null;
            }
            ikConstraint.weight = 0;
            yield return null;
        }

        public void Repair()
        {
            StopCoroutine(nameof(RepairCoroutine));
            hammer.parent = hand;
            hammer.transform.localPosition = Vector3.zero;
            hammer.transform.localRotation = Quaternion.identity;
            StartCoroutine(nameof(RepairCoroutine));
        }

        private IEnumerator RepairCoroutine()
        {
            for (float t = 0; t < 1.5f * 3.142f; t += Time.deltaTime * repairSpeed + t/20)
            {
                if (t <= 1.1f) ikConstraint.weight = Mathf.Clamp(t, 0, 1);
                if (t >= 5.2f) ikConstraint.weight = Mathf.Clamp((6.3f - t), 0, 1);
                leftHandIK.transform.localPosition = new Vector3(0, -1.5f*repairRadius * Mathf.Sin(-t), repairForward + repairRadius * Mathf.Cos(t + 3.152f));
                leftHandIK.transform.LookAt(leftHandIK.transform.parent, new Vector3(0, 1, 0));
                leftHandIK.transform.Rotate(0, -90, 0);
                leftHandIK.transform.Rotate(0, 0, 90);
                yield return null;
            }
            for (float t = 0; t < 0.2f; t+= Time.deltaTime)
            {
                yield return null;
            }
            for (float t = 1; t > 0.0f; t -= Time.deltaTime*3)
            {
                ikConstraint.weight = Mathf.Clamp(t, 0, 1);
                yield return null;
            }
            ikConstraint.weight = 0;
            yield return null;
        }

        private IEnumerator FallingCoroutine()
        {
            for (float t = 0.1f; t < 1; t += Time.deltaTime/t)
            {
                _anim.SetFloat(_animIDLanding, (1-Mathf.Cos(2*t*3.14f))/2);
                yield return null;
            }
            _anim.SetFloat(_animIDLanding, 0);
            yield return null;
        }
    }
}