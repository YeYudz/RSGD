using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeMgr : MonoBehaviour
{
    public static TimeMgr Instance { get; private set; }

    [Header("UI ????")]
    public Text timerText; 

    private float elapsedTime = 0f;
    private bool isTracking = true;
    private bool isOpen = false;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Update()
    {
        if (isTracking)
        {
            elapsedTime += Time.unscaledDeltaTime;
            UpdateTimerUI();
        }
        if(elapsedTime>2f&&!isOpen)
        {
            LevelNodeMgr.GetInstance().StartGame();
            isOpen = true; 
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isTracking = false;
        isOpen = false;
        UpdateTimerUI();
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTracking = true;
        isOpen = false;
        UpdateTimerUI();
    }

    public void PauseTimer()
    {
        isTracking = false;
    }

    public void ResumeTimer()
    {
        isTracking = true;
    }

    public float GetCurrentTime()
    {
        return elapsedTime;
    }
    public void PauseScale()
    {
        Time.timeScale = 0f;
    }
    public void ResumeScale()
    {
        Time.timeScale = 1f;
    }
}
