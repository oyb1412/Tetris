using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public bool isLive;
    public GameObject pausePanel;
    public static UIManager Instance;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        isLive = true;
    }

    public void ResumeClick()
    {
        pausePanel.SetActive(false);
        isLive = true;
    }
    public void Pauselick()
    {
        pausePanel.SetActive(true);
        isLive = false;
    }

    public void RestartClick()
    {
        SceneManager.LoadScene(0);
    }
}
