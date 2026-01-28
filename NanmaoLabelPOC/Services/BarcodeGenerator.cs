using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 條碼生成實作
/// [ref: raw_spec 7.2, 6.1, 6.2, 13.9, 13.16, 13.17]
///
/// 重要約束：
/// - 條碼內容禁止裁切、縮字、Ellipsis [ref: raw_spec 13.9]
/// - 條碼尺寸不得縮放至影響掃描 [ref: raw_spec 13.16]
/// - 條碼生成 DPI: 300 [ref: raw_spec 13.17]
/// </summary>
public class BarcodeGenerator : IBarcodeGenerator
{
    /// <summary>
    /// Code 128 預設高度 (mm) [ref: raw_spec 6.1]
    /// </summary>
    private const double DefaultBarcodeHeightMm = 10.0;

    /// <summary>
    /// QR Code 預設尺寸 (mm) [ref: raw_spec 6.2]
    /// </summary>
    private const double DefaultQrCodeSizeMm = 20.0;

    /// <summary>
    /// Code 128 Quiet Zone (像素，10 倍最小單元寬度) [ref: raw_spec 6.1]
    /// </summary>
    private const int Code128Margin = 10;

    /// <summary>
    /// QR Code Quiet Zone (單元數，四周各 4 倍) [ref: raw_spec 6.2]
    /// </summary>
    private const int QrCodeMargin = 4;

    /// <summary>
    /// Code 128 預設寬度 (像素)
    /// 根據內容長度動態調整，此為最小寬度
    /// </summary>
    private const int Code128MinWidth = 200;

    /// <inheritdoc />
    public byte[] GenerateCode128(string content)
    {
        return GenerateCode128(content, DefaultBarcodeHeightMm);
    }

    /// <inheritdoc />
    public byte[] GenerateCode128(string content, double heightMm)
    {
        // 空內容時回傳空陣列（不產生條碼）[ref: raw_spec 13.15]
        if (string.IsNullOrEmpty(content))
        {
            return Array.Empty<byte>();
        }

        // 計算高度像素 [ref: raw_spec 13.17]
        var heightPx = UnitConverter.MmToPx300Int(heightMm);

        // 動態計算寬度（根據內容長度）
        // Code 128 每字元約需 11 個模組，加上起始/結束碼
        var estimatedWidth = Math.Max(Code128MinWidth, (content.Length + 4) * 11);

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = estimatedWidth,
                Height = heightPx,
                Margin = Code128Margin,        // Quiet Zone [ref: raw_spec 6.1]
                PureBarcode = true             // 不含文字，由渲染引擎獨立繪製 [ref: research.md]
            }
        };

        using var bitmap = writer.Write(content);
        return ConvertBitmapToPng(bitmap);
    }

    /// <inheritdoc />
    public byte[] GenerateQRCode(string content)
    {
        return GenerateQRCode(content, DefaultQrCodeSizeMm);
    }

    /// <inheritdoc />
    public byte[] GenerateQRCode(string content, double sizeMm)
    {
        // 空內容時回傳空陣列（不產生 QR Code）[ref: raw_spec 13.15]
        if (string.IsNullOrEmpty(content))
        {
            return Array.Empty<byte>();
        }

        // 計算尺寸像素 [ref: raw_spec 13.17]
        var sizePx = UnitConverter.MmToPx300Int(sizeMm);

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Width = sizePx,
                Height = sizePx,
                Margin = QrCodeMargin,                        // Quiet Zone [ref: raw_spec 6.2]
                ErrorCorrection = ErrorCorrectionLevel.M,     // Level M (15%) [ref: raw_spec 6.2]
                CharacterSet = "UTF-8"                        // 支援中文 [ref: raw_spec 6.2]
            }
        };

        using var bitmap = writer.Write(content);
        return ConvertBitmapToPng(bitmap);
    }

    /// <summary>
    /// 將 Bitmap 轉換為 PNG 位元組陣列
    /// </summary>
    private static byte[] ConvertBitmapToPng(System.Drawing.Bitmap bitmap)
    {
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
        return memoryStream.ToArray();
    }
}
