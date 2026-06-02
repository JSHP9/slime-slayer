using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 유니티 씬이 다시 로드되어도 gameObject를 파괴하지 않음.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이후 새로 생긴건 삭제
            Destroy(gameObject);
        }
    }
}