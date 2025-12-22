using Game.Battle.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Battle.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterController : MonoBehaviour, BattleActions.IPlayerActions
    {
        private static readonly int xInputAnimatorProperty = Animator.StringToHash("X Input");
        private static readonly int yInputAnimatorProperty = Animator.StringToHash("Y Input");
        private static readonly int speedAnimatorProperty = Animator.StringToHash("Speed");
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
        private WeaponRenderer weaponRenderer;
    
        private Rigidbody2D rb;
    
        private Vector2 moveInput;
        private Vector2 lastMoveInputNormalized;
    
        private bool isSprinting;

        private float currentSpeed;
    
        private BattleActions actions;        
        private BattleActions.PlayerActions playerActions;

        private int weaponLayer;

        private IWeapon currentWeapon = new EmptyWeapon();
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        
            actions = new BattleActions();
            playerActions = actions.Player;
            playerActions.AddCallbacks(this);
            
            weaponLayer = LayerMask.NameToLayer($"Weapon");
            
            weaponRenderer.HideRenderer();
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
            if (other.gameObject.layer == weaponLayer && other.TryGetComponent<ICollectableWeapon>(out var weapon))
                CollectWeapon(weapon);
        }

        private void CollectWeapon(ICollectableWeapon weapon)
        {
            weapon.Hide();
            currentWeapon = weapon;
            weaponRenderer.ShowRenderer();
        }

        private void FixedUpdate()
            => ApplyMovement();

        private void LateUpdate()
        {
            animator.SetFloat(xInputAnimatorProperty, lastMoveInputNormalized.x);
            animator.SetFloat(yInputAnimatorProperty, lastMoveInputNormalized.y);
            animator.SetFloat(speedAnimatorProperty, currentSpeed);
            animator.SetFloat(gunAnimatorProperty, currentWeapon.weaponAnimationIndex);

            var isFlip = lastMoveInputNormalized.x > 0;
            
            spriteRenderer.flipX = isFlip;
            weaponRenderer.SetFlip(isFlip);
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
            currentSpeed = moveSpeed;
        
            if (isSprinting)
                currentSpeed *= sprintMultiplier;

            if (moveInput.normalized == Vector2.zero)
                currentSpeed = 0;
            else
                MovePosition();
        }

        private void MovePosition()
        {
            lastMoveInputNormalized = moveInput.normalized;
            var nextVec = moveInput.normalized * (currentSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + nextVec);
        }

        public void OnLook(InputAction.CallbackContext context) { }
        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
    }
}