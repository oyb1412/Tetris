using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause UI ����
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //���� ���� ���� ����
    public bool IsLive { get; private set; }
    //����� ��� UI ������Ʈ
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
    /// ���� �簳(�ݹ����� ȣ��)
    /// </summary>
    public void ResumeClick()
    {
        pausePanel.SetActive(false);
        IsLive = true;
    }

    /// <summary>
    /// ���� ����(�ݹ����� ȣ��)
    /// </summary>
    public void Pauselick()
    {
        pausePanel.SetActive(true);
        IsLive = false;
    }

    /// <summary>
    /// ���� �����(�ݹ����� ȣ��)
    /// </summary>
    public void RestartClick()
    {
        SceneManager.LoadScene(0);
    }
}
