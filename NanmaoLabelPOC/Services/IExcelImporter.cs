using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 訊息嚴重程度等級
/// [ref: spec.md FR-003]
/// </summary>
public enum MessageSeverity
{
    /// <summary>
    /// 錯誤：匯入失敗，無法繼續
    /// </summary>
    Error,

    /// <summary>
    /// 警告：匯入成功，但部分資料可能不完整
    /// </summary>
    Warning,

    /// <summary>
    /// 資訊：提示資訊，不影響結果
    /// </summary>
    Info
}

/// <summary>
/// 匯入過程中產生的單一訊息
/// [ref: spec.md FR-003, FR-009]
/// </summary>
public class ImportMessage
{
    /// <summary>
    /// 訊息等級
    /// </summary>
    public MessageSeverity Severity { get; init; }

    /// <summary>
    /// 訊息內容（繁體中文）
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 關聯的欄位名稱（可選）
    /// </summary>
    public string? FieldName { get; init; }
}

/// <summary>
/// Excel 匯入介面
/// [ref: raw_spec 7.2, 3.1 F-01, 3.3]
/// </summary>
public interface IExcelImporter
{
    /// <summary>
    /// 從 Excel 檔案匯入資料
    /// [ref: raw_spec 3.1 F-01, 3.3]
    ///
    /// 匯入規則：
    /// - 首列為欄位名稱
    /// - 欄位名稱不分大小寫 [ref: raw_spec 13.11]
    /// - 所有內容視為字串（保留前導零）[ref: raw_spec 3.1 備註]
    /// - 欄位值 Trim 前後空白 [ref: raw_spec 3.3]
    /// - 日期轉換為 yyyy-MM-dd [ref: raw_spec 3.3, 13.2]
    /// </summary>
    /// <param name="filePath">Excel 檔案路徑 (*.xlsx)</param>
    /// <returns>匯入結果</returns>
    ImportResult Import(string filePath);
}

/// <summary>
/// 匯入結果
/// </summary>
public class ImportResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 匯入的資料紀錄
    /// </summary>
    public List<DataRecord> Records { get; set; } = new();

    /// <summary>
    /// 錯誤訊息 [ref: raw_spec 8.9, 13.21]
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 結構化訊息清單（所有等級）
    /// [ref: spec.md FR-007, FR-008]
    /// </summary>
    public List<ImportMessage> Messages { get; set; } = new();

    /// <summary>
    /// 警告訊息清單（分號警告、欄位缺失等）[ref: raw_spec 3.3, 13.4]
    /// </summary>
    [Obsolete("Use Messages property instead")]
    public List<string> Warnings
    {
        get => Messages
            .Where(m => m.Severity != MessageSeverity.Info)
            .Select(m => m.Message)
            .ToList();
        set
        {
            // 向後相容：將舊式 string 警告轉為 Warning 等級的 ImportMessage
            foreach (var warning in value)
            {
                Messages.Add(new ImportMessage
                {
                    Severity = MessageSeverity.Warning,
                    Message = warning
                });
            }
        }
    }

    /// <summary>
    /// 匯入筆數
    /// </summary>
    public int RecordCount => Records.Count;

    /// <summary>
    /// Error 等級訊息數量
    /// [ref: spec.md FR-008]
    /// </summary>
    public int ErrorCount => Messages.Count(m => m.Severity == MessageSeverity.Error);

    /// <summary>
    /// Warning 等級訊息數量
    /// [ref: spec.md FR-008]
    /// </summary>
    public int WarningCount => Messages.Count(m => m.Severity == MessageSeverity.Warning);

    /// <summary>
    /// Info 等級訊息數量
    /// [ref: spec.md FR-008]
    /// </summary>
    public int InfoCount => Messages.Count(m => m.Severity == MessageSeverity.Info);

    /// <summary>
    /// 是否有 Error 等級訊息
    /// </summary>
    public bool HasErrors => ErrorCount > 0;

    /// <summary>
    /// 是否有 Warning 等級訊息
    /// </summary>
    public bool HasWarnings => WarningCount > 0;

    /// <summary>
    /// 是否有 Info 等級訊息
    /// </summary>
    public bool HasInfos => InfoCount > 0;
}
