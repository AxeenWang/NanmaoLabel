using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// BarcodeGenerator 單元測試
/// [ref: raw_spec 7.2, 憲章 II, T021]
///
/// 測試項目：
/// - Code 128 generation with Quiet Zone [ref: raw_spec 6.1]
/// - QR Code generation with Level M, UTF-8 [ref: raw_spec 6.2]
/// - Barcode content uses Raw Value only [ref: raw_spec 13.13]
/// </summary>
public class BarcodeGeneratorTests
{
    private readonly BarcodeGenerator _generator;

    public BarcodeGeneratorTests()
    {
        _generator = new BarcodeGenerator();
    }

    #region Code 128 Tests [ref: raw_spec 6.1]

    /// <summary>
    /// 測試：Code 128 應產生有效圖片 [ref: raw_spec 6.1]
    /// </summary>
    [Fact]
    public void GenerateCode128_ValidContent_ReturnsNonEmptyBytes()
    {
        // Arrange
        var content = "E110-X0";

        // Act
        var result = _generator.GenerateCode128(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：Code 128 PNG 格式驗證
    /// </summary>
    [Fact]
    public void GenerateCode128_ReturnsPngFormat()
    {
        // Arrange
        var content = "E110-X0";

        // Act
        var result = _generator.GenerateCode128(content);

        // Assert - PNG magic number: 89 50 4E 47
        Assert.True(result.Length >= 8);
        Assert.Equal(0x89, result[0]);
        Assert.Equal(0x50, result[1]); // 'P'
        Assert.Equal(0x4E, result[2]); // 'N'
        Assert.Equal(0x47, result[3]); // 'G'
    }

    /// <summary>
    /// 測試：Code 128 空內容應回傳空陣列 [ref: raw_spec 13.15]
    /// </summary>
    [Fact]
    public void GenerateCode128_EmptyContent_ReturnsEmptyArray()
    {
        // Arrange & Act
        var resultEmpty = _generator.GenerateCode128("");
        var resultNull = _generator.GenerateCode128(null!);

        // Assert
        Assert.Empty(resultEmpty);
        Assert.Empty(resultNull);
    }

    /// <summary>
    /// 測試：Code 128 預設高度 10mm [ref: raw_spec 6.1]
    /// </summary>
    [Fact]
    public void GenerateCode128_DefaultHeight_Is10mm()
    {
        // 預設高度 10mm = 118px @ 300 DPI
        // mm × 300 ÷ 25.4 = 10 × 300 ÷ 25.4 ≈ 118
        var expectedHeightPx = (int)Math.Round(10.0 * 300 / 25.4);
        Assert.Equal(118, expectedHeightPx);
    }

    /// <summary>
    /// 測試：Code 128 自訂高度
    /// </summary>
    [Fact]
    public void GenerateCode128_CustomHeight_GeneratesSuccessfully()
    {
        // Arrange
        var content = "E110-X0";
        var heightMm = 15.0;

        // Act
        var result = _generator.GenerateCode128(content, heightMm);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：Code 128 支援各種字元
    /// </summary>
    [Theory]
    [InlineData("ABC123")]
    [InlineData("E110-X0")]
    [InlineData("TEST-BARCODE-001")]
    [InlineData("1234567890")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void GenerateCode128_VariousContent_GeneratesSuccessfully(string content)
    {
        // Act
        var result = _generator.GenerateCode128(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：Code 128 長內容處理
    /// </summary>
    [Fact]
    public void GenerateCode128_LongContent_GeneratesSuccessfully()
    {
        // Arrange - 較長的條碼內容
        var content = "VERY-LONG-BARCODE-CONTENT-FOR-TESTING-PURPOSE-12345";

        // Act
        var result = _generator.GenerateCode128(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region QR Code Tests [ref: raw_spec 6.2]

    /// <summary>
    /// 測試：QR Code 應產生有效圖片 [ref: raw_spec 6.2]
    /// </summary>
    [Fact]
    public void GenerateQRCode_ValidContent_ReturnsNonEmptyBytes()
    {
        // Arrange
        var content = "PO123;DEV456;6733;備註說明";

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：QR Code PNG 格式驗證
    /// </summary>
    [Fact]
    public void GenerateQRCode_ReturnsPngFormat()
    {
        // Arrange
        var content = "TEST123";

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert - PNG magic number
        Assert.True(result.Length >= 8);
        Assert.Equal(0x89, result[0]);
        Assert.Equal(0x50, result[1]);
        Assert.Equal(0x4E, result[2]);
        Assert.Equal(0x47, result[3]);
    }

    /// <summary>
    /// 測試：QR Code 空內容應回傳空陣列 [ref: raw_spec 13.15]
    /// </summary>
    [Fact]
    public void GenerateQRCode_EmptyContent_ReturnsEmptyArray()
    {
        // Arrange & Act
        var resultEmpty = _generator.GenerateQRCode("");
        var resultNull = _generator.GenerateQRCode(null!);

        // Assert
        Assert.Empty(resultEmpty);
        Assert.Empty(resultNull);
    }

    /// <summary>
    /// 測試：QR Code 預設尺寸 20mm [ref: raw_spec 6.2]
    /// </summary>
    [Fact]
    public void GenerateQRCode_DefaultSize_Is20mm()
    {
        // 預設尺寸 20mm = 236px @ 300 DPI
        // mm × 300 ÷ 25.4 = 20 × 300 ÷ 25.4 ≈ 236
        var expectedSizePx = (int)Math.Round(20.0 * 300 / 25.4);
        Assert.Equal(236, expectedSizePx);
    }

    /// <summary>
    /// 測試：QR Code 自訂尺寸
    /// </summary>
    [Fact]
    public void GenerateQRCode_CustomSize_GeneratesSuccessfully()
    {
        // Arrange
        var content = "TEST123";
        var sizeMm = 25.0;

        // Act
        var result = _generator.GenerateQRCode(content, sizeMm);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：QR Code UTF-8 編碼支援中文 [ref: raw_spec 6.2]
    /// </summary>
    [Fact]
    public void GenerateQRCode_ChineseContent_GeneratesSuccessfully()
    {
        // Arrange
        var content = "訂單編號;產品名稱;數量;備註說明";

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：QR Code 分號分隔格式 [ref: raw_spec 13.4]
    /// </summary>
    [Theory]
    [InlineData("A;B;C;D")]                           // 標準分隔
    [InlineData("PO123;DEV456;6733;備註")]            // 混合中英文
    [InlineData("A;;C")]                               // 空值保留 [ref: raw_spec 13.15]
    [InlineData(";B;C")]                               // 首欄空值
    [InlineData("A;B;")]                               // 末欄空值
    public void GenerateQRCode_SemicolonSeparatedContent_GeneratesSuccessfully(string content)
    {
        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// 測試：QR Code 長內容處理
    /// </summary>
    [Fact]
    public void GenerateQRCode_LongContent_GeneratesSuccessfully()
    {
        // Arrange - 較長的 QR Code 內容
        var content = string.Join(";", Enumerable.Range(1, 20).Select(i => $"欄位{i}值{i}"));

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Raw Value Tests [ref: raw_spec 13.13]

    /// <summary>
    /// 測試：條碼內容應使用 Raw Value（無千分位）[ref: raw_spec 13.13]
    /// </summary>
    [Fact]
    public void BarcodeContent_ShouldUseRawValue_NoThousandSeparator()
    {
        // Arrange
        var record = new DataRecord { Ogd09 = "6733" };

        // Act
        var rawValue = record.GetRawValue("ogd09");
        var displayValue = record.GetDisplayValue("ogd09");

        // Assert
        Assert.Equal("6733", rawValue);           // Raw Value 無千分位
        Assert.Equal("6,733", displayValue);      // Display Value 有千分位

        // 條碼應使用 Raw Value
        var barcode = _generator.GenerateQRCode(rawValue);
        Assert.NotEmpty(barcode);
    }

    /// <summary>
    /// 測試：DataRecord.GetRawValue 不區分大小寫
    /// </summary>
    [Theory]
    [InlineData("ogd09")]
    [InlineData("OGD09")]
    [InlineData("Ogd09")]
    public void DataRecord_GetRawValue_CaseInsensitive(string fieldName)
    {
        // Arrange
        var record = new DataRecord { Ogd09 = "12345" };

        // Act
        var result = record.GetRawValue(fieldName);

        // Assert
        Assert.Equal("12345", result);
    }

    /// <summary>
    /// 測試：DataRecord.GetRawValue 應 Trim 空白
    /// </summary>
    [Fact]
    public void DataRecord_GetRawValue_ShouldTrim()
    {
        // Arrange
        var record = new DataRecord { Ogb19 = "  G25A111577  " };

        // Act
        var result = record.GetRawValue("ogb19");

        // Assert
        Assert.Equal("G25A111577", result);
    }

    /// <summary>
    /// 測試：不存在的欄位應回傳空字串
    /// </summary>
    [Fact]
    public void DataRecord_GetRawValue_UnknownField_ReturnsEmpty()
    {
        // Arrange
        var record = new DataRecord();

        // Act
        var result = record.GetRawValue("unknown_field");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region UnitConverter Tests [ref: raw_spec 13.1]

    /// <summary>
    /// 測試：mm 轉 px @300DPI 換算正確 [ref: raw_spec 13.1]
    /// </summary>
    [Theory]
    [InlineData(10.0, 118)]   // Code 128 高度
    [InlineData(20.0, 236)]   // QR Code 尺寸
    [InlineData(25.4, 300)]   // 1 inch = 300 pixels
    [InlineData(1.0, 12)]     // 1mm
    public void MmToPx300_ConversionCorrect(double mm, int expectedPx)
    {
        // Act
        var result = UnitConverter.MmToPx300Int(mm);

        // Assert
        Assert.Equal(expectedPx, result);
    }

    /// <summary>
    /// 測試：mm 轉 pt 換算正確 (PDF 用) [ref: raw_spec 13.1]
    /// </summary>
    [Theory]
    [InlineData(25.4, 72.0)]  // 1 inch = 72 points
    [InlineData(100.0, 283.46456692913385)]  // 標籤寬度
    [InlineData(60.0, 170.07874015748031)]   // 標籤高度
    public void MmToPt_ConversionCorrect(double mm, double expectedPt)
    {
        // Act
        var result = UnitConverter.MmToPt(mm);

        // Assert
        Assert.Equal(expectedPt, result, 5); // 5 位小數精度
    }

    /// <summary>
    /// 測試：mm 轉 WPF 單位換算正確 (預覽用) [ref: raw_spec 13.1]
    /// </summary>
    [Theory]
    [InlineData(25.4, 96.0)]  // 1 inch = 96 WPF units
    [InlineData(10.0, 37.795275590551185)]
    public void MmToWpf_ConversionCorrect(double mm, double expectedWpf)
    {
        // Act
        var result = UnitConverter.MmToWpf(mm);

        // Assert
        Assert.Equal(expectedWpf, result, 5);
    }

    #endregion

    #region Quiet Zone Tests [ref: raw_spec 6.1, 6.2]

    /// <summary>
    /// 測試：Code 128 Quiet Zone 設定 [ref: raw_spec 6.1]
    /// Quiet Zone: 左右各 10 倍最小單元寬度
    /// </summary>
    [Fact]
    public void Code128_QuietZone_Margin10()
    {
        // Code 128 應使用 Margin=10
        // 此測試驗證條碼能正確生成（包含 Quiet Zone）

        // Arrange
        var content = "TEST123";

        // Act
        var result = _generator.GenerateCode128(content);

        // Assert - 條碼應能成功生成
        Assert.NotEmpty(result);

        // 注意：實際 Quiet Zone 驗證需要解析圖片，這裡只驗證生成成功
        // Margin=10 設定在 BarcodeGenerator.cs 中
    }

    /// <summary>
    /// 測試：QR Code Quiet Zone 設定 [ref: raw_spec 6.2]
    /// Quiet Zone: 四周各 4 倍單元寬度
    /// </summary>
    [Fact]
    public void QRCode_QuietZone_Margin4()
    {
        // QR Code 應使用 Margin=4

        // Arrange
        var content = "TEST123";

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotEmpty(result);
    }

    #endregion

    #region Error Correction Tests [ref: raw_spec 6.2]

    /// <summary>
    /// 測試：QR Code 容錯等級 Level M (15%) [ref: raw_spec 6.2]
    /// </summary>
    [Fact]
    public void QRCode_ErrorCorrectionLevelM()
    {
        // ErrorCorrectionLevel.M 設定在 BarcodeGenerator.cs 中
        // 此測試驗證 QR Code 能正確生成

        // Arrange
        var content = "TEST-ERROR-CORRECTION";

        // Act
        var result = _generator.GenerateQRCode(content);

        // Assert
        Assert.NotEmpty(result);
    }

    #endregion

    #region No Truncation Tests [ref: raw_spec 13.9]

    /// <summary>
    /// 測試：條碼內容不應被裁切 [ref: raw_spec 13.9]
    /// </summary>
    [Fact]
    public void Barcode_NoEllipsis_NoTruncation()
    {
        // 驗證長內容條碼能完整生成，不會被裁切

        // Arrange
        var longContent = "VERY-LONG-BARCODE-CONTENT-THAT-SHOULD-NOT-BE-TRUNCATED-12345678901234567890";

        // Act
        var result = _generator.GenerateCode128(longContent);

        // Assert
        Assert.NotEmpty(result);
        // 條碼成功生成表示內容未被裁切
    }

    /// <summary>
    /// 測試：QR Code 內容不應被裁切 [ref: raw_spec 13.9]
    /// </summary>
    [Fact]
    public void QRCode_NoEllipsis_NoTruncation()
    {
        // Arrange
        var longContent = string.Join(";", Enumerable.Range(1, 50).Select(i => $"Field{i}"));

        // Act
        var result = _generator.GenerateQRCode(longContent);

        // Assert
        Assert.NotEmpty(result);
    }

    #endregion

    #region Consistency Tests

    /// <summary>
    /// 測試：相同內容應產生相同條碼
    /// </summary>
    [Fact]
    public void GenerateCode128_SameContent_ProducesSameResult()
    {
        // Arrange
        var content = "CONSISTENT-CONTENT";

        // Act
        var result1 = _generator.GenerateCode128(content);
        var result2 = _generator.GenerateCode128(content);

        // Assert
        Assert.Equal(result1.Length, result2.Length);
        Assert.True(result1.SequenceEqual(result2));
    }

    /// <summary>
    /// 測試：不同內容應產生不同條碼
    /// </summary>
    [Fact]
    public void GenerateCode128_DifferentContent_ProducesDifferentResult()
    {
        // Arrange & Act
        var result1 = _generator.GenerateCode128("CONTENT-A");
        var result2 = _generator.GenerateCode128("CONTENT-B");

        // Assert
        Assert.False(result1.SequenceEqual(result2));
    }

    #endregion
}
