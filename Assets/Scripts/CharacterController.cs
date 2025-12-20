using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class CharacterController : MonoBehaviour, GameActions.IPlayerActions
{
    private static readonly int xInputAnimatorProperty = Animator.StringToHash("X Input");
    private static readonly int yInputAnimatorProperty = Animator.StringToHash("Y Input");
    private static readonly int speedAnimatorProperty = Animator.StringToHash("Speed");

    [SerializeField, Range(0, 10f)] 
    private float moveSpeed = 5f;
    
    [SerializeField, Range(1, 3f)] 
    private float sprintMultiplier = 1.5f;
    
    [SerializeField]
    private Animator animator;
    
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D rb;
    
    private Vector2 moveInput;
    private Vector2 lastMoveInputNormalized;
    
    private bool isSprinting;

    private float currentSpeed;
    
    private GameActions actions;        
    private GameActions.PlayerActions playerActions;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        actions = new GameActions();
        playerActions = actions.Player;
        playerActions.AddCallbacks(this);
    }

    private void OnDestroy()
        => actions.Dispose();

    private void OnEnable()
        => playerActions.Enable();

    private void OnDisable()
        => playerActions.Disable();
    
    public void OnMove(InputAction.CallbackContext context)
        => moveInput = context.ReadValue<Vector2>();
    
    private void FixedUpdate()
        => ApplyMovement();

    private void LateUpdate()
    {
        animator.SetFloat(xInputAnimatorProperty, lastMoveInputNormalized.normalized.x);
        animator.SetFloat(yInputAnimatorProperty, lastMoveInputNormalized.normalized.y);
        animator.SetFloat(speedAnimatorProperty, currentSpeed);
        
        spriteRenderer.flipX = lastMoveInputNormalized.normalized.x > 0;
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
