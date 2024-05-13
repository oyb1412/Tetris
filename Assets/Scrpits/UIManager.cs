using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause UI 관리
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //현재 게임 진행 여부
    public bool IsLive { get; private set; }
    //퍼즈시 출력 UI 오브젝트
    [SerializeField]private GameObject pausePanel;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        IsLive = true;
    }

    /// <summary>
    /// 게임 재개(콜백으로 호출)
    /// </summary>
    public void ResumeClick()
    {
        pausePanel.SetActive(false);
        IsLive = true;
    }

    /// <summary>
    /// 게임 퍼즈(콜백으로 호출)
    /// </summary>
    public void Pauselick()
    {
        pausePanel.SetActive(true);
        IsLive = false;
    }

    /// <summary>
    /// 게임 재시작(콜백으로 호출)
    /// </summary>
    public void RestartClick()
    {
        SceneManager.LoadScene(0);
    }
}
