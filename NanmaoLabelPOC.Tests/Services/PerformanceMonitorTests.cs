using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.Templates;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// 效能監控測試
/// [ref: 憲章 IV]
///
/// T075: 驗證啟動時間 ≤ 3 秒
/// T076: 驗證操作回應 ≤ 100ms
/// </summary>
public class PerformanceMonitorTests
{
    #region T075: 啟動時間驗證測試

    [Fact]
    public void StartupTimeThreshold_ShouldBe3000Ms()
    {
        // Arrange & Act & Assert
        // [ref: 憲章 IV] 應用程式啟動至可操作狀態必須在 3 秒內完成
        Assert.Equal(3000, PerformanceMonitor.StartupTimeThresholdMs);
    }

    [Fact]
    public void StartApplication_ShouldStartTimer()
    {
        // Arrange & Act
        PerformanceMonitor.StartApplication();

        // Assert
        // 能夠呼叫且不拋出例外
        Assert.True(true);
    }

    [Fact]
    public void EndStartup_ShouldReturnElapsedTime()
    {
        // Arrange
        PerformanceMonitor.StartApplication();
        Thread.Sleep(10); // 模擬一些初始化時間

        // Act
        var elapsedMs = PerformanceMonitor.EndStartup();

        // Assert
        Assert.True(elapsedMs >= 10, $"經過時間應至少 10ms，實際：{elapsedMs}ms");
    }

    [Fact]
    public void ValidateStartupTime_WhenWithinThreshold_ShouldReturnTrue()
    {
        // Arrange
        PerformanceMonitor.StartApplication();
        // 立即結束，時間應遠小於 3 秒

        // Act
        PerformanceMonitor.EndStartup();
        var isValid = PerformanceMonitor.ValidateStartupTime();

        // Assert
        Assert.True(isValid, "快速啟動應該通過驗證");
    }

    #endregion

    #region T076: 操作回應時間驗證測試

    [Fact]
    public void OperationResponseThreshold_ShouldBe100Ms()
    {
        // Arrange & Act & Assert
        // [ref: 憲章 IV] 標註操作回應時間必須在 100ms 以內
        Assert.Equal(100, PerformanceMonitor.OperationResponseThresholdMs);
    }

    [Fact]
    public void MeasureOperation_ShouldMeasureExecutionTime()
    {
        // Arrange
        var operationExecuted = false;

        // Act
        var elapsedMs = PerformanceMonitor.MeasureOperation(() =>
        {
            operationExecuted = true;
            Thread.Sleep(10); // 模擬一些處理時間
        }, "測試操作");

        // Assert
        Assert.True(operationExecuted, "操作應該被執行");
        Assert.True(elapsedMs >= 10, $"經過時間應至少 10ms，實際：{elapsedMs}ms");
    }

    [Fact]
    public void MeasureOperationGeneric_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = "測試結果";

        // Act
        var result = PerformanceMonitor.MeasureOperation(() =>
        {
            return expectedResult;
        }, "測試操作");

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ValidateOperationTime_WhenWithinThreshold_ShouldReturnTrue()
    {
        // Arrange & Act
        PerformanceMonitor.MeasureOperation(() =>
        {
            // 快速操作，應在 100ms 內
        }, "快速操作");

        var isValid = PerformanceMonitor.ValidateOperationTime();

        // Assert
        Assert.True(isValid, "快速操作應該通過驗證");
    }

    [Fact]
    public void MeasureOperation_WithSlowOperation_ShouldStillComplete()
    {
        // Arrange & Act
        var elapsedMs = PerformanceMonitor.MeasureOperation(() =>
        {
            // 模擬較慢的操作（超過 100ms）
            Thread.Sleep(150);
        }, "慢速操作");

        // Assert
        Assert.True(elapsedMs >= 150, $"經過時間應至少 150ms，實際：{elapsedMs}ms");
        // 驗證：操作時間超過閾值
        Assert.False(PerformanceMonitor.ValidateOperationTime(),
            "超過 100ms 的操作不應通過驗證");
    }

    #endregion

    #region GetSummary 測試

    [Fact]
    public void GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        PerformanceMonitor.StartApplication();
        PerformanceMonitor.EndStartup();
        PerformanceMonitor.MeasureOperation(() => { }, "測試");

        // Act
        var summary = PerformanceMonitor.GetSummary();

        // Assert
        Assert.Contains("啟動時間", summary);
        Assert.Contains("操作回應", summary);
        Assert.Contains($"{PerformanceMonitor.StartupTimeThresholdMs}ms", summary);
        Assert.Contains($"{PerformanceMonitor.OperationResponseThresholdMs}ms", summary);
    }

    #endregion

    #region 整合驗證：模擬核心服務操作時間

    [Fact]
    public void DataStore_Load_ShouldCompleteWithin100Ms()
    {
        // Arrange
        var dataStore = new DataStore();

        // Act
        long elapsedMs = 0;
        PerformanceMonitor.MeasureOperation(
            () =>
            {
                dataStore.Load();
            },
            "DataStore.Load");
        elapsedMs = (long)PerformanceMonitor.LastOperationTime.TotalMilliseconds;

        // Assert
        // [ref: 憲章 IV] 操作回應時間必須在 100ms 以內
        Assert.True(elapsedMs <= PerformanceMonitor.OperationResponseThresholdMs,
            $"DataStore.Load 應在 {PerformanceMonitor.OperationResponseThresholdMs}ms 內完成，實際：{elapsedMs}ms");
    }

    [Fact]
    public void LabelRenderer_Render_ShouldCompleteWithin100Ms()
    {
        // Arrange
        var renderer = new LabelRenderer();
        var template = Templates.BuiltInTemplates.GetByCode("QW075551-1")!;
        var record = new Models.DataRecord
        {
            Id = Guid.NewGuid().ToString(),
            Ogb03 = "TEST",
            Ogb19 = "G25A111577",
            Ogb092 = "20250126",
            Ogb905 = "INV001",
            Ogd12b = "20250101",
            Ogd12e = "20251231",
            Ima902 = "CUST-001",
            Ogd15 = "Part-A",
            Ogd09 = "1000",
            Obe25 = "20250126",
            NvrCust = "測試客戶",
            NvrCustItemNo = "ITEM-001",
            NvrCustPn = "PN-12345",
            NvrRemark10 = "備註",
            Pono = "PO-001",
            Erpmat = "MAT-001",
            Cscustpo = "CUSTPO-001"
        };

        // Act
        PerformanceMonitor.MeasureOperation(
            () => renderer.Render(template, record),
            "LabelRenderer.Render");
        var elapsedMs = (long)PerformanceMonitor.LastOperationTime.TotalMilliseconds;

        // Assert
        // [ref: 憲章 IV] 操作回應時間必須在 100ms 以內
        Assert.True(elapsedMs <= PerformanceMonitor.OperationResponseThresholdMs,
            $"LabelRenderer.Render 應在 {PerformanceMonitor.OperationResponseThresholdMs}ms 內完成，實際：{elapsedMs}ms");
    }

    #endregion
}
