using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


public enum PlayerState
{
    Idle,       // 대기
    Run,        // 달리기
    Jump,       // 점프
    Dash,       // 대시 중
    Attack,     // 공격 중
    Dead        // 사망
}

public class PlayerController : MonoBehaviour
{
    // StringToHash 최적화
    private readonly int hashDashing = Animator.StringToHash("Dashing");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashMove = Animator.StringToHash("Move");
    private readonly int hashGrounded = Animator.StringToHash("Grounded");
    private readonly int hashDie = Animator.StringToHash("Die");

    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float dashingSpeed = 10f;
    public LayerMask groundLayer;

    private float gravityScale;
    private float moveInput;

    // 현재 상태 딱 하나만 관리
    private PlayerState currentState = PlayerState.Idle;

    // 바닥 체크
    private bool isGrounded = true;

    private float dashCooldown = 1f;
    private float lastdashTime = -10f; // 시작하자마자 대시 가능하게 초기값 세팅

    private WaitForSeconds dashWait;
    private WaitForSeconds dashAnimation;

    [SerializeField] private AudioClip dashClip; // 대쉬 사용
    private AudioSource playerAudio;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerHealth ph;
    private PlayerAttack pa;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        ph = GetComponent<PlayerHealth>();
        pa = GetComponent<PlayerAttack>();
        playerAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        gravityScale = rb.gravityScale;
        dashWait = new WaitForSeconds(0.3f);
        dashAnimation = new WaitForSeconds(0.271f);
    }

    private void OnEnable()
    {
        ph.onPlayerDeath += HandleDeath;
        pa.Attacked += OnAttackEnded; // 공격 끝났다는 이벤트 구독
    }

    private void OnDisable()
    {
        ph.onPlayerDeath -= HandleDeath;
        pa.Attacked -= OnAttackEnded;
    }

    private void Update()
    {
        CheckGround();
        UpdateAnimation();

        // 2. FSM의 핵심, 현재 상태에 맞는 함수만 딱딱 실행
        switch (currentState)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Run:
                UpdateRun();
                break;
            case PlayerState.Jump:
                UpdateJump();
                break;
            case PlayerState.Attack:
            case PlayerState.Dash:
            case PlayerState.Dead:
                // 공격, 대시, 사망 중일 때는 Update에서 따로 입력 안 받음. 알아서 끝날때까지 대기
                break;
        }
    }

    private void FixedUpdate()
    {
        // 대기, 달리기, 점프 상태일 때만 물리 이동 적용, 공격/대시 중 미끄러짐 방지
        if (currentState == PlayerState.Idle || currentState == PlayerState.Run || currentState == PlayerState.Jump)
        {
            rb.linearVelocityX = moveInput * moveSpeed;
        }
    }

    // 상태 변경 함수
    private void ChangeState(PlayerState newState)
    {
        if (currentState == PlayerState.Dead) return;

        // 빠져나올 때
        if (currentState == PlayerState.Dash)
        {
            rb.gravityScale = gravityScale; // 대시 끝나면 중력 원상복구
            rb.linearVelocityX = 0;
        }

        // 상태 변경
        currentState = newState;

        // 새로 들어갈 때
        if (currentState == PlayerState.Attack)
        {
            moveInput = 0;
            rb.linearVelocityX = 0;
            animator.SetTrigger(hashAttack); // 공격 애니메이션 시작
        }
        else if (currentState == PlayerState.Dash)
        {
            StartCoroutine(DashRoutine()); // 대시 코루틴 시작
        }
    }

    // --- 각 상태별 로직들 (여기서 입력받고 상태를 전환함) ---
    private void UpdateIdle()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Fire1") && isGrounded) 
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            ChangeState(PlayerState.Attack); return; 
        }
        if (Input.GetButtonDown("Fire2") && Time.time > dashCooldown + lastdashTime) { ChangeState(PlayerState.Dash); return; }
        if (Input.GetButtonDown("Jump") && isGrounded) { rb.linearVelocityY = jumpForce; ChangeState(PlayerState.Jump); return; }

        if (moveInput != 0) { ChangeState(PlayerState.Run); }
    }

    private void UpdateRun()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0) { transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1); }
        else { ChangeState(PlayerState.Idle); return; }

        if (Input.GetButtonDown("Fire1") && isGrounded) 
        {
            if (EventSystem.current.IsPointerOverGameObject()) // UI 클릭은 공격 입력으로 처리하지 않음
                return;
            ChangeState(PlayerState.Attack); return; 
        }
        if (Input.GetButtonDown("Fire2") && Time.time > dashCooldown + lastdashTime) { ChangeState(PlayerState.Dash); return; }
        if (Input.GetButtonDown("Jump") && isGrounded) { rb.linearVelocityY = jumpForce; ChangeState(PlayerState.Jump); return; }

        if (!isGrounded) { ChangeState(PlayerState.Jump); } // 절벽에서 떨어질 때
    }

    private void UpdateJump()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0) { transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1); }

        if (Input.GetButtonDown("Fire2") && Time.time > dashCooldown + lastdashTime) { ChangeState(PlayerState.Dash); return; }

        // 착지 확인!
        if (isGrounded && rb.linearVelocityY <= 0.1f)
        {
            if (moveInput != 0) ChangeState(PlayerState.Run);
            else ChangeState(PlayerState.Idle);
        }
    }

    private IEnumerator DashRoutine()
    {
        playerAudio.PlayOneShot(dashClip); // 이 소리 한번만 쏴라 라고 던지는 함수

        lastdashTime = Time.time;
        animator.SetTrigger(hashDashing);
        moveInput = 0;

        rb.linearVelocityY = 0;
        rb.linearVelocityX = transform.localScale.x * dashingSpeed;
        rb.gravityScale = 0;

        yield return dashWait;

        rb.linearVelocityX = 0;
        rb.gravityScale = gravityScale;

        yield return dashAnimation;

        // 대시가 끝났을 때 아직도 Dash 상태라면 Idle로 복귀
        if (currentState == PlayerState.Dash)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    // PlayerAttack에서 Action을 통해 호출되는 함수
    private void OnAttackEnded()
    {
        if (currentState == PlayerState.Attack)
        {
            ChangeState(PlayerState.Idle); // 공격 끝났으니 대기 상태
        }
    }

    private void UpdateAnimation()
    {
        animator.SetBool(hashGrounded, isGrounded);
        animator.SetBool(hashMove, currentState == PlayerState.Run); // 달릴 때만 Move 애니메이션 켜기
    }

    private void HandleDeath()
    {
        ChangeState(PlayerState.Dead);
        animator.SetTrigger(hashDie);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        isGrounded = hit.collider != null;
    }
}