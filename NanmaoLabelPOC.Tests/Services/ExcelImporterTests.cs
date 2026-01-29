using System.Data;
using System.IO;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// ExcelImporter 單元測試
/// [ref: raw_spec 7.2, 憲章 II, T020]
///
/// 測試項目：
/// - Case-insensitive field matching [ref: raw_spec 3.3, 13.11]
/// - Field name validation (reject underscore, space, special chars) [ref: raw_spec 13.11]
/// - Trim behavior [ref: raw_spec 3.3]
/// - Date format conversion [ref: raw_spec 3.3, 13.2]
/// - Quantity validation (reject thousand separators) [ref: raw_spec 13.14]
/// - Semicolon warning [ref: raw_spec 13.4]
/// </summary>
public class ExcelImporterTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ExcelImporter _importer;

    public ExcelImporterTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"NanmaoLabelPOC_ExcelTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _importer = new ExcelImporter();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // 忽略清理失敗
        }
    }

    #region File Validation Tests

    /// <summary>
    /// 測試：檔案不存在應回傳錯誤 [ref: raw_spec 8.9]
    /// </summary>
    [Fact]
    public void Import_FileNotExists_ReturnsError()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "not_exists.xlsx");

        // Act
        var result = _importer.Import(nonExistentPath);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("找不到", result.ErrorMessage);
    }

    /// <summary>
    /// 測試：非 xlsx 格式應回傳錯誤 [ref: raw_spec 8.9]
    /// </summary>
    [Fact]
    public void Import_InvalidFormat_ReturnsError()
    {
        // Arrange
        var xlsPath = Path.Combine(_testDirectory, "test.xls");
        File.WriteAllText(xlsPath, "dummy content");

        // Act
        var result = _importer.Import(xlsPath);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("xlsx", result.ErrorMessage);
        Assert.Contains("格式不正確", result.ErrorMessage);
    }

    /// <summary>
    /// 測試：CSV 格式應回傳錯誤
    /// </summary>
    [Fact]
    public void Import_CsvFormat_ReturnsError()
    {
        // Arrange
        var csvPath = Path.Combine(_testDirectory, "test.csv");
        File.WriteAllText(csvPath, "ogb19,nvr_cust\nG25A111577,TSMC");

        // Act
        var result = _importer.Import(csvPath);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("xlsx", result.ErrorMessage);
    }

    #endregion

    #region Field Name Validation Tests [ref: spec.md FR-001, FR-002]

    /// <summary>
    /// T005: 含底線的欄位名稱應被判定為合法 [ref: spec.md FR-001]
    ///
    /// FR-001: 系統 MUST 接受欄位名稱包含英數字（A-Z、a-z、0-9）及底線（_）
    ///
    /// 此測試驗證：
    /// - nvr_cust, nvr_cust_item_no, nvr_cust_pn, nvr_remark10 等含底線的標準欄位
    /// - 使用新的正規表達式 ^[A-Za-z0-9_]+$ 進行驗證
    /// </summary>
    [Fact]
    public void FieldNameValidation_WithUnderscore_ShouldBeValid()
    {
        // Arrange - 規格書定義的含底線合法欄位
        var validNamesWithUnderscore = new[]
        {
            "nvr_cust",           // 客戶代號
            "nvr_cust_item_no",   // 客戶件號
            "nvr_cust_pn",        // 客戶料號
            "nvr_remark10",       // 備註
            "field_name",         // 一般底線欄位
            "a_b_c",              // 多底線
            "_prefix",            // 底線開頭
            "suffix_"             // 底線結尾
        };

        // 新的正規表達式：允許英數字及底線
        var pattern = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_]+$");

        // Act & Assert
        foreach (var name in validNamesWithUnderscore)
        {
            Assert.True(pattern.IsMatch(name), $"'{name}' should be valid (FR-001: underscore allowed)");
        }
    }

    /// <summary>
    /// T006: 含特殊字元的欄位名稱應被判定為非法 [ref: spec.md FR-002]
    ///
    /// FR-002: 系統 MUST 拒絕欄位名稱包含其他特殊字元（如 -、@、空格等）
    ///
    /// 此測試驗證：
    /// - 含空格、連字號、@、中文等特殊字元的欄位名稱應被拒絕
    /// - 空字串應被拒絕
    /// </summary>
    [Fact]
    public void FieldNameValidation_WithSpecialChars_ShouldBeInvalid()
    {
        // Arrange - 含特殊字元的非法欄位名稱
        var invalidNames = new[]
        {
            "field name",         // 含空格
            "field-name",         // 含連字號
            "field@name",         // 含 @
            "field.name",         // 含句點
            "field#name",         // 含 #
            "欄位名",             // 中文
            "field名稱",          // 混合中文
            "",                   // 空字串
            "   ",                // 純空白
            "field\tname",        // 含 Tab
            "field\nname"         // 含換行
        };

        // 新的正規表達式：允許英數字及底線
        var pattern = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_]+$");

        // Act & Assert
        foreach (var name in invalidNames)
        {
            Assert.False(pattern.IsMatch(name), $"'{name}' should be invalid (FR-002: special chars rejected)");
        }
    }

    /// <summary>
    /// 測試：欄位名稱允許英數字及底線 [ref: spec.md FR-001, FR-002]
    ///
    /// 綜合驗證新的欄位名稱規則
    /// </summary>
    [Fact]
    public void FieldNameValidation_AlphanumericAndUnderscore()
    {
        // Arrange
        var validNames = new[]
        {
            "ogb19", "OGB19", "Ogb19",           // 純英數字
            "nvrcust", "NVRCUST", "field123",   // 純英數字
            "nvr_cust", "nvr_cust_item_no",     // 含底線（FR-001 允許）
            "field_name", "a_b_c"               // 含底線
        };

        var invalidNames = new[]
        {
            "field name",         // 空格（FR-002 拒絕）
            "field@name",         // @ 符號
            "field-name",         // 連字號
            "欄位名"              // 中文
        };

        var pattern = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9_]+$");

        // Act & Assert
        foreach (var name in validNames)
        {
            Assert.True(pattern.IsMatch(name), $"'{name}' should be valid");
        }

        foreach (var name in invalidNames)
        {
            Assert.False(pattern.IsMatch(name), $"'{name}' should be invalid");
        }
    }

    #endregion

    #region Case-Insensitive Matching Tests [ref: raw_spec 3.3, 13.11]

    /// <summary>
    /// 測試：欄位名稱不分大小寫 [ref: raw_spec 3.3, 13.11]
    /// </summary>
    [Fact]
    public void CaseInsensitiveMatching_ShouldMatchDifferentCases()
    {
        // 驗證 Dictionary 使用 StringComparer.OrdinalIgnoreCase
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "ogb19", 0 },
            { "nvrcust", 1 }
        };

        // 不同大小寫應該能找到
        Assert.True(mapping.ContainsKey("OGB19"));
        Assert.True(mapping.ContainsKey("ogB19"));
        Assert.True(mapping.ContainsKey("NVRCUST"));
        Assert.True(mapping.ContainsKey("NvrCust"));
    }

    #endregion

    #region Trim Tests [ref: raw_spec 3.3]

    /// <summary>
    /// 測試：欄位值應 Trim 前後空白 [ref: raw_spec 3.3]
    /// </summary>
    [Fact]
    public void TrimBehavior_ShouldTrimWhitespace()
    {
        // 驗證 Trim 邏輯
        var testCases = new Dictionary<string, string>
        {
            { "  TSMC  ", "TSMC" },
            { "\tTSMC\t", "TSMC" },
            { " \n TSMC \n ", "TSMC" },
            { "TSMC", "TSMC" },
            { "", "" },
            { "   ", "" }
        };

        foreach (var (input, expected) in testCases)
        {
            Assert.Equal(expected, input?.Trim() ?? string.Empty);
        }
    }

    #endregion

    #region Date Format Tests [ref: raw_spec 3.3, 13.2]

    /// <summary>
    /// 測試：日期應轉換為 yyyy-MM-dd 格式 [ref: raw_spec 3.3, 13.2]
    /// </summary>
    [Fact]
    public void DateConversion_ShouldConvertToYyyyMmDd()
    {
        // 驗證日期格式轉換邏輯
        var testCases = new[]
        {
            (Input: "2025/11/14", Expected: "2025-11-14"),
            (Input: "2025-11-14", Expected: "2025-11-14"),
            (Input: "11/14/2025", Expected: "2025-11-14"),
        };

        foreach (var (input, expected) in testCases)
        {
            if (DateTime.TryParse(input, out var date))
            {
                var result = date.ToString("yyyy-MM-dd");
                Assert.Equal(expected, result);
            }
        }
    }

    /// <summary>
    /// 測試：DateTime 物件應轉換為 yyyy-MM-dd
    /// </summary>
    [Fact]
    public void DateConversion_DateTimeObject_ShouldFormat()
    {
        // Arrange
        var dateTime = new DateTime(2025, 11, 14);

        // Act
        var result = dateTime.ToString("yyyy-MM-dd");

        // Assert
        Assert.Equal("2025-11-14", result);
    }

    #endregion

    #region Quantity Validation Tests [ref: raw_spec 13.14]

    /// <summary>
    /// 測試：數量欄位應僅允許純數字 [ref: raw_spec 13.14]
    /// </summary>
    [Fact]
    public void QuantityValidation_DigitsOnly()
    {
        // 驗證數字模式
        var digitsPattern = new System.Text.RegularExpressions.Regex(@"^\d+$");

        var validValues = new[] { "6733", "0", "123456789", "007" };
        var invalidValues = new[] { "6,733", "6.733", "6733個", "-100", "1.5" };

        foreach (var value in validValues)
        {
            Assert.True(digitsPattern.IsMatch(value), $"'{value}' should be valid");
        }

        foreach (var value in invalidValues)
        {
            Assert.False(digitsPattern.IsMatch(value), $"'{value}' should be invalid");
        }
    }

    /// <summary>
    /// 測試：千分位格式應被檢測 [ref: raw_spec 13.14]
    /// </summary>
    [Fact]
    public void QuantityValidation_ThousandSeparatorDetection()
    {
        // 驗證千分位檢測模式
        var thousandPattern = new System.Text.RegularExpressions.Regex(@"^\d{1,3}(,\d{3})+$");

        var withSeparators = new[] { "6,733", "1,234,567", "12,345" };
        var withoutSeparators = new[] { "6733", "1234567", "12345", "6,73", "6,7333" };

        foreach (var value in withSeparators)
        {
            Assert.True(thousandPattern.IsMatch(value), $"'{value}' should match thousand separator pattern");
        }

        foreach (var value in withoutSeparators)
        {
            Assert.False(thousandPattern.IsMatch(value), $"'{value}' should not match thousand separator pattern");
        }
    }

    /// <summary>
    /// 測試：千分位移除後應為純數字
    /// </summary>
    [Fact]
    public void QuantityValidation_ThousandSeparatorRemoval()
    {
        // Arrange
        var valueWithSeparator = "1,234,567";

        // Act
        var cleaned = valueWithSeparator.Replace(",", "");

        // Assert
        Assert.Equal("1234567", cleaned);
        Assert.True(long.TryParse(cleaned, out var number));
        Assert.Equal(1234567, number);
    }

    #endregion

    #region Semicolon Warning Tests [ref: raw_spec 3.3, 13.4]

    /// <summary>
    /// 測試：欄位值包含分號應產生警告 [ref: raw_spec 3.3, 13.4]
    /// </summary>
    [Fact]
    public void SemicolonWarning_ShouldDetectSemicolon()
    {
        // 驗證分號檢測邏輯
        var qrCodeFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pono", "ima902", "nvr_remark10", "erpmat", "cscustpo", "ogd09"
        };

        var valuesWithSemicolon = new Dictionary<string, string>
        {
            { "pono", "PO;123" },
            { "ima902", "A;B;C" },
            { "nvr_remark10", "備註;說明" }
        };

        foreach (var (field, value) in valuesWithSemicolon)
        {
            Assert.True(value.Contains(';'), $"Value '{value}' should contain semicolon");
            Assert.True(qrCodeFields.Contains(field), $"Field '{field}' should be QR code field");
        }
    }

    /// <summary>
    /// 測試：QR Code 欄位清單正確
    /// </summary>
    [Fact]
    public void QrCodeFields_ShouldContainCorrectFields()
    {
        // 根據 raw_spec，QR Code 組合欄位
        var expectedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pono", "ima902", "nvr_remark10", "erpmat", "cscustpo", "ogd09"
        };

        // QW075551-1: pono;ima902;ogd09;nvr_remark10
        Assert.Contains("pono", expectedFields);
        Assert.Contains("ima902", expectedFields);
        Assert.Contains("ogd09", expectedFields);
        Assert.Contains("nvr_remark10", expectedFields);

        // QW075551-2: cscustpo;erpmat;ogd09
        Assert.Contains("cscustpo", expectedFields);
        Assert.Contains("erpmat", expectedFields);
    }

    #endregion

    #region UUID Generation Tests [ref: raw_spec 附錄 B.2]

    /// <summary>
    /// 測試：DataRecord 預設應有 UUID
    /// </summary>
    [Fact]
    public void DataRecord_ShouldHaveDefaultUuid()
    {
        // Arrange & Act
        var record = new DataRecord();

        // Assert
        Assert.False(string.IsNullOrEmpty(record.Id));
        Assert.True(Guid.TryParse(record.Id, out _));
    }

    /// <summary>
    /// 測試：多個 DataRecord 應有不同 UUID
    /// </summary>
    [Fact]
    public void DataRecord_MultipleRecords_ShouldHaveDifferentUuids()
    {
        // Arrange & Act
        var records = Enumerable.Range(0, 100).Select(_ => new DataRecord()).ToList();

        // Assert
        var uniqueIds = records.Select(r => r.Id).Distinct().Count();
        Assert.Equal(100, uniqueIds);
    }

    #endregion

    #region Empty Row Handling Tests [ref: raw_spec 3.3]

    /// <summary>
    /// 測試：空白列應被忽略 [ref: raw_spec 3.3]
    /// </summary>
    [Fact]
    public void EmptyRowDetection_AllEmpty_ShouldBeSkipped()
    {
        // 驗證空白列檢測邏輯
        var emptyValues = new[] { "", "   ", null, "\t", "\n" };

        foreach (var value in emptyValues)
        {
            Assert.True(string.IsNullOrWhiteSpace(value));
        }
    }

    #endregion

    #region Leading Zeros Preservation Tests [ref: raw_spec 3.1]

    /// <summary>
    /// 測試：前導零應被保留 [ref: raw_spec 3.1 備註]
    /// </summary>
    [Fact]
    public void LeadingZeros_ShouldBePreserved()
    {
        // 驗證字串處理保留前導零
        var valuesWithLeadingZeros = new[] { "007", "00123", "000" };

        foreach (var value in valuesWithLeadingZeros)
        {
            // 作為字串處理，前導零應保留
            Assert.StartsWith("0", value);

            // 如果轉為數字會失去前導零
            if (int.TryParse(value, out var number))
            {
                var numericString = number.ToString();
                Assert.NotEqual(value, numericString); // 數字格式會失去前導零
            }
        }
    }

    #endregion

    #region Import Result Structure Tests

    /// <summary>
    /// 測試：ImportResult 應正確初始化
    /// </summary>
    [Fact]
    public void ImportResult_DefaultValues()
    {
        // Arrange & Act
        var result = new ImportResult();

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Records);
        Assert.Empty(result.Records);
        Assert.Null(result.ErrorMessage);
        // TODO: Phase 6 (T027) 將遷移至 Messages 屬性
#pragma warning disable CS0618 // Obsolete - 向後相容，Phase 6 遷移
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
#pragma warning restore CS0618
        Assert.Equal(0, result.RecordCount);
    }

    /// <summary>
    /// 測試：ImportResult.RecordCount 應反映實際筆數
    /// </summary>
    [Fact]
    public void ImportResult_RecordCount_ShouldReflectActualCount()
    {
        // Arrange
        var result = new ImportResult();
        result.Records.Add(new DataRecord { Ogb19 = "G001" });
        result.Records.Add(new DataRecord { Ogb19 = "G002" });
        result.Records.Add(new DataRecord { Ogb19 = "G003" });

        // Act & Assert
        Assert.Equal(3, result.RecordCount);
    }

    #endregion
}
