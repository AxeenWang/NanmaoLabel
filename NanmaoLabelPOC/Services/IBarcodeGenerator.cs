namespace NanmaoLabelPOC.Services;

/// <summary>
/// 條碼生成介面
/// [ref: raw_spec 7.2, 6.1, 6.2]
/// </summary>
public interface IBarcodeGenerator
{
    /// <summary>
    /// 產生 Code 128 一維條碼圖片
    /// [ref: raw_spec 6.1]
    ///
    /// 規格：
    /// - 高度: 10mm (118px @ 300 DPI)
    /// - Quiet Zone: 左右各 10 倍最小單元寬度
    /// - 不含條碼下方文字 (PureBarcode = true)
    /// </summary>
    /// <param name="content">條碼內容 (Raw Value，不含千分位或格式化)</param>
    /// <returns>條碼圖片位元組陣列 (PNG 格式)</returns>
    byte[] GenerateCode128(string content);

    /// <summary>
    /// 產生 QR Code 二維條碼圖片
    /// [ref: raw_spec 6.2]
    ///
    /// 規格：
    /// - 尺寸: 20mm × 20mm (236×236 px @ 300 DPI)
    /// - 容錯等級: Level M (15%)
    /// - 編碼: UTF-8 (支援中文)
    /// - Quiet Zone: 四周各 4 倍單元寬度
    /// </summary>
    /// <param name="content">QR Code 內容 (組合欄位，分號分隔)</param>
    /// <returns>QR Code 圖片位元組陣列 (PNG 格式)</returns>
    byte[] GenerateQRCode(string content);

    /// <summary>
    /// 產生 Code 128 條碼圖片並指定高度
    /// </summary>
    /// <param name="content">條碼內容</param>
    /// <param name="heightMm">條碼高度 (mm)</param>
    /// <returns>條碼圖片位元組陣列 (PNG 格式)</returns>
    byte[] GenerateCode128(string content, double heightMm);

    /// <summary>
    /// 產生 QR Code 圖片並指定尺寸
    /// </summary>
    /// <param name="content">QR Code 內容</param>
    /// <param name="sizeMm">QR Code 尺寸 (mm，正方形)</param>
    /// <returns>QR Code 圖片位元組陣列 (PNG 格式)</returns>
    byte[] GenerateQRCode(string content, double sizeMm);
}
