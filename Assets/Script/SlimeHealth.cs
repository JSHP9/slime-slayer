using UnityEngine;
using System.Collections;

public class SlimeHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 30;
    [SerializeField] private float knokbackPower = 50f;
    
    [SerializeField] private AudioClip hitClip; // 데미지 받음
    [SerializeField] private AudioClip deathClip; // 죽음
    private AudioSource slimeAudio;

    private int currentHp;
    private Rigidbody2D rb;
    void Awake()
    {
        slimeAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage, Vector2 knokbackDirection)
    {
        currentHp -= damage;
        if (currentHp > 0)
            slimeAudio.PlayOneShot(hitClip); // 내장스피커임.
        StartCoroutine(KnokbackRoutine(knokbackDirection));
        if (currentHp <= 0) {
            Die();
            return;
        }
    }

    private IEnumerator KnokbackRoutine(Vector2 dir)
    {
        // SlimePatrol 비활성화
        GetComponent<SlimePatrol>().enabled = false;
        // 원래 가속도를 0
        rb.linearVelocity = Vector2.zero;
        // ForceMode2D.Impulse: 순간적으로 힘을 가해서 속도를 즉시 바꾸는 모드
        Vector2 launchDir = new Vector2(dir.x, 0.8f).normalized; // 뒤로 + 위로 튕기게 방향 설정
        rb.AddForce(launchDir * knokbackPower, ForceMode2D.Impulse);

        // 0.2초동안 기절
        yield return new WaitForSeconds(0.5f);
        // 가속도 0
        rb.linearVelocity = Vector2.zero;

        // SlimePatrol 활성화
        if (currentHp > 0)
        {
            GetComponent<SlimePatrol>().enabled = true;
        }
    }

    private void Die()
    {
        // 시한부 함수(소리재생이 끝날때까지 유니티가 알아서 스피커를 살려뒀다가 지움)
        AudioSource.PlayClipAtPoint(deathClip, Camera.main.transform.position); // 소리 소환위치를 메인카메라 위치로 변경
        UIManager.Instance.DecreaseSlime(); // 슬라임 하나 죽었음 보고
        Destroy(gameObject);
    }
}
