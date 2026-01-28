namespace NanmaoLabelPOC.Models;

/// <summary>
/// 欄位類型列舉
/// [ref: raw_spec 4.1, data-model.md 3. LabelField]
/// </summary>
public enum FieldType
{
    /// <summary>
    /// 純文字 - 支援裁切 (Ellipsis)、千分位顯示
    /// </summary>
    Text,

    /// <summary>
    /// 一維條碼 - Code 128，禁止裁切 [ref: IC-007]
    /// </summary>
    Barcode,

    /// <summary>
    /// 二維條碼 - QR Code Level M，禁止裁切 [ref: IC-007]
    /// </summary>
    QRCode
}

/// <summary>
/// 文字對齊方式
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// 標籤欄位定義，描述單一欄位的渲染屬性
/// [ref: raw_spec 4.1, 4.2, data-model.md 3. LabelField]
/// </summary>
public class LabelField
{
    /// <summary>
    /// 欄位識別名稱（如 "CSCUSTOMER", "QRCODE"）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 欄位類型（Text, Barcode, QRCode）
    /// </summary>
    public FieldType FieldType { get; set; } = FieldType.Text;

    /// <summary>
    /// 資料來源 - 欄位代碼或常數值
    /// </summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// 是否為常數 - true = 直接輸出 DataSource 值
    /// </summary>
    public bool IsConstant { get; set; }

    /// <summary>
    /// 組合模式 - QR Code 用，如 "{pono};{ima902}"
    /// </summary>
    public string? CombinePattern { get; set; }

    /// <summary>
    /// 是否使用 Display Value（千分位格式化）
    /// [ref: raw_spec 13.13]
    /// </summary>
    public bool UseDisplayValue { get; set; }

    /// <summary>
    /// X 座標 (mm) - 原點為左上角
    /// [ref: raw_spec 13.1]
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y 座標 (mm)
    /// [ref: raw_spec 13.1]
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// 寬度 (mm)
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// 高度 (mm)
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// 字型大小 (pt) - 僅 Text 類型使用
    /// </summary>
    public double? FontSize { get; set; }

    /// <summary>
    /// 是否粗體
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// 對齊方式
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
}
