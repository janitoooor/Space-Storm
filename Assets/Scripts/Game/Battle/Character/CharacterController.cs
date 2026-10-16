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
        private const float UnarmedPose = 0f;
        private const float RiflePose = 1f;
        private const float FacingDown = 0f;
        private const float FacingSide = 1f;
        private const float FacingUp = 2f;
        private const float AimEpsilon = 0.0001f;
        private const string WeaponLayerName = "Weapon";
        private const string RifleArmName = "rifle_side-overlay";
        private const string RifleArmStateName = "rifle_walk_side-overlay";

        private static readonly int speedAnimatorProperty = Animator.StringToHash("Speed");
        private static readonly int facingAnimatorProperty = Animator.StringToHash("Facing");
        private static readonly int gunAnimatorProperty = Animator.StringToHash("Gun");
        private static readonly int rifleArmState = Animator.StringToHash(RifleArmStateName);

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
        private GameObject rifleArm;
        private SpriteRenderer rifleArmRenderer;
        private Animator rifleArmAnimator;
        private Vector3 rifleArmRest;

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
            CacheRifleArm();
        }

        private void CacheRifleArm()
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name != RifleArmName)
                    continue;

                rifleArm = children[i].gameObject;
                rifleArmRenderer = rifleArm.GetComponent<SpriteRenderer>();
                rifleArmAnimator = rifleArm.GetComponent<Animator>();
                rifleArmRest = rifleArm.transform.localPosition;
                HidePackGuns(rifleArm.transform);
                return;
            }
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

            bool sprinting = isSprinting && !loadout.IsReloading;
            loadout.SetSuppressed(sprinting);

            bool firing = IsFiring();
            attackPulse = false;
            float locomotion = IdleLocomotion;
            if (currentSpeed > 0f)
                locomotion = sprinting ? RunLocomotion : WalkLocomotion;

            bool showWeapon = loadout.IsDrawn && !sprinting;
            float facing = FacingFor(aimDirection);
            float gun = showWeapon ? RiflePose : UnarmedPose;

            bool faceRight = facing == FacingSide && aimDirection.x > 0f;
            spriteRenderer.flipX = faceRight;
            ShowRifleArm(gun == RiflePose && facing == FacingSide, faceRight);
            SyncRifleArm();
            animator.speed = 1f;
            animator.SetFloat(speedAnimatorProperty, locomotion);
            animator.SetFloat(facingAnimatorProperty, facing);
            animator.SetFloat(gunAnimatorProperty, gun);
            suitAudio.TickSteps(currentSpeed > 0f, sprinting);
            PlayReloadCues();

            if (!firing)
                loadout.ReleaseTrigger();

            if (!showWeapon)
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

        private void SyncRifleArm()
        {
            if (rifleArm == null || rifleArmAnimator == null || !rifleArm.activeInHierarchy)
                return;

            rifleArm.transform.localPosition = rifleArmRest;
            float speed = IdleLocomotion;
            bool side = Mathf.Abs(animator.GetFloat(facingAnimatorProperty) - FacingSide) <= 0.01f;
            float shown = animator.GetFloat(speedAnimatorProperty);
            if (animator.GetFloat(gunAnimatorProperty) >= RiflePose && side && shown > IdleLocomotion && shown <= WalkLocomotion)
                speed = shown;

            rifleArmAnimator.SetFloat(speedAnimatorProperty, speed);
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            rifleArmAnimator.Play(rifleArmState, 0, state.normalizedTime);
            rifleArmAnimator.Update(0f);
        }

        private static float FacingFor(Vector2 direction)
        {
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                return direction.y > 0f ? FacingUp : FacingDown;

            return FacingSide;
        }

        private static void HidePackGuns(Transform arm)
        {
            for (int i = 0; i < arm.childCount; i++)
                arm.GetChild(i).gameObject.SetActive(false);
        }

        private void ShowRifleArm(bool show, bool faceRight)
        {
            if (rifleArm == null)
                return;

            if (rifleArm.activeSelf != show)
                rifleArm.SetActive(show);

            if (show && rifleArmRenderer != null)
                rifleArmRenderer.flipX = faceRight;
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
            if (isSprinting && !loadout.IsReloading)
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
