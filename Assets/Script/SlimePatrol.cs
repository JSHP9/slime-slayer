using UnityEngine;

public enum SlimeState
{
    Patrol, // 순찰
    Chase, // 추격
    Dead // 사망
}
public class SlimePatrol : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 2.0f; // 이동속도
    [SerializeField] private int direction = 1; // 방향
    [SerializeField] private float chaseSpeed = 3.5f;

    private bool isGrounded = false; // 땅에 닿는지 확인
    private float offset = 0.7972635f; // 몸통 길이

    private Transform playerTarget; // 플레이어의 위치정보

    private SlimeState currentState = SlimeState.Patrol; // 기본 상태

    // 바닥 레이어 설정
    public LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGround();

        switch (currentState)
        {
            case SlimeState.Patrol:
                PatrolSlime();
                break;
            case SlimeState.Chase:
                ChasePlayer();
                break;
            case SlimeState.Dead: break;
        }

    }

    private void PatrolSlime()
    {
        // 슬라임 위치에서 아래로 0.1f만큼 레이저 쏘기 -> transform.position이 몸통이기때문에 몸길이(0.8701255) 반절 나눠서 0.2정도 더함
        // 슬라임 가로 사이즈가 1.594527임 반절 나누면 0.7972635임.
        // 레이캐스트(시작지점, 방향, 사정거리, 타겟 레이어) <- 2D기준
        Vector2 rayStart = (Vector2)transform.position + new Vector2(offset * direction, 0);
        RaycastHit2D hitDown = Physics2D.Raycast(rayStart, Vector2.down, offset, groundLayer);
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, Vector2.right * direction, offset, groundLayer);

        if ((hitWall.collider != null || hitDown.collider == null) && isGrounded)
        {
            // 방향 변경
            direction *= -1;
            // 좌우 대칭
            Vector3 currentScale = transform.localScale; // transform.localScale이 값을 직접 수정하는게 안됨. 변수에 저장하고 수정해야함. 자체를 교체하는건 가능.
            currentScale.x = Mathf.Abs(currentScale.x) * direction; // 좌우 대칭. 현재 상태 뒤집기가 아니라 direction 상태에 맞게 강제 세팅.
            transform.localScale = currentScale; // 자체를 교체
        }
    }

    private void ChasePlayer()
    {
        // 좌우로만 가니까 x값만 필요
        float dir = (playerTarget.transform.position - transform.position).x;
        // 힌트 2: Mathf.Sign(방향.x)를 쓰면 왼쪽(-1)인지 오른쪽(1)인지 깔끔하게 나옴.
        direction = (int)Mathf.Sign(dir);
        Vector3 currentScale = transform.localScale; 
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentState = SlimeState.Chase; // 추적 상태로 변경
            playerTarget = collision.transform; // 플레이어 위치 저장

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 플레이어 탈출하면
        if (collision.CompareTag("Player"))
        {
            currentState = SlimeState.Patrol; // 순찰 상태로 변경
            playerTarget = null; // 플레이어 위치 비워둠
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case SlimeState.Patrol:
                rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);
                break;

            case SlimeState.Chase:
                rb.linearVelocity = new Vector2(chaseSpeed * direction, rb.linearVelocity.y);
                break;

            case SlimeState.Dead:
                // 죽었을 때 관성 때문에 미끄러지는 거 방지
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, offset, groundLayer);

        if (hit)
        {
            isGrounded = true;
            return;
        }

        isGrounded = false;
    }
}
