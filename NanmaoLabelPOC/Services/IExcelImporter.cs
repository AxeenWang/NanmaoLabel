using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

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
    /// 警告訊息清單（分號警告、欄位缺失等）[ref: raw_spec 3.3, 13.4]
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 匯入筆數
    /// </summary>
    public int RecordCount => Records.Count;
}
