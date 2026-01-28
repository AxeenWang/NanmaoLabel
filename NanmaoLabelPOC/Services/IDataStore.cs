using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 資料儲存介面，負責 data.json 的讀寫操作
/// [ref: raw_spec 7.2, 3.1 F-01~F-02, 13.5]
/// </summary>
public interface IDataStore
{
    /// <summary>
    /// 載入資料
    /// 程式啟動時自動載入 data.json [ref: raw_spec 13.5]
    /// </summary>
    /// <returns>所有資料紀錄</returns>
    /// <exception cref="InvalidOperationException">data.json 格式錯誤時擲出</exception>
    IReadOnlyList<DataRecord> Load();

    /// <summary>
    /// 儲存資料（覆蓋模式）
    /// [ref: raw_spec 13.5]
    /// - 整份取代現有資料
    /// - 更新 lastModified 為 ISO 8601 格式
    /// - 自動為新紀錄產生 UUID
    /// </summary>
    /// <param name="records">要儲存的資料紀錄</param>
    void Save(IEnumerable<DataRecord> records);

    /// <summary>
    /// 取得所有資料紀錄
    /// data.json 為唯一資料來源 (Single Source of Truth) [ref: raw_spec 13.25]
    /// </summary>
    /// <returns>所有資料紀錄</returns>
    IReadOnlyList<DataRecord> GetAll();

    /// <summary>
    /// 取得資料檔案路徑
    /// [ref: raw_spec 2.3]
    /// </summary>
    string DataFilePath { get; }

    /// <summary>
    /// 檢查資料檔案是否存在
    /// </summary>
    bool DataFileExists { get; }
}
