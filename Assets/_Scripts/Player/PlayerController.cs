using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Component References")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sr;

    [Header("Movement")]
        public float maxSpeed = 8f;        // Maximální rychlost běhu
        public float acceleration = 40f;   // Jak rychle se dostane na maxSpeed
        public float deceleration = 50f;   // Jak rychle zabrzdí, když pustíš klávesu
        private float _horizontal;
    
    [Header("Jump")]
        public float jumpPower = 7.5f;
        public float fallMultiplier = 2.5f; // Jak rychle padá dolů
        public float lowJumpMultiplier = 2f; // Jak rychle ho to stáhne, když pustí mezerník brzo
        public Vector2 groundCheckSize = new Vector2(0.4f, 0.2f);
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;
        private float _coyoteTime = 0.2f;
        private float _coyoteTimeCounter;
        private float _jumpBufferTime = 0.2f;
        private float _jumpBufferTimeCounter;

    [Header("Graphics & Flip")]
        public Transform VisualsTransform; 
        public float FlipSpeed = 1200f; // Rychlost otáčení (musí být vysoká, aby to bylo "snappy")
        private bool isFacingRight = true;

    void Start()
    {
        
    }

    void Update()
    {
        RegisterUserInput();
        FlipPlayerSprite();
        UpdateAnimatorParameters();

        Jump();
    }

    void FixedUpdate()
    {
        PlayerMovement();
    }

    private void RegisterUserInput()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void PlayerMovement()
    {
        // Vypočítáme, jakou rychlostí chceme ideálně běžet (cíl)
        float targetSpeed = _horizontal * maxSpeed;

        // Rozhodneme, jestli zrovna zrychlujeme (držíme šipku), nebo brzdíme (nepouštíme nic)
        float accelRate = (Mathf.Abs(_horizontal) > 0.01f) ? acceleration : deceleration;

        // Plynule posuneme současnou rychlost směrem k cílové rychlosti
        float newSpeedX = Mathf.MoveTowards(_rb.linearVelocityX, targetSpeed, accelRate * Time.fixedDeltaTime);

        // Aplikujeme novou plynulou rychlost, Y osu necháme být (gravitace)
        _rb.linearVelocity = new Vector2(newSpeedX, _rb.linearVelocityY);
    }

    private void UpdateAnimatorParameters()
    {
        _anim.SetFloat("velocityX", Mathf.Abs(_horizontal));
    }
    private void FlipPlayerSprite()
    {
        // 1. Změna směru pohledu podle vstupu
        if (_horizontal > 0.01f && !isFacingRight)
        {
            isFacingRight = true;
        }
        else if (_horizontal < -0.01f && isFacingRight)
        {
            isFacingRight = false;
        }

        // 2. PLYNULÝ FLIP (Krok za krokem rotujeme osu Y)
        // Cílový úhel: 0 pokud koukáme doprava, 180 pokud doleva
        float targetAngle = isFacingRight ? 0f : 180f;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        // RotateTowards zajistí, že rotace se nikdy "nepřetočí" a dorazí přesně na cíl
        VisualsTransform.localRotation = Quaternion.RotateTowards(
            VisualsTransform.localRotation, 
            targetRotation, 
            FlipSpeed * Time.deltaTime
        );
    }
    private void Jump()
    {
        // 1. COYOTE TIME A BUFFER (Zůstává stejné, to máš skvěle)
        #region CoyoteTime & JumpBuffer
        if (IsGrounded())
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferTimeCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferTimeCounter -= Time.deltaTime;
        }
        #endregion

        // 2. SAMOTNÝ ODKOK (Pořád instantní)
        #region Jump Execute
        if (_coyoteTimeCounter > 0f && _jumpBufferTimeCounter > 0f)
        {
            _rb.linearVelocityY = jumpPower;
            
            _jumpBufferTimeCounter = 0f;
            _coyoteTimeCounter = 0f; 
        }
        #endregion

        // 3. MAGIE S GRAVITACÍ (Místo sekání rychlosti)
        #region Smooth Variable Jump (Gravity Modification)
        
        // 1. Záchrana na zemi: Pokud stojíme nebo běžíme, gravitace je vždy normální
        if (IsGrounded())
        {
            _rb.gravityScale = 1f;
        }
        else
        {
            // 2. Logika ve vzduchu s mrtvou zónou (0.1f) proti fyzikálním mikro-výkyvům
            if (_rb.linearVelocityY < -0.1f)
            {
                // Padáme
                _rb.gravityScale = fallMultiplier;
            }
            else if (_rb.linearVelocityY > 0.1f && !Input.GetButton("Jump"))
            {
                // Letíme nahoru, ale pustili jsme mezerník
                _rb.gravityScale = lowJumpMultiplier;
            }
            else
            {
                // Klasický let nahoru s drženým tlačítkem, nebo jsme v úplném vrcholu skoku (kolem 0)
                _rb.gravityScale = 1f;
            }
        }
        
        #endregion
    }
    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(_groundCheck.position, groundCheckSize, 0f, _groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_groundCheck.position, groundCheckSize);
    }
}
