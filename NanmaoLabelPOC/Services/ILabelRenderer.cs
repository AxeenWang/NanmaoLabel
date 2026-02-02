using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 標籤渲染介面
/// [ref: raw_spec 7.2, 2.5, 4.2]
///
/// 提供標籤渲染指令產生，供預覽與 PDF 輸出共用
/// </summary>
public interface ILabelRenderer
{
    /// <summary>
    /// 渲染標籤，產生渲染指令集合
    /// [ref: raw_spec 2.5]
    ///
    /// 處理規則：
    /// - 變數對應：從 DataRecord 取值 [ref: raw_spec 4.2]
    /// - 常數值：直接輸出 DataSource [ref: raw_spec 4.2]
    /// - Raw Value：用於 Barcode/QRCode [ref: raw_spec 13.13]
    /// - Display Value：用於 Text（數量加千分位）[ref: raw_spec 13.13]
    /// - Text 溢出：Ellipsis + 裁切 [ref: raw_spec 13.2]
    /// - Barcode/QRCode：禁止裁切 [ref: raw_spec 13.9]
    /// - QR Code 組合：空值保留分號位置 (A;;C) [ref: raw_spec 13.4, 13.15]
    /// </summary>
    /// <param name="template">標籤格式定義</param>
    /// <param name="record">資料紀錄</param>
    /// <returns>渲染指令集合</returns>
    IReadOnlyList<RenderCommand> Render(LabelTemplate template, DataRecord record);

    /// <summary>
    /// 驗證資料紀錄是否具備必要欄位
    /// [ref: raw_spec 3.3, 8.9]
    ///
    /// 檢查 Barcode 欄位的資料來源是否有值
    /// </summary>
    /// <param name="template">標籤格式定義</param>
    /// <param name="record">資料紀錄</param>
    /// <returns>缺失的必要欄位名稱清單，若為空則表示通過驗證</returns>
    IReadOnlyList<string> ValidateRequiredFields(LabelTemplate template, DataRecord record);
}

/// <summary>
/// 渲染指令類型
/// </summary>
public enum RenderCommandType
{
    /// <summary>
    /// 文字渲染
    /// </summary>
    Text,

    /// <summary>
    /// 條碼渲染 (Code 128)
    /// </summary>
    Barcode,

    /// <summary>
    /// QR Code 渲染
    /// </summary>
    QRCode
}

/// <summary>
/// 渲染指令，描述單一元素的渲染資訊
/// </summary>
public class RenderCommand
{
    /// <summary>
    /// 指令類型
    /// </summary>
    public RenderCommandType CommandType { get; init; }

    /// <summary>
    /// 渲染內容（文字內容或條碼編碼內容）
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// X 座標 (mm)
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Y 座標 (mm)
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// 寬度 (mm)
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// 高度 (mm)
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// 字型大小 (pt) - 僅 Text 類型使用
    /// </summary>
    public double? FontSize { get; init; }

    /// <summary>
    /// 是否粗體 - 僅 Text 類型使用
    /// </summary>
    public bool IsBold { get; init; }

    /// <summary>
    /// 對齊方式 - 僅 Text 類型使用
    /// </summary>
    public TextAlignment Alignment { get; init; }

    /// <summary>
    /// 欄位名稱（用於除錯與追蹤）
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// 是否略過此渲染（如條碼內容為空）
    /// [ref: raw_spec 13.15]
    /// </summary>
    public bool Skip { get; init; }

    /// <summary>
    /// 計算後的實際字體大小（經過縮小處理）
    /// [ref: FR-008]
    /// </summary>
    public double? ActualFontSize { get; init; }

    /// <summary>
    /// 是否需要換行
    /// [ref: FR-008]
    /// </summary>
    public bool RequiresWrap { get; init; }
}
