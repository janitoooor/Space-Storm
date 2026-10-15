using Game.Battle.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Battle.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterController : MonoBehaviour, BattleActions.IPlayerActions
    {
        private const float IdleLocomotion = 0f;
        private const float WalkLocomotion = 1f;
        private const float RunLocomotion = 2f;
        private const float FacingSide = 1f;
        private const float AimEpsilon = 0.0001f;
        private const string WeaponLayerName = "Weapon";

        private static readonly int speedAnimatorProperty = Animator.StringToHash("Speed");
        private static readonly int facingAnimatorProperty = Animator.StringToHash("Facing");
        private static readonly int gunAnimatorProperty = Animator.StringToHash("Gun");

        [SerializeField, Range(0, 10f)]
        private float moveSpeed = 5f;

        [SerializeField, Range(1, 3f)]
        private float sprintMultiplier = 1.5f;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private WeaponLoadout loadout;

        [SerializeField]
        private SuitAudio suitAudio;

        private Rigidbody2D rb;

        private Vector2 moveInput;
        private Vector2 lastMoveInputNormalized;

        private bool isSprinting;
        private bool attackHeld;
        private bool attackPulse;
        private int heardReloadStarted;
        private int heardReloadFinished;

        private float currentSpeed;
        private Vector2 aimDirection = Vector2.left;

        public Vector2 LookDirection => aimDirection;

        private Camera gameplayCamera;

        private BattleActions actions;
        private BattleActions.PlayerActions playerActions;

        private int weaponLayer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            actions = new BattleActions();
            playerActions = actions.Player;
            playerActions.AddCallbacks(this);

            weaponLayer = LayerMask.NameToLayer(WeaponLayerName);
            gameplayCamera = Camera.main;

            loadout.Hide();
        }

        private void OnDestroy()
            => actions.Dispose();

        private void OnEnable()
            => playerActions.Enable();

        private void OnDisable()
            => playerActions.Disable();

        public void OnMove(InputAction.CallbackContext context)
            => moveInput = context.ReadValue<Vector2>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != weaponLayer)
                return;

            ICollectableWeapon weapon;
            if (other.TryGetComponent(out weapon))
                CollectWeapon(weapon);
        }

        private void CollectWeapon(ICollectableWeapon weapon)
        {
            weapon.Hide();
            if (loadout.Collect(weapon.WeaponIndex))
                suitAudio.PlayPickup();
        }

        private void FixedUpdate()
            => ApplyMovement();

        private void LateUpdate()
        {
            if (Time.timeScale <= 0f)
                return;

            UpdateAim();

            bool firing = IsFiring();
            attackPulse = false;
            float locomotion = IdleLocomotion;
            if (currentSpeed > 0f)
                locomotion = isSprinting ? RunLocomotion : WalkLocomotion;

            animator.speed = 1f;
            animator.SetFloat(speedAnimatorProperty, locomotion);
            animator.SetFloat(facingAnimatorProperty, FacingSide);
            animator.SetFloat(gunAnimatorProperty, IdleLocomotion);
            spriteRenderer.flipX = aimDirection.x > 0f;
            suitAudio.TickSteps(currentSpeed > 0f, isSprinting);
            PlayReloadCues();

            if (!firing)
                loadout.ReleaseTrigger();

            if (!loadout.IsDrawn)
            {
                loadout.Hide();
                return;
            }

            loadout.Aim(aimDirection);
            if (!firing)
                return;

            if (loadout.TryShoot(aimDirection))
                suitAudio.PlayShot(loadout.Selected);
            else if (loadout.ConsumeEmptyClick())
                suitAudio.PlayEmpty();
        }

        private void PlayReloadCues()
        {
            int started = loadout.ReloadStarted;
            if (started != heardReloadStarted)
            {
                heardReloadStarted = started;
                suitAudio.PlayReload();
            }

            int finished = loadout.ReloadFinished;
            if (finished != heardReloadFinished)
            {
                heardReloadFinished = finished;
                suitAudio.PlayReloadReady();
            }
        }

        private void UpdateAim()
        {
            if (Mouse.current == null || gameplayCamera == null)
                return;

            Vector2 screen = Mouse.current.position.ReadValue();
            float depth = -gameplayCamera.transform.position.z;
            Vector3 world = gameplayCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            Vector2 delta = (Vector2)world - loadout.AimOrigin;
            if (delta.sqrMagnitude < AimEpsilon)
                return;

            aimDirection = delta.normalized;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
                isSprinting = true;
            else if (context.canceled)
                isSprinting = false;
        }

        private void ApplyMovement()
        {
            if (moveInput.sqrMagnitude == 0f)
            {
                currentSpeed = 0f;
                return;
            }

            lastMoveInputNormalized = moveInput.normalized;

            currentSpeed = moveSpeed;
            if (isSprinting)
                currentSpeed *= sprintMultiplier;

            Vector2 nextStep = lastMoveInputNormalized * (currentSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + nextStep);
        }

        public void OnLook(InputAction.CallbackContext context) { }
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                attackHeld = true;
                attackPulse = true;
            }
            else if (context.canceled)
                attackHeld = false;
        }

        private bool IsFiring()
        {
            if (attackHeld || attackPulse)
                return true;

            Mouse mouse = Mouse.current;
            return mouse != null && mouse.rightButton.isPressed;
        }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
    }
}
