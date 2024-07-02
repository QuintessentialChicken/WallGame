﻿using System.Collections;
using AnimationControllers;
using Input;
using Interaction;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float sprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.1f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;

        [Tooltip("Useful for rough ground")] public float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;

      [Header("Inventory")] public Transform backpackWood;

        public Transform backpackStone;

        public Transform woodReplenisher;
        public Transform stoneReplenisher;

        public int stone = 1;

        public int stoneCapacity;
        //public List<GameObject> diegeticStones;

        public int wood = 1;
        public int woodCapacity;
        public float launchDuration = 1.0f;
        private readonly float _terminalVelocity = 53.0f;
        private InputActionMap _actionMapCatapult;
        private InputActionMap _actionMapNormal;
        private float _airTime;
        private float _animationBlend;

        private PlayerAnimationController _animController;
        private float _cinemachineTargetPitch;

        //public List<GameObject> diegeticWood;

        // cinemachine
        private float _cinemachineTargetYaw;
        private CharacterController _controller;

        private PlayerState _currentState = PlayerState.Normal;
        private float _fallTimeoutDelta;
        private bool _hasAnimController;
        private bool _hasCamera;

        private Inputs _input;
        // inventory


        // timeout
        private float _jumpTimeoutDelta;
        private Vector3[] _launchPath;
        private int _launchPathLength;

        private float _launchTime;

        private GameObject _mainCamera;
        private bool _midAir;

        private PlayerInput _playerInput;
        private float _rotationVelocity;

        // player
        private float _speed;


        private readonly GUIStyle _style = new();
        private float _targetRotation;
        private float _verticalVelocity;

        public bool inCatapult = false;

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            EventManager.OnEnterCatapult += EnterCatapult;
            EventManager.OnExitCatapult += ExitCatapult;
            EventManager.OnPlayerStunned += PauseMovement;
            EventManager.OnCatapultFire += GetLaunched;
            EventManager.OnReplenishWood += FillWood;
            EventManager.OnReplenishStone += FillStone;
            EventManager.OnRepairedWood += HandleRepairedWood;
            EventManager.OnRepairedStone += HandleRepairedStone;

            Inputs.Jump += Jump;
        }

        private void Start()
        {
            _animController = GetComponentInChildren<PlayerAnimationController>();
            _hasAnimController = _animController != null;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<Inputs>();
            _playerInput = GetComponent<PlayerInput>();
            _actionMapNormal = _playerInput.actions.FindActionMap("Normal", true);
            _actionMapCatapult = _playerInput.actions.FindActionMap("Catapult", true);

            AssignActionMapIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        private void Update()
        {
            if (_currentState == PlayerState.Launched)
            {
                Fly();
                return;
            }

            if (_currentState != PlayerState.Normal) return;
            GroundedCheck();
            Falling();
            Move();
        }

        private void OnDestroy()
        {
            EventManager.OnEnterCatapult -= EnterCatapult;
            EventManager.OnExitCatapult -= ExitCatapult;
            EventManager.OnPlayerStunned -= PauseMovement;
            EventManager.OnCatapultFire -= GetLaunched;
            EventManager.OnReplenishWood -= FillWood;
            EventManager.OnReplenishStone -= FillStone;
            EventManager.OnRepairedWood -= HandleRepairedWood;
            EventManager.OnRepairedStone -= HandleRepairedStone;
            Inputs.Jump -= Jump;
        }

        private void OnDrawGizmos()
        {
            _style.fontSize = 32;
            // if (chosenOne) Handles.Label(transform.position + new Vector3(0, 3, 0), "Wall Health: " + health, _style);
            Handles.Label(transform.position + new Vector3(0, 5, 0), "Wood: " + wood + "\nStone: " + stone, _style);
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }

        private void AssignActionMapIDs()
        {
            // TODO
        }


        private void GroundedCheck()
        {
            // set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
                transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);
            
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = _input.sprint ? sprintSpeed : moveSpeed;
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            var speedOffset = 0.1f;
            var inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            var inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            var targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimController)
                // animator.SetFloat(_animIDSpeed, _animationBlend);
                // animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                _animController.SetSpeed(math.remap(0, 4, 0, 1, _speed));
        }

        private void Falling()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // update animator if using character
                if (_midAir)
                {
                    _midAir = false;

                    if (_hasAnimController)
                    {
                        // animator.SetTrigger(_animIDJump);
                        // _animator.SetBool(_animIDFreeFall, false);
                        _animController.SetFalling(false);
                        _animController.SetAirTime(_airTime);
                        // PauseMovement(_airTime < 1.2 ? 0.0f : 1f);
                    }

                    // stop our velocity dropping infinitely when grounded
                    if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
                    _airTime = 0;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (!_midAir) _midAir = true;
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;
                _airTime += Time.deltaTime;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimController)
                        // _animator.SetBool(_animIDFreeFall, true);
                        _animController.SetFalling(true);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += gravity * Time.deltaTime;
        }

        private void Jump()
        {
            if (_currentState == PlayerState.Normal && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // update animator if using character
                if (_hasAnimController) _animController.SetJump();
            }
        }

        private void EnterCatapult(Transform catapultBowl)
        {
            inCatapult = true;
            CancelVelocity();
            //Switch to Action map Catapult
            // _controller.enabled = false;
            _actionMapNormal.Disable();
            _actionMapCatapult.Enable();
            _currentState = PlayerState.InCatapult;
            transform.position = catapultBowl.position;
            transform.parent = catapultBowl;
            transform.rotation = catapultBowl.parent.rotation;
            _animController.EnterCatap();
        }

        private void ExitCatapult(Transform dropOffPoint)
        {
            CancelVelocity();
            //Switch to action map Normal
            _actionMapNormal.Enable();
            _actionMapCatapult.Disable();
            _currentState = PlayerState.Normal;
            transform.position = dropOffPoint.position;
            transform.parent = null;
            // _controller.enabled = true;
        }

        private void GetLaunched(Vector3[] path, int vertexCount)
        {
            CancelVelocity();
            EventManager.RaisePlayerStunned(launchDuration);
            //Switch to action map Normal
            _launchTime = Time.time;
            _actionMapNormal.Enable();
            _actionMapCatapult.Disable();
            _currentState = PlayerState.Launched;
            _launchPath = path;
            _launchPathLength = vertexCount;
            Invoke(nameof(DoneLaunching), launchDuration);
            transform.parent = null;
            // _controller.enabled = true;
            _animController.Launching(true);
        }

        private void Fly()
        {
            var currentProgress = (Time.time - _launchTime) / launchDuration;
            //The Mid-Air Movement is divided into y and xz because in a normal flying object, horizontal and vertical speed are also disconnected.

            var horiz = Vector3.Lerp(_launchPath[0], _launchPath[_launchPathLength - 1], currentProgress);

            var lastPoint =
                _launchPath[
                    Mathf.FloorToInt(Mathf.Clamp(currentProgress * _launchPathLength, 0, _launchPathLength - 1))];
            var nextPoint =
                _launchPath[
                    Mathf.CeilToInt(Mathf.Clamp(currentProgress * _launchPathLength, 0, _launchPathLength - 1))];
            var uber = currentProgress * _launchPathLength - Mathf.FloorToInt(currentProgress * _launchPathLength);
            transform.position = new Vector3(horiz.x, Mathf.Lerp(lastPoint.y, nextPoint.y, uber), horiz.z);
        }

        private void DoneLaunching()
        {
            _currentState = PlayerState.Normal;
            _controller.SimpleMove(_launchPath[_launchPathLength - 1] - _launchPath[_launchPathLength - 2]);
            _animController.Launching(false);
            inCatapult = false;
        }

        /// <summary>
        ///     Cancels players velocity and suspends movement for duration
        /// </summary>
        /// <param name="duration">Time in Seconds</param>
        private void PauseMovement(float duration)
        {
            CancelVelocity();
            print(this);
            StartCoroutine(StunCoroutine(duration));
        }

        private void CancelVelocity()
        {
            _controller.SimpleMove(Vector3.zero);
            _speed = 0;
            _animController.SetSpeed(0);
        }

        private IEnumerator StunCoroutine(float duration)
        {
            _currentState = PlayerState.Stunned;
            yield return new WaitForSeconds(duration);
            _currentState = PlayerState.Normal;
        }

        private void FillWood(int amount = 5)
        {
            amount = Mathf.Min(woodCapacity, wood + amount);
            wood = amount;

            for (int i = 0; i < woodCapacity; i++)
            {
                backpackWood.GetChild(i).gameObject.SetActive(i < wood);
            }
        }

        private void FillStone(int amount = 5)
        {
            amount = Mathf.Min(stoneCapacity, stone + amount);
            stone = amount;
            for (int i = 0; i < stoneCapacity; i++)
            {
                backpackStone.GetChild(i).gameObject.SetActive(i < stone);
            }
            /* Leon's Implementation
            while (stoneReplenisher.childCount > 0 && amount > 0)
            {
                transform.parent = backpackStone;
                var child = stoneReplenisher.GetChild(stoneReplenisher.childCount - 1);
                child.SetParent(backpackStone, false);
                child.localRotation = Quaternion.Euler(0, 0, 15);
                child.localPosition =
                    Vector3.zero + new Vector3(-0.13f + Random.Range(-0.05f, 0.05f), 0, 0.16f * amount);
                --amount;
            }*/
        }

        private void HandleRepairedWood()
        {
            wood--;
            for (int i = 0; i < woodCapacity; i++)
            {
                backpackWood.GetChild(i).gameObject.SetActive(i < wood);
            }
            
            /*
            var toRemove = backpackWood.GetChild(0);
            toRemove.SetParent(woodReplenisher, false);
            toRemove.localRotation = Quaternion.identity;
            toRemove.localPosition = Vector3.zero + new Vector3(-0.8f, 0.07f * woodReplenisher.transform.childCount, 0);
            wood--;*/
        }

        

        private void HandleRepairedStone()
        {
            stone--;
            for (int i = 0; i < stoneCapacity; i++)
            {
                backpackStone.GetChild(i).gameObject.SetActive(i < stone);
            }
            /*var toRemove = backpackStone.GetChild(0);
            toRemove.SetParent(stoneReplenisher, false);
            toRemove.localRotation = Quaternion.Euler(0, Random.Range(-5, 5), 0);
            toRemove.localPosition = Vector3.zero + new Vector3(2.7f, 0.55f, Random.Range(-0.6f, 0.6f));
            stone--;*/
        }

        public bool CanRepairWood()
        {
            return wood > 0;
        }

        public bool CanRepairStone()
        {
            return stone > 0;
        }

        private enum PlayerState
        {
            Normal,
            InCatapult,
            Launched,
            Stunned
        }
    }
}