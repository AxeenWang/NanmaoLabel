using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// PDF 輸出介面
/// [ref: raw_spec 7.2, 3.1 F-08]
///
/// 提供標籤 PDF 輸出功能
/// </summary>
public interface IPdfExporter
{
    /// <summary>
    /// 輸出單筆標籤為 PDF
    /// [ref: raw_spec 3.1 F-08]
    ///
    /// 規格：
    /// - 頁面尺寸: 100mm × 60mm [ref: raw_spec 5.1]
    /// - 字型嵌入: 微軟正黑體 Regular + Bold [ref: raw_spec 5.3, 13.2]
    /// - 座標換算: 使用 UnitConverter.MmToPt [ref: raw_spec 13.1]
    /// - 條碼下方文字: 8pt, 置中 [ref: raw_spec 6.1]
    /// - 300 DPI 渲染 [ref: raw_spec 7, 13.17]
    /// </summary>
    /// <param name="template">標籤格式</param>
    /// <param name="record">資料紀錄</param>
    /// <param name="outputPath">輸出檔案路徑（若為 null 則使用預設路徑與檔名）</param>
    /// <returns>實際輸出的檔案路徑</returns>
    string ExportSingle(LabelTemplate template, DataRecord record, string? outputPath = null);

    /// <summary>
    /// 取得預設輸出目錄
    /// [ref: raw_spec 2.3]
    /// </summary>
    string OutputDirectory { get; }

    /// <summary>
    /// 產生預設檔名
    /// [ref: raw_spec 13.6]
    ///
    /// 格式: Label_{ogb19}_{yyyyMMdd_HHmmss}.pdf
    /// </summary>
    /// <param name="record">資料紀錄</param>
    /// <returns>檔名</returns>
    string GenerateDefaultFileName(DataRecord record);

    /// <summary>
    /// 批次輸出多筆標籤為單一多頁 PDF
    /// [ref: raw_spec 3.3 批次輸出規格, TC-12]
    ///
    /// 規格：
    /// - 每頁一張標籤 [ref: raw_spec 3.3]
    /// - 頁面尺寸: 100mm × 60mm [ref: raw_spec 5.1]
    /// - 檔名格式: Labels_Batch_{yyyyMMdd_HHmmss}.pdf [ref: raw_spec 13.6]
    /// </summary>
    /// <param name="template">標籤格式</param>
    /// <param name="records">資料紀錄清單</param>
    /// <param name="outputPath">輸出檔案路徑（若為 null 則使用預設路徑與檔名）</param>
    /// <returns>實際輸出的檔案路徑</returns>
    string ExportBatch(LabelTemplate template, IEnumerable<DataRecord> records, string? outputPath = null);

    /// <summary>
    /// 產生批次輸出預設檔名
    /// [ref: raw_spec 13.6]
    ///
    /// 格式: Labels_Batch_{yyyyMMdd_HHmmss}.pdf
    /// </summary>
    /// <returns>檔名</returns>
    string GenerateBatchFileName();
}
