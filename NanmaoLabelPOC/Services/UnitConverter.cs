namespace NanmaoLabelPOC.Services;

/// <summary>
/// 座標單位換算靜態類別
/// [ref: raw_spec 13.1, IC-001~003, research.md 5. mm → pt 渲染一致性]
///
/// 換算公式（固定，不得各處各自實作）:
/// - mm → pt (PDF): mm × 72 ÷ 25.4
/// - mm → px @ 300 DPI: mm × 300 ÷ 25.4
/// - mm → WPF 邏輯單位: mm × 96 ÷ 25.4
/// </summary>
public static class UnitConverter
{
    /// <summary>
    /// 1 英吋 = 25.4 mm
    /// </summary>
    private const double MmPerInch = 25.4;

    /// <summary>
    /// PDF 使用 72 pt/inch
    /// </summary>
    private const double PtPerInch = 72.0;

    /// <summary>
    /// 條碼生成使用 300 DPI
    /// [ref: raw_spec 13.17]
    /// </summary>
    private const double Dpi300 = 300.0;

    /// <summary>
    /// WPF 使用 96 DPI 邏輯單位
    /// </summary>
    private const double WpfDpi = 96.0;

    /// <summary>
    /// mm 轉換為 pt（用於 PDF 輸出）
    /// 公式: mm × 72 ÷ 25.4
    /// 範例: 100mm = 283.46pt
    /// </summary>
    /// <param name="mm">毫米</param>
    /// <returns>點 (pt)</returns>
    public static double MmToPt(double mm)
    {
        return mm * PtPerInch / MmPerInch;
    }

    /// <summary>
    /// mm 轉換為像素（用於 300 DPI 條碼生成）
    /// 公式: mm × 300 ÷ 25.4
    /// 範例: 100mm = 1181.1px, 10mm = 118px, 20mm = 236px
    /// [ref: raw_spec 13.17]
    /// </summary>
    /// <param name="mm">毫米</param>
    /// <returns>像素 (px @ 300 DPI)</returns>
    public static double MmToPx300(double mm)
    {
        return mm * Dpi300 / MmPerInch;
    }

    /// <summary>
    /// mm 轉換為像素並四捨五入為整數（用於條碼生成）
    /// </summary>
    /// <param name="mm">毫米</param>
    /// <returns>整數像素 (px @ 300 DPI)</returns>
    public static int MmToPx300Int(double mm)
    {
        return (int)Math.Round(MmToPx300(mm));
    }

    /// <summary>
    /// mm 轉換為 WPF 邏輯單位（用於預覽）
    /// 公式: mm × 96 ÷ 25.4
    /// 範例: 100mm = 377.95
    /// </summary>
    /// <param name="mm">毫米</param>
    /// <returns>WPF 邏輯單位</returns>
    public static double MmToWpf(double mm)
    {
        return mm * WpfDpi / MmPerInch;
    }

    /// <summary>
    /// pt 轉換為 mm（逆向換算，較少使用）
    /// </summary>
    /// <param name="pt">點</param>
    /// <returns>毫米</returns>
    public static double PtToMm(double pt)
    {
        return pt * MmPerInch / PtPerInch;
    }

    /// <summary>
    /// 標籤寬度 100mm 對應的 pt 值
    /// [ref: raw_spec 5.1]
    /// </summary>
    public static double LabelWidthPt => MmToPt(100);

    /// <summary>
    /// 標籤高度 60mm 對應的 pt 值
    /// [ref: raw_spec 5.1]
    /// </summary>
    public static double LabelHeightPt => MmToPt(60);

    /// <summary>
    /// Code 128 條碼高度 10mm 對應的像素值 (118px @ 300 DPI)
    /// [ref: raw_spec 6.1, 13.17]
    /// </summary>
    public static int BarcodeHeightPx => MmToPx300Int(10);

    /// <summary>
    /// QR Code 尺寸 20mm 對應的像素值 (236px @ 300 DPI)
    /// [ref: raw_spec 6.2, 13.17]
    /// </summary>
    public static int QrCodeSizePx => MmToPx300Int(20);
}
