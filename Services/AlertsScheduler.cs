using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using Lc_auto.Models;
using Timer = System.Timers.Timer;

namespace Lc_auto.Services;

/// <summary>
/// 구성된 알림 시간을 기반으로 Windows 타이머를 관리하며 AlertTriggered 이벤트를 발생시키는 스케줄러입니다.
/// </summary>
public class AlertsScheduler : IAlertsScheduler
{
    private const string RepeatDaily = "daily";
    private const string RepeatOnce = "once";

    private readonly object _sync = new();
    private readonly List<Timer> _timers = new();
    private readonly Dictionary<Timer, AlertConfig> _timerMap = new();
    private List<AlertConfig> _alerts = new();
    private bool _isRunning;

    /// <inheritdoc />
    public event EventHandler<string>? AlertTriggered;

    /// <inheritdoc />
    public void Apply(IEnumerable<AlertConfig> alerts)
    {
        ArgumentNullException.ThrowIfNull(alerts);

        var sanitizedAlerts = alerts
            .Where(a => a != null)
            .Select(CloneAlert)
            .ToList();

        lock (_sync)
        {
            _alerts = sanitizedAlerts;

            if (_isRunning)
            {
                RebuildTimersLocked();
            }
        }
    }

    /// <inheritdoc />
    public void Start()
    {
        lock (_sync)
        {
            if (_isRunning)
            {
                return;
            }

            if (_alerts.Count == 0)
            {
                LoggingService.LogWarn("등록된 알림이 없어 AlertsScheduler를 시작하지 않습니다.");
                return;
            }

            _isRunning = true;
            RebuildTimersLocked();
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        lock (_sync)
        {
            if (!_isRunning && _timers.Count == 0)
            {
                return;
            }

            _isRunning = false;
            DisposeAllTimersLocked();
        }
    }

    private void RebuildTimersLocked()
    {
        DisposeAllTimersLocked();

        if (!_isRunning)
        {
            return;
        }

        var now = DateTime.Now;
        foreach (var alert in _alerts)
        {
            ScheduleAlertLocked(alert, now);
        }
    }

    private void ScheduleAlertLocked(AlertConfig alert, DateTime referenceTime, Timer? reusableTimer = null)
    {
        var nextRun = CalculateNextRun(alert, referenceTime);
        if (nextRun == null)
        {
            if (reusableTimer != null)
            {
                RemoveTimerLocked(reusableTimer, dispose: true);
            }

            return;
        }

        var intervalMs = Math.Max(1, (nextRun.Value - DateTime.Now).TotalMilliseconds);

        Timer timer;
        if (reusableTimer == null)
        {
            timer = new Timer(intervalMs)
            {
                AutoReset = false
            };
            timer.Elapsed += OnTimerElapsed;
            _timers.Add(timer);
            _timerMap[timer] = alert;
        }
        else
        {
            timer = reusableTimer;
            timer.Stop();
            timer.Interval = intervalMs;
        }

        var repeat = NormalizeRepeat(alert.Repeat);
        LoggingService.LogInfo($"알림 스케줄 등록: time={alert.Time}, repeat={repeat}, next={nextRun:yyyy-MM-dd HH:mm}");

        timer.Start();
    }

    private DateTime? CalculateNextRun(AlertConfig alert, DateTime referenceTime)
    {
        if (!TryParseTime(alert.Time, out var alertTime))
        {
            LoggingService.LogError($"알림 시간 파싱에 실패했습니다. time={alert.Time}", new FormatException("Invalid HH:mm"));
            return null;
        }

        var repeat = NormalizeRepeat(alert.Repeat);
        var candidate = referenceTime.Date + alertTime;

        if (candidate <= referenceTime)
        {
            if (repeat == RepeatDaily)
            {
                candidate = candidate.AddDays(1);
            }
            else
            {
                LoggingService.LogWarn($"과거 시각의 1회성 알림을 건너뜁니다. time={alert.Time}");
                return null;
            }
        }

        return candidate;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (sender is not Timer timer)
        {
            return;
        }

        AlertConfig? alert;
        lock (_sync)
        {
            if (!_timerMap.TryGetValue(timer, out alert))
            {
                return;
            }
        }

        var message = alert.Message;
        try
        {
            AlertTriggered?.Invoke(this, message);
        }
        catch (Exception ex)
        {
            LoggingService.LogError("AlertTriggered 이벤트 처리 중 예외가 발생했습니다.", ex);
        }

        lock (_sync)
        {
            if (!_timerMap.TryGetValue(timer, out alert))
            {
                return;
            }

            if (!_isRunning)
            {
                RemoveTimerLocked(timer, dispose: true);
                return;
            }

            var repeat = NormalizeRepeat(alert.Repeat);
            if (repeat == RepeatDaily)
            {
                ScheduleAlertLocked(alert, DateTime.Now, timer);
            }
            else
            {
                LoggingService.LogInfo($"1회성 알림을 완료했습니다. time={alert.Time}");
                RemoveTimerLocked(timer, dispose: true);
            }
        }
    }

    private void DisposeAllTimersLocked()
    {
        foreach (var timer in _timers.ToList())
        {
            RemoveTimerLocked(timer, dispose: true);
        }

        _timers.Clear();
        _timerMap.Clear();
    }

    private void RemoveTimerLocked(Timer timer, bool dispose)
    {
        if (_timerMap.ContainsKey(timer))
        {
            _timerMap.Remove(timer);
        }

        _timers.Remove(timer);

        if (!dispose)
        {
            return;
        }

        try
        {
            timer.Stop();
            timer.Elapsed -= OnTimerElapsed;
            timer.Dispose();
        }
        catch (Exception ex)
        {
            LoggingService.LogWarn("알림 타이머 정리 중 경고가 발생했습니다.", ex);
        }
    }

    private static bool TryParseTime(string time, out TimeSpan result)
    {
        return TimeSpan.TryParseExact(time, @"hh\:mm", CultureInfo.InvariantCulture, out result);
    }

    private static string NormalizeRepeat(string? repeat)
    {
        if (string.IsNullOrWhiteSpace(repeat))
        {
            return RepeatDaily;
        }

        var normalized = repeat.Trim().ToLowerInvariant();
        return normalized is RepeatDaily or RepeatOnce ? normalized : RepeatDaily;
    }

    private static AlertConfig CloneAlert(AlertConfig alert)
    {
        return new AlertConfig
        {
            Time = alert.Time,
            Message = alert.Message,
            Repeat = alert.Repeat
        };
    }
}
