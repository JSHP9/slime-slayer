using System.Collections; // 코루틴 쓸때 씀
using UnityEngine;
using System; // Action 쓸때 씀

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action onPlayerDeath;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private AudioClip hitClip; // 데미지 받음
    [SerializeField] private AudioClip deathClip; // 죽음
    private AudioSource playerAudio;

    private int currentHp;
    private bool isInvincible = false;

    private SpriteRenderer sr; // 플레이어 반투명하게 바꾸기 위해 사용
    private WaitForSeconds invincibleTime; // 플레이어 무적 시간
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        playerAudio = GetComponent<AudioSource>();
    }
    void Start()
    {
        currentHp = maxHp;
        invincibleTime = new WaitForSeconds(1.5f);
    }
    
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible) { return; }
        currentHp -= damage;
        playerAudio.PlayOneShot(hitClip);
        UIManager.Instance.UpdateHearts(currentHp); // 깎인 체력을 매니저한테 즉시 보고
        if ( currentHp <= 0 )
        {
            isInvincible=true; // 시체 훼손 방지
            Die();
            return;
        }

        StartCoroutine(Invincible());
    }
    private IEnumerator Invincible()
    {
        isInvincible = true;
        // 반투명하게 바꿈
        sr.color = new Color(1, 1, 1, 0.5f);
        // 무적 시간
        yield return invincibleTime;
        // 투명도 원복
        sr.color = new Color(1, 1, 1, 1f);

        isInvincible = false;
    }

    private void Die()
    {
        playerAudio.PlayOneShot(deathClip);
        onPlayerDeath?.Invoke();
        UIManager.Instance.ShowGameOver(); // 게임오버 배너 호출
    }
}
