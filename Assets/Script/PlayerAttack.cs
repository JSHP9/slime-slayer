using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public event Action Attacked;
    // 인스펙터 세팅용 변수들
    public Transform attackPoint; // 1. 허공에 원을 그릴 중심점 (빈 오브젝트를 플레이어 자식으로 만들어서 연결)
    public float attackRadius = 0.5f; // 2. 타격 범위 (반지름)
    public LayerMask enemyLayer; // 때릴 대상의 레이어(ex:Enemy)

    public void Attack()
    {
        // 물리엔진이 attackPoint위치에, attackRadius 크기의 원을 그려서, enemyLayer인 애들 싹 다 배열로가저옴.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        // 원 안에 들어온 슬라임들을 하나씩 꺼내서 공격
        foreach(Collider2D enemy in hitEnemies)
        {
            if (enemy.isTrigger) { continue; } // 적 추격 레이더가 히트박스가 되는걸 방지.
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                damageable.TakeDamage(10, knockbackDirection);
            }

        }
    }
    public void AttackEnd()
    {
        Attacked?.Invoke();
    }

    // Scene 뷰에서 공격 범위를 눈으로 보게 해주는 방법
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        // attackPoint를 중심으로 빨간색 테두리 원을 그려줌 (인스펙터에서 크기 조절할 때 개꿀임)
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
