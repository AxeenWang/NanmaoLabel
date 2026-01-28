using System.Diagnostics;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 效能監控服務
/// [ref: 憲章 IV]
///
/// T075: 驗證啟動時間 ≤ 3 秒
/// T076: 驗證操作回應 ≤ 100ms
///
/// 效能要求：
/// - 應用程式啟動至可操作狀態必須（MUST）在 3 秒內完成
/// - 標註操作回應時間必須（MUST）在 100ms 以內
/// </summary>
public static class PerformanceMonitor
{
    /// <summary>
    /// 啟動時間上限（毫秒）
    /// [ref: 憲章 IV] 應用程式啟動至可操作狀態必須在 3 秒內完成
    /// </summary>
    public const int StartupTimeThresholdMs = 3000;

    /// <summary>
    /// 操作回應時間上限（毫秒）
    /// [ref: 憲章 IV] 標註操作回應時間必須在 100ms 以內
    /// </summary>
    public const int OperationResponseThresholdMs = 100;

    /// <summary>
    /// 應用程式啟動時間戳
    /// </summary>
    private static readonly Stopwatch StartupStopwatch = new();

    /// <summary>
    /// 上次記錄的啟動時間
    /// </summary>
    public static TimeSpan LastStartupTime { get; private set; }

    /// <summary>
    /// 上次記錄的操作回應時間
    /// </summary>
    public static TimeSpan LastOperationTime { get; private set; }

    /// <summary>
    /// 標記應用程式啟動開始
    /// [ref: 憲章 IV] T075
    /// </summary>
    public static void StartApplication()
    {
        StartupStopwatch.Restart();
    }

    /// <summary>
    /// 標記應用程式啟動完成並驗證時間
    /// [ref: 憲章 IV] T075
    /// </summary>
    /// <returns>啟動時間（毫秒）</returns>
    public static long EndStartup()
    {
        StartupStopwatch.Stop();
        LastStartupTime = StartupStopwatch.Elapsed;
        var elapsedMs = StartupStopwatch.ElapsedMilliseconds;

#if DEBUG
        // Debug 模式下輸出啟動時間供驗證
        Debug.WriteLine($"[PerformanceMonitor] 啟動時間：{elapsedMs}ms (上限：{StartupTimeThresholdMs}ms)");

        if (elapsedMs > StartupTimeThresholdMs)
        {
            Debug.WriteLine($"[PerformanceMonitor] ⚠️ 警告：啟動時間超過 {StartupTimeThresholdMs}ms！");
        }
#endif

        return elapsedMs;
    }

    /// <summary>
    /// 驗證啟動時間是否符合要求
    /// [ref: 憲章 IV] T075
    /// </summary>
    /// <returns>true 如果啟動時間 ≤ 3 秒</returns>
    public static bool ValidateStartupTime()
    {
        return LastStartupTime.TotalMilliseconds <= StartupTimeThresholdMs;
    }

    /// <summary>
    /// 測量操作執行時間
    /// [ref: 憲章 IV] T076
    /// </summary>
    /// <param name="operation">要測量的操作</param>
    /// <param name="operationName">操作名稱（用於日誌）</param>
    /// <returns>操作執行時間（毫秒）</returns>
    public static long MeasureOperation(Action operation, string operationName = "Operation")
    {
        var sw = Stopwatch.StartNew();
        operation();
        sw.Stop();

        LastOperationTime = sw.Elapsed;
        var elapsedMs = sw.ElapsedMilliseconds;

#if DEBUG
        Debug.WriteLine($"[PerformanceMonitor] {operationName}：{elapsedMs}ms (上限：{OperationResponseThresholdMs}ms)");

        if (elapsedMs > OperationResponseThresholdMs)
        {
            Debug.WriteLine($"[PerformanceMonitor] ⚠️ 警告：{operationName} 超過 {OperationResponseThresholdMs}ms！");
        }
#endif

        return elapsedMs;
    }

    /// <summary>
    /// 測量操作執行時間（泛型版本）
    /// [ref: 憲章 IV] T076
    /// </summary>
    /// <typeparam name="T">回傳值類型</typeparam>
    /// <param name="operation">要測量的操作</param>
    /// <param name="operationName">操作名稱（用於日誌）</param>
    /// <returns>操作結果</returns>
    public static T MeasureOperation<T>(Func<T> operation, string operationName = "Operation")
    {
        var sw = Stopwatch.StartNew();
        var result = operation();
        sw.Stop();

        LastOperationTime = sw.Elapsed;
        var elapsedMs = sw.ElapsedMilliseconds;

#if DEBUG
        Debug.WriteLine($"[PerformanceMonitor] {operationName}：{elapsedMs}ms (上限：{OperationResponseThresholdMs}ms)");

        if (elapsedMs > OperationResponseThresholdMs)
        {
            Debug.WriteLine($"[PerformanceMonitor] ⚠️ 警告：{operationName} 超過 {OperationResponseThresholdMs}ms！");
        }
#endif

        return result;
    }

    /// <summary>
    /// 驗證操作回應時間是否符合要求
    /// [ref: 憲章 IV] T076
    /// </summary>
    /// <returns>true 如果操作時間 ≤ 100ms</returns>
    public static bool ValidateOperationTime()
    {
        return LastOperationTime.TotalMilliseconds <= OperationResponseThresholdMs;
    }

    /// <summary>
    /// 取得效能摘要
    /// </summary>
    /// <returns>效能摘要字串</returns>
    public static string GetSummary()
    {
        return $"啟動時間：{LastStartupTime.TotalMilliseconds:F0}ms/{StartupTimeThresholdMs}ms, " +
               $"操作回應：{LastOperationTime.TotalMilliseconds:F0}ms/{OperationResponseThresholdMs}ms";
    }
}
