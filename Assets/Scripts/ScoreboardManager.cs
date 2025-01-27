using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance;

    [SerializeField] private TMP_Text team1PointsText;
    [SerializeField] private TMP_Text team2PointsText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private float totalMatchTimeSeconds;
    private float _currentMatchTimeSeconds;

    private bool _timeTicking;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (_timeTicking)
        {
            _currentMatchTimeSeconds -= Time.deltaTime;

            _currentMatchTimeSeconds = MathF.Max(_currentMatchTimeSeconds, 0.0f);

            uint timeSeconds = (uint)math.floor(_currentMatchTimeSeconds);

            uint timeMinutes = timeSeconds / 60;
            timeSeconds %= 60;

            timeText.SetText(string.Format("{0}:{1:00}", timeMinutes, timeSeconds));

            if (_currentMatchTimeSeconds == 0.0f)
                if (!MatchManager.Instance.TryWinByTime())
                    timeText.SetText("OT");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartTimer()
    {
        _currentMatchTimeSeconds = totalMatchTimeSeconds;
        _timeTicking = true;
    }

    public void StopTimer()
    {
        _timeTicking = true;
    }

    public void SetScore(uint team1Score, uint team2Score)
    {
        team1PointsText.SetText(team1Score.ToString());
        team2PointsText.SetText(team2Score.ToString());
    }
}