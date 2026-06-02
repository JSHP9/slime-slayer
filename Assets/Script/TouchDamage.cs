using System;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    // OnCollisionStay2D는 Unity가 Collision2D를 넘겨주도록 정해져 있어서 매개변수를 Collider2D로 바꿀 수 없음.
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 나랑 부딪힌 녀석(collision.gameObject)한테서 IDamageable 컴포넌트를 뽑음.
        IDamageable entity = collision.gameObject.GetComponent<IDamageable>();
        // 컴포넌트가 존재
        if (entity != null)
        {
            // damage, 넉백 방향 벡터
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            entity.TakeDamage(damage, dir);
        }
    }
}
