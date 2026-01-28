using System.Globalization;
using System.Text.Json.Serialization;

namespace NanmaoLabelPOC.Models;

/// <summary>
/// 交易資料紀錄，對應 Excel 匯入之單筆資料
/// [ref: raw_spec 附錄 B.2, data-model.md 1. DataRecord]
/// </summary>
public class DataRecord
{
    /// <summary>
    /// 紀錄唯一識別碼 (UUID v4，系統自動生成)
    /// [ref: raw_spec 附錄 B.2, 13.13]
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 訂單編號 [ref: raw_spec 附錄 A]
    /// </summary>
    [JsonPropertyName("ogb03")]
    public string Ogb03 { get; set; } = string.Empty;

    /// <summary>
    /// 單據編號 - 業務主鍵，用於 PDF 檔名
    /// </summary>
    [JsonPropertyName("ogb19")]
    public string Ogb19 { get; set; } = string.Empty;

    /// <summary>
    /// 訂單日期
    /// </summary>
    [JsonPropertyName("ogb092")]
    public string Ogb092 { get; set; } = string.Empty;

    /// <summary>
    /// 預計交貨日
    /// </summary>
    [JsonPropertyName("ogb905")]
    public string Ogb905 { get; set; } = string.Empty;

    /// <summary>
    /// 出貨起始日
    /// </summary>
    [JsonPropertyName("ogd12b")]
    public string Ogd12b { get; set; } = string.Empty;

    /// <summary>
    /// 出貨結束日
    /// </summary>
    [JsonPropertyName("ogd12e")]
    public string Ogd12e { get; set; } = string.Empty;

    /// <summary>
    /// 裝置編號 - QR Code 組合欄位
    /// </summary>
    [JsonPropertyName("ima902")]
    public string Ima902 { get; set; } = string.Empty;

    /// <summary>
    /// 產品說明
    /// </summary>
    [JsonPropertyName("ogd15")]
    public string Ogd15 { get; set; } = string.Empty;

    /// <summary>
    /// 數量 - 儲存為原始數值（不含千分位）
    /// [ref: raw_spec 13.13, IC-005~006]
    /// </summary>
    [JsonPropertyName("ogd09")]
    public string Ogd09 { get; set; } = string.Empty;

    /// <summary>
    /// 日期 - 格式 yyyy-MM-dd
    /// </summary>
    [JsonPropertyName("obe25")]
    public string Obe25 { get; set; } = string.Empty;

    /// <summary>
    /// 客戶名稱 - QW075551-1 CSCUSTOMER
    /// </summary>
    [JsonPropertyName("nvr_cust")]
    public string NvrCust { get; set; } = string.Empty;

    /// <summary>
    /// 客戶料號 - CSCUSTITEMNO
    /// </summary>
    [JsonPropertyName("nvr_cust_item_no")]
    public string NvrCustItemNo { get; set; } = string.Empty;

    /// <summary>
    /// 客戶 P/N - Code 128 條碼來源
    /// </summary>
    [JsonPropertyName("nvr_cust_pn")]
    public string NvrCustPn { get; set; } = string.Empty;

    /// <summary>
    /// 備註 - 可為空
    /// </summary>
    [JsonPropertyName("nvr_remark10")]
    public string NvrRemark10 { get; set; } = string.Empty;

    /// <summary>
    /// 製令單號 - QR Code 組合欄位
    /// </summary>
    [JsonPropertyName("pono")]
    public string Pono { get; set; } = string.Empty;

    /// <summary>
    /// ERP 料號 - QW075551-2 條碼來源
    /// </summary>
    [JsonPropertyName("erpmat")]
    public string Erpmat { get; set; } = string.Empty;

    /// <summary>
    /// 客戶採購單號 - QW075551-2 用
    /// </summary>
    [JsonPropertyName("cscustpo")]
    public string Cscustpo { get; set; } = string.Empty;

    /// <summary>
    /// 取得 Raw Value（用於 Barcode/QRCode 編碼）
    /// [ref: raw_spec 13.13, IC-005]
    /// </summary>
    /// <param name="fieldName">欄位名稱（不區分大小寫）</param>
    /// <returns>原始值，已 Trim</returns>
    public string GetRawValue(string fieldName)
    {
        var value = GetPropertyValue(fieldName);
        return value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// 取得 Display Value（用於畫面顯示、標籤文字）
    /// [ref: raw_spec 13.13, IC-006]
    /// </summary>
    /// <param name="fieldName">欄位名稱（不區分大小寫）</param>
    /// <returns>格式化後的顯示值</returns>
    public string GetDisplayValue(string fieldName)
    {
        var rawValue = GetRawValue(fieldName);

        return fieldName.ToLowerInvariant() switch
        {
            "ogd09" => FormatQuantity(rawValue),  // 加千分位
            "obe25" => rawValue,  // 已是 yyyy-MM-dd
            _ => rawValue
        };
    }

    /// <summary>
    /// 根據欄位名稱取得屬性值
    /// </summary>
    private string? GetPropertyValue(string fieldName)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "id" => Id,
            "ogb03" => Ogb03,
            "ogb19" => Ogb19,
            "ogb092" => Ogb092,
            "ogb905" => Ogb905,
            "ogd12b" => Ogd12b,
            "ogd12e" => Ogd12e,
            "ima902" => Ima902,
            "ogd15" => Ogd15,
            "ogd09" => Ogd09,
            "obe25" => Obe25,
            "nvr_cust" => NvrCust,
            "nvr_cust_item_no" => NvrCustItemNo,
            "nvr_cust_pn" => NvrCustPn,
            "nvr_remark10" => NvrRemark10,
            "pono" => Pono,
            "erpmat" => Erpmat,
            "cscustpo" => Cscustpo,
            _ => null
        };
    }

    /// <summary>
    /// 格式化數量欄位（加千分位）
    /// </summary>
    private static string FormatQuantity(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return string.Empty;

        if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return number.ToString("N0", CultureInfo.InvariantCulture);

        return rawValue;
    }
}
