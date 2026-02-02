namespace NanmaoLabelPOC.Models;

/// <summary>
/// 標籤格式定義，POC 階段內建兩種格式
/// [ref: raw_spec 5.1, 5.2, data-model.md 2. LabelTemplate]
/// </summary>
public class LabelTemplate
{
    /// <summary>
    /// 標籤代碼（如 "QW075551-1", "QW075551-2"）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 標籤名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 標籤寬度 (mm)
    /// [ref: raw_spec 5.1 - 100mm]
    /// </summary>
    public double WidthMm { get; set; } = 100;

    /// <summary>
    /// 標籤高度 (mm)
    /// [ref: raw_spec 5.1 - 60mm]
    /// </summary>
    public double HeightMm { get; set; } = 60;

    /// <summary>
    /// 欄位清單
    /// </summary>
    public List<LabelField> Fields { get; set; } = new();

    /// <summary>
    /// 是否繪製標籤外框
    /// [ref: FR-003]
    /// </summary>
    public bool HasBorder { get; set; } = false;

    /// <summary>
    /// 取得顯示用名稱（代碼 + 名稱）
    /// </summary>
    public string DisplayName => $"{Code} - {Name}";
}
