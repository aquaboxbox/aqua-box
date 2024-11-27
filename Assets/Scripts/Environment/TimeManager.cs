using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    [SerializeField, Range(0, 24)]
    [Tooltip("Hour of the day (0.0f-24.0f)")]
    private float hour = 12.0f;

    [SerializeField, Range(0, 6000)]
    [Tooltip("The speed of time as a multiplier of realtime (0-6000)")]
    private int timeScale = 6000;

    // Seconds of the day between 0.0f and 86400.0f
    private float seconds = 0.0f;

    // The current day
    private int days;

    private float totalElapsedTime = 0;
    private float startTime = 0;

    public bool timePaused = false;

    void Start()
    {
        // game will start at 08:00
        // hour = 2.5f;
        startTime = hour * 3600;
        totalElapsedTime += startTime;

        // Convert hours to seconds
        seconds = hour * 3600;

        // Set the current day
        days = 1;
    }

    void FixedUpdate()
    {
        if (timePaused)
        {
            return;
        }

        seconds += Time.fixedDeltaTime * timeScale;
        totalElapsedTime += Time.fixedDeltaTime * timeScale;
        hour = (seconds / 3600) % 24;

        if (seconds >= 86400)
        {
            days++;
            seconds = 0;
        }
    }

    public int GetCurrentDay()
    {
        // next day comes at 06:00
        if (hour < 6)
        {
            return days - 1;
        }
        else
        {
            return days;
        }
    }

    public float GetHour()
    {
        return hour;
    }

    public float GetSeconds()
    {
        return seconds;
    }

    public float GetNormalizedTime()
    {
        return hour / 24.0f;
    }

    public bool GetIsDay()
    {
        return hour >= 6 && hour < 18;
    }

    public bool IsTimePaused()
    {
        return timePaused;
    }

    public float GetTotalElapsedTime()
    {
        return totalElapsedTime;
    }

    public float GetTimeScale()
    {
        return timeScale;
    }

    public float GetElapsedTimeAtDay(int day)
    {
        return 86400 * (day - 1) + 6 * 3600;
    }
}
