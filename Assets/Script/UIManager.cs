using TMPro;
using UnityEngine;
using UnityEngine.UI; // Image 네임스페이스 생략
using UnityEngine.SceneManagement; // 씬 재시작

public class UIManager : MonoBehaviour
{
    public Image[] hearts; // 하트 이미지 배열
    public Sprite emptyHeart; // 텅 빈 하트

    [SerializeField] private TextMeshProUGUI slimeText; // 슬라임 숫자 띄울 텍스트
    [SerializeField] private GameObject gameOverPanel; // 게임오버 패널
    [SerializeField] private GameObject gameClearPanel; // 게임클리어 패널 
    [SerializeField] private GameObject gameStopPanel; // 게임중지 패널
    private int maxSlimeCount; // 최대 슬라임 수
    private int currentSlimeCount; // 지금 남은 슬라임 수
    public static UIManager Instance { get; private set; } // UIManager 객체 자체를 저장

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            // 먼저 들어온 놈이 진짜, 나중에 생긴 놈이 가짜 -> 싱글톤 핵심.
            Destroy(gameObject); // gameObject(_UIManager 자체를 삭제), this(UIManager 컴포넌트만 삭제)
            return;
        }
        Instance = this;
    }

    void Start()
    {
        maxSlimeCount = GameObject.FindGameObjectsWithTag("Enemy").Length; // find 계열 함수는 무거워서, 보통은 슬라임이 직접 UI매니저한테 신고 하게 만든다함.
        currentSlimeCount = maxSlimeCount;

        UpdateSlimeUI(); // 시작하자마자 텍스트 한번 띄움
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // 이건 조작키 바꿀일 없어서 걍 KeyCode로 씀(esc)임
        {
            if (gameStopPanel.activeSelf) // 게임정지패널 활성화 상태에서 esc눌렸으면 이어하기 버튼 자동 눌림
                ContinueGame();
            else
            {
                ShowGameStop();
                Time.timeScale = 0f; // 게임 속도 조절
            }
        }
    }

    public void UpdateSlimeUI()
    {
        // 현재마리수 / 총마리수
        slimeText.text = $"current slime: {currentSlimeCount} / {maxSlimeCount}";
    }

    public void DecreaseSlime()
    {
        currentSlimeCount--; // 슬라임 감소
        UpdateSlimeUI(); // 슬라임 UI 최신화
        if (currentSlimeCount == 0)
        {
            slimeText.text = " ";
            ShowGameClear(); // 게임 클리어 배너 호출
        }
    }

    public void UpdateHearts(int currentHp)
    {
        if (currentHp < 0 || currentHp >= hearts.Length)
            return;
        hearts[currentHp].sprite = emptyHeart;
    }
    public void ShowGameStop()
    {
        gameStopPanel.SetActive(true);
    }
    public void ShowGameClear()
    {
        gameClearPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true); // gameOver패널 활성화
    }

    public void RestartGame() // 재시작 버튼
    {
        Time.timeScale = 1f;
        // 현재 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueGame() // 이어하기 버튼
    {
        Time.timeScale = 1f;
        gameStopPanel.SetActive(false);
    }

    public void QuitGame() // 게임 종료 버튼
    {
        // 에디터 안에서는 Application.Quit()이 작동안함
        // 그래서 로그를 띄워서 버튼이 잘 눌렸는지 확인해야 함.
        Debug.Log("게임 강제 종료 버튼 클릭됨!");

        // 실제 게임을 빌드(exe)해서 실행했을 때 프로그램을 꺼주는 코드
        Application.Quit();
    }
}
