using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.Templates;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// LabelRenderer 單元測試
/// [ref: raw_spec 7.2, 憲章 II, TC-02, TC-03]
///
/// 測試項目：
/// - 變數對應 [ref: raw_spec TC-02]
/// - 常數值處理 [ref: raw_spec TC-03]
/// - Raw Value vs Display Value 分離 [ref: raw_spec 13.13]
/// - QR Code 空值保留 (A;;C format) [ref: raw_spec 13.15]
/// - 條碼內容無 Ellipsis [ref: raw_spec 13.9]
/// </summary>
public class LabelRendererTests
{
    private readonly LabelRenderer _sut;

    public LabelRendererTests()
    {
        _sut = new LabelRenderer();
    }

    #region Variable Substitution Tests [ref: TC-02]

    [Fact]
    public void Render_WithVariableField_ShouldSubstituteFromDataRecord()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "TestField",
            FieldType = FieldType.Text,
            DataSource = "nvr_cust",
            IsConstant = false,
            X = 10, Y = 10, Width = 50, Height = 10
        });

        var record = new DataRecord
        {
            NvrCust = "TestCustomer"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        Assert.Equal("TestCustomer", commands[0].Content);
    }

    [Fact]
    public void Render_WithMultipleVariables_ShouldSubstituteAll()
    {
        // Arrange
        var template = new LabelTemplate
        {
            Code = "TEST",
            Name = "Test",
            WidthMm = 100,
            HeightMm = 60,
            Fields = new List<LabelField>
            {
                new() { Name = "Field1", FieldType = FieldType.Text, DataSource = "nvr_cust", X = 10, Y = 10, Width = 50, Height = 10 },
                new() { Name = "Field2", FieldType = FieldType.Text, DataSource = "ogb19", X = 10, Y = 20, Width = 50, Height = 10 }
            }
        };

        var record = new DataRecord
        {
            NvrCust = "Customer1",
            Ogb19 = "DOC12345"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Equal(2, commands.Count);
        Assert.Equal("Customer1", commands[0].Content);
        Assert.Equal("DOC12345", commands[1].Content);
    }

    #endregion

    #region Constant Value Tests [ref: TC-03]

    [Fact]
    public void Render_WithConstantField_ShouldOutputConstantValue()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "ConstField",
            FieldType = FieldType.Text,
            DataSource = "17008",
            IsConstant = true,
            X = 10, Y = 10, Width = 50, Height = 10
        });

        var record = new DataRecord();

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        Assert.Equal("17008", commands[0].Content);
    }

    [Fact]
    public void Render_QW075551_2_CSNUMBER_ShouldBeConstant17008()
    {
        // Arrange - 使用 QW075551-2 模板
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            Ogd09 = "1000"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - CSNUMBER 應為常數 "17008"
        var csNumberCommand = commands.FirstOrDefault(c => c.FieldName == "CSNUMBER");
        Assert.NotNull(csNumberCommand);
        Assert.Equal("17008", csNumberCommand.Content);
    }

    #endregion

    #region Raw Value vs Display Value Tests [ref: 13.13]

    [Fact]
    public void Render_TextFieldWithDisplayValue_ShouldFormatQuantityWithThousandSeparator()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "CSQTY",
            FieldType = FieldType.Text,
            DataSource = "ogd09",
            IsConstant = false,
            UseDisplayValue = true,
            X = 10, Y = 10, Width = 50, Height = 10
        });

        var record = new DataRecord
        {
            Ogd09 = "6733"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 應有千分位
        Assert.Single(commands);
        Assert.Equal("6,733", commands[0].Content);
    }

    [Fact]
    public void Render_BarcodeField_ShouldUseRawValue()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "ERPPARTNO",
            FieldType = FieldType.Barcode,
            DataSource = "erpmat",
            IsConstant = false,
            UseDisplayValue = false, // Barcode 永遠使用 Raw Value
            X = 10, Y = 10, Width = 60, Height = 10
        });

        var record = new DataRecord
        {
            Erpmat = "4D010018"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 條碼應為原始值
        Assert.Single(commands);
        Assert.Equal(RenderCommandType.Barcode, commands[0].CommandType);
        Assert.Equal("4D010018", commands[0].Content);
    }

    [Fact]
    public void Render_QRCodeField_ShouldUseRawValueInCombinePattern()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "QRCODE",
            FieldType = FieldType.QRCode,
            DataSource = string.Empty,
            IsConstant = false,
            CombinePattern = "{pono};{ima902};{ogd09}",
            X = 75, Y = 40, Width = 20, Height = 20
        });

        var record = new DataRecord
        {
            Pono = "511-251020041",
            Ima902 = "FC7800280H",
            Ogd09 = "6733" // Raw value, 無千分位
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - QR Code 內容應使用 Raw Value（無千分位）
        Assert.Single(commands);
        Assert.Equal(RenderCommandType.QRCode, commands[0].CommandType);
        Assert.Equal("511-251020041;FC7800280H;6733", commands[0].Content);
    }

    #endregion

    #region QR Code Empty Field Preservation Tests [ref: 13.4, 13.15]

    [Fact]
    public void Render_QRCodeWithEmptyField_ShouldPreserveSemicolonPosition()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "QRCODE",
            FieldType = FieldType.QRCode,
            DataSource = string.Empty,
            IsConstant = false,
            CombinePattern = "{pono};{ima902};{ogd09};{nvr_remark10}",
            X = 75, Y = 40, Width = 20, Height = 20
        });

        var record = new DataRecord
        {
            Pono = "PO123",
            Ima902 = "", // 空值
            Ogd09 = "1000",
            NvrRemark10 = "" // 空值
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 空值應保留分號位置 (A;;C; 格式)
        Assert.Single(commands);
        Assert.Equal("PO123;;1000;", commands[0].Content);
    }

    [Fact]
    public void Render_QRCodeWithAllEmptyFields_ShouldSkip()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "QRCODE",
            FieldType = FieldType.QRCode,
            DataSource = string.Empty,
            IsConstant = false,
            CombinePattern = "{pono};{ima902};{ogd09}",
            X = 75, Y = 40, Width = 20, Height = 20
        });

        var record = new DataRecord
        {
            Pono = "",
            Ima902 = "",
            Ogd09 = ""
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 全空應標記為 Skip
        Assert.Single(commands);
        Assert.True(commands[0].Skip);
    }

    #endregion

    #region Barcode Skip Tests [ref: 13.15]

    [Fact]
    public void Render_BarcodeWithEmptyContent_ShouldSkip()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "CSCUSTPN",
            FieldType = FieldType.Barcode,
            DataSource = "nvr_cust_pn",
            IsConstant = false,
            X = 10, Y = 35, Width = 60, Height = 10
        });

        var record = new DataRecord
        {
            NvrCustPn = "" // 空值
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 空條碼應標記為 Skip
        Assert.Single(commands);
        Assert.True(commands[0].Skip);
    }

    [Fact]
    public void Render_BarcodeWithContent_ShouldNotSkip()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "CSCUSTPN",
            FieldType = FieldType.Barcode,
            DataSource = "nvr_cust_pn",
            IsConstant = false,
            X = 10, Y = 35, Width = 60, Height = 10
        });

        var record = new DataRecord
        {
            NvrCustPn = "E110-X0"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 有內容的條碼不應 Skip
        Assert.Single(commands);
        Assert.False(commands[0].Skip);
        Assert.Equal("E110-X0", commands[0].Content);
    }

    #endregion

    #region Barcode Content No Ellipsis Tests [ref: 13.9]

    [Fact]
    public void Render_BarcodeField_ShouldNotTruncateContent()
    {
        // Arrange - 使用較長的條碼內容
        var longContent = "VERY-LONG-BARCODE-CONTENT-12345678901234567890";
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "CSCUSTPN",
            FieldType = FieldType.Barcode,
            DataSource = "nvr_cust_pn",
            IsConstant = false,
            X = 10, Y = 35, Width = 60, Height = 10
        });

        var record = new DataRecord
        {
            NvrCustPn = longContent
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 條碼內容應完整，無裁切
        Assert.Single(commands);
        Assert.Equal(longContent, commands[0].Content);
        Assert.DoesNotContain("...", commands[0].Content);
    }

    #endregion

    #region Delta Spec QW075551-1 Tests [ref: spec.md 006-delta-label-qw075551-1]

    /// <summary>
    /// T009: 驗證日期格式轉換 yyyy-MM-dd → yyyy/MM/dd
    /// [ref: FR-006, SC-002]
    /// </summary>
    [Fact]
    public void Render_QW075551_1_DateFormat_ReturnsSlashFormat()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "TestCustomer",
            Obe25 = "2025-11-14", // 輸入格式: yyyy-MM-dd
            Ogd09 = "1000",
            NvrCustItemNo = "ITEM001",
            NvrCustPn = "PN001",
            Pono = "PO001",
            Ima902 = "DEV001",
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - FINDPRTDC 日期格式應為 yyyy/MM/dd
        var dateCommand = commands.FirstOrDefault(c => c.FieldName == "FINDPRTDC");
        Assert.NotNull(dateCommand);
        Assert.Equal("2025/11/14", dateCommand.Content);
    }

    /// <summary>
    /// T009: 驗證日期格式異常時返回原始值
    /// [ref: FR-006, spec.md Edge Cases]
    /// </summary>
    [Fact]
    public void Render_QW075551_1_DateFormat_InvalidFormat_ReturnsOriginalValue()
    {
        // Arrange
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "FINDPRTDC",
            FieldType = FieldType.Text,
            DataSource = "obe25",
            IsConstant = false,
            X = 10, Y = 10, Width = 50, Height = 10
        });

        var record = new DataRecord
        {
            Obe25 = "invalid-date-format"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 格式異常時返回原始值
        Assert.Single(commands);
        Assert.Equal("invalid-date-format", commands[0].Content);
    }

    /// <summary>
    /// T010: 驗證 CSCUSTPN 欄位為 Text 類型
    /// [ref: FR-005, FR-007, SC-003, SC-010]
    ///
    /// 注意：此測試目前會 FAIL，因為模板尚未更新
    /// 等待 T012 實作後會 PASS
    /// </summary>
    [Fact]
    public void Render_QW075551_1_CSCUSTPN_ReturnsText()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "TestCustomer",
            Obe25 = "2025-11-14",
            Ogd09 = "1000",
            NvrCustItemNo = "ITEM001",
            NvrCustPn = "E110-X0",
            Pono = "PO001",
            Ima902 = "DEV001",
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - CSCUSTPN 應為 Text 類型，不是 Barcode
        var cscustpnCommand = commands.FirstOrDefault(c => c.FieldName == "CSCUSTPN");
        Assert.NotNull(cscustpnCommand);
        Assert.Equal(RenderCommandType.Text, cscustpnCommand.CommandType);
        Assert.Equal("E110-X0", cscustpnCommand.Content);
    }

    /// <summary>
    /// T010: 驗證 QW075551-1 模板無 Code 128 條碼
    /// [ref: FR-007, SC-010]
    ///
    /// 注意：此測試目前會 FAIL，因為模板尚未更新
    /// 等待 T012 實作後會 PASS
    /// </summary>
    [Fact]
    public void Render_QW075551_1_NoCode128Barcode()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "TestCustomer",
            Obe25 = "2025-11-14",
            Ogd09 = "1000",
            NvrCustItemNo = "ITEM001",
            NvrCustPn = "E110-X0",
            Pono = "PO001",
            Ima902 = "DEV001",
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 不應有任何 Barcode 類型的指令
        var barcodeCommands = commands.Where(c => c.CommandType == RenderCommandType.Barcode).ToList();
        Assert.Empty(barcodeCommands);
    }

    /// <summary>
    /// T017: 驗證 QR Code 位置為左下角 (X=5, Y=55)
    /// [ref: FR-012, SC-004]
    ///
    /// 驗收條件：QR Code 尺寸為 20mm × 20mm，位於左下角 Remarks 區段
    /// </summary>
    [Fact]
    public void Render_QW075551_1_QRCode_Position()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "TestCustomer",
            Obe25 = "2025-11-14",
            Ogd09 = "1000",
            NvrCustItemNo = "ITEM001",
            NvrCustPn = "PN001",
            Pono = "511-251020041",
            Ima902 = "FC7800280H-00000-00005",
            NvrRemark10 = "00U35NVVR"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - QRCODE 位置應在左下角 (X=5, Y=57), 尺寸 20x20
        // Y 座標配合標題分行調整
        var qrCodeCommand = commands.FirstOrDefault(c => c.FieldName == "QRCODE");
        Assert.NotNull(qrCodeCommand);
        Assert.Equal(RenderCommandType.QRCode, qrCodeCommand.CommandType);
        Assert.Equal(5, qrCodeCommand.X);   // [FR-012] 左下角 X=5
        Assert.Equal(57, qrCodeCommand.Y);  // [FR-012] 左下角 Y=57 (配合標題分行調整)
        Assert.Equal(20, qrCodeCommand.Width);   // 尺寸 20mm
        Assert.Equal(20, qrCodeCommand.Height);  // 尺寸 20mm
    }

    /// <summary>
    /// T018: 驗證 QR Code 內容格式為 {CSMO};{OUTDEVICENO};{CSQTY};{CSREMARK}
    /// [ref: FR-009, FR-010, SC-005, SC-006]
    ///
    /// 驗收條件：
    /// 1. QR Code 編碼內容為 `511-251020041;FC7800280H-00000-00005;6733;00U35NVVR`
    /// 2. CSQTY 在 QR Code 中使用原始值 (6733，無千分位)
    /// </summary>
    [Fact]
    public void Render_QW075551_1_QRCode_Content()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        // 使用 spec.md User Story 2 的測試資料
        var record = new DataRecord
        {
            NvrCust = "TestCustomer",
            Obe25 = "2025-11-14",
            Ogd09 = "6733",  // 原始值，QR Code 應為 6733（無千分位）
            NvrCustItemNo = "ITEM001",
            NvrCustPn = "PN001",
            Pono = "511-251020041",           // CSMO
            Ima902 = "FC7800280H-00000-00005", // OUTDEVICENO
            NvrRemark10 = "00U35NVVR"         // CSREMARK
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - QR Code 內容格式應為 CSMO;OUTDEVICENO;CSQTY;CSREMARK
        var qrCodeCommand = commands.FirstOrDefault(c => c.FieldName == "QRCODE");
        Assert.NotNull(qrCodeCommand);
        Assert.Equal(RenderCommandType.QRCode, qrCodeCommand.CommandType);

        // [FR-009] 內容格式: {CSMO};{OUTDEVICENO};{CSQTY};{CSREMARK}
        // [FR-010] CSQTY 使用原始值（無千分位）
        Assert.Equal("511-251020041;FC7800280H-00000-00005;6733;00U35NVVR", qrCodeCommand.Content);
    }

    #endregion

    #region Full Template Tests

    /// <summary>
    /// 整合測試：驗證 QW075551-1 模板渲染正確
    /// [更新] 依據 Delta Spec 修改：
    /// - CSCUSTPN 由 Barcode 改為 Text [FR-005, FR-007]
    /// - 日期格式改為 yyyy/MM/dd [FR-006]
    /// - 移除 MoLabel/DeviceLabel/RemarkLabel，新增 RemarksLabel [FR-013]
    /// - 欄位數由 17 改為 15 (17 - 3 + 1 = 15)
    /// </summary>
    [Fact]
    public void Render_QW075551_1_ShouldProduceCorrectCommands()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "XinshengIntel",
            Obe25 = "2025-11-14",
            Ogd09 = "6733",
            NvrCustItemNo = "XPA72EA0I-008",
            NvrCustPn = "E110-X0",
            Pono = "511-251020041",
            Ima902 = "FC7800280H",
            NvrRemark10 = "00U35NVVR"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 應有 15 個欄位 (原 17 - 3 獨立標籤 + 1 統一標籤) [FR-013]
        Assert.Equal(15, commands.Count);

        // 驗證關鍵欄位
        var customerCommand = commands.First(c => c.FieldName == "CSCUSTOMER");
        Assert.Equal("XinshengIntel", customerCommand.Content);

        var qtyCommand = commands.First(c => c.FieldName == "CSQTY");
        Assert.Equal("6,733", qtyCommand.Content); // Display Value

        // [Delta Spec FR-005, FR-007] CSCUSTPN 現在是 Text 類型
        var cscustpnCommand = commands.First(c => c.FieldName == "CSCUSTPN");
        Assert.Equal(RenderCommandType.Text, cscustpnCommand.CommandType);
        Assert.Equal("E110-X0", cscustpnCommand.Content);

        // [Delta Spec FR-006] 日期格式 yyyy/MM/dd
        var dateCommand = commands.First(c => c.FieldName == "FINDPRTDC");
        Assert.Equal("2025/11/14", dateCommand.Content);

        // [Delta Spec FR-013] RemarksLabel 應存在
        var remarksLabelCommand = commands.First(c => c.FieldName == "RemarksLabel");
        Assert.Equal("Remarks", remarksLabelCommand.Content);

        // [Delta Spec FR-013] MoLabel, DeviceLabel, RemarkLabel 應不存在
        Assert.DoesNotContain(commands, c => c.FieldName == "MoLabel");
        Assert.DoesNotContain(commands, c => c.FieldName == "DeviceLabel");
        Assert.DoesNotContain(commands, c => c.FieldName == "RemarkLabel");

        var qrCommand = commands.First(c => c.FieldName == "QRCODE");
        Assert.Equal(RenderCommandType.QRCode, qrCommand.CommandType);
        Assert.Equal("511-251020041;FC7800280H;6733;00U35NVVR", qrCommand.Content);
    }

    #endregion

    #region User Story 4 Tests - PDF 輸出符合新規格 [ref: spec.md Phase 6]

    /// <summary>
    /// T026: 驗證 QW075551-1 模板 PDF 頁面尺寸為 100mm × 80mm
    /// [ref: FR-001, FR-017, SC-001]
    ///
    /// 驗收條件：PDF 輸出頁面尺寸驗證為 100mm × 80mm
    /// </summary>
    [Fact]
    public void Export_QW075551_1_PageSize_100x80mm()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        // Assert - 驗證模板尺寸為 100mm × 80mm [FR-001, FR-017]
        Assert.Equal(100, template.WidthMm);
        Assert.Equal(80, template.HeightMm);  // [FR-001] 60 → 80
    }

    /// <summary>
    /// T027: 驗證 QW075551-1 模板有外框且無分隔線
    /// [ref: FR-003, SC-007]
    ///
    /// 驗收條件：標籤具有外框（單線矩形邊框），無分隔線
    /// 注意：分隔線的驗證需透過視覺驗收或欄位檢查（無分隔線欄位定義）
    /// </summary>
    [Fact]
    public void Export_QW075551_1_HasBorder_NoSeparator()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        // Assert - 驗證有外框 [FR-003]
        Assert.True(template.HasBorder, "QW075551-1 應具有外框");

        // Assert - 驗證無分隔線欄位
        // 分隔線在模板中會以 FieldType.Line 或類似的欄位定義
        // 無分隔線表示不應有任何 Separator/Line 類型的欄位
        var separatorFields = template.Fields
            .Where(f => f.Name.Contains("Separator", StringComparison.OrdinalIgnoreCase) ||
                       f.Name.Contains("Line", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.Empty(separatorFields);
    }

    #endregion

    #region User Story 5 Tests - 長文字溢出處理 [ref: spec.md Phase 7]

    /// <summary>
    /// T030: 驗證長文字縮小字體邏輯，最小字體為 6pt
    /// [ref: FR-008, SC-009]
    ///
    /// 驗收條件：
    /// 1. 當 Text 欄位內容超過版面寬度時，自動縮小字體
    /// 2. 最小字體下限為 6pt
    /// 3. ActualFontSize 應反映縮小後的字體大小
    ///
    /// 估算公式: 字寬係數 = FontSize × 0.035
    /// - 中文字: 1.0 × 係數
    /// 11pt 時: 中文 0.385mm/字
    /// 測試: 50 字 × 0.385 = 19.25mm > 8mm (需縮小)
    /// 縮小至 9pt 時: 50 字 × 0.315 = 15.75mm > 8mm (還需縮小)
    /// 縮小至 7pt 時: 50 字 × 0.245 = 12.25mm > 8mm (還需縮小)
    /// 縮小至 6pt 時: 50 字 × 0.21 = 10.5mm > 8mm (觸發 RequiresWrap)
    /// </summary>
    [Fact]
    public void Render_LongText_ShrinkFont_MinSize6pt()
    {
        // Arrange - 建立啟用 AutoShrinkFont 的欄位
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "LongTextField",
            FieldType = FieldType.Text,
            DataSource = "nvr_cust",
            IsConstant = false,
            AutoShrinkFont = true,  // [FR-008] 啟用自動縮小
            MinFontSize = 6,        // [FR-008] 最小 6pt
            X = 10, Y = 10, Width = 12, Height = 5,  // 狹窄欄位 12mm
            FontSize = 11
        });

        var record = new DataRecord
        {
            // 使用 50 個中文字確保觸發縮小但不至於達到最小字體
            // 50 字 × 0.385 = 19.25mm > 12mm (11pt 時)
            // 50 字 × 0.315 = 15.75mm > 12mm (9pt 時)
            // 50 字 × 0.28 = 14mm > 12mm (8pt 時)
            // 50 字 × 0.245 = 12.25mm > 12mm (7pt 時，剛好超過一點)
            // 50 字 × 0.2275 = 11.375mm < 12mm (6.5pt 時適合)
            NvrCust = "這是一段非常非常非常非常非常非常非常非常非常非常非常長的客戶名稱測試"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        var command = commands[0];

        // 驗證字體已縮小 (ActualFontSize 應小於原始 FontSize)
        Assert.NotNull(command.ActualFontSize);
        Assert.True(command.ActualFontSize < 11, "長文字應自動縮小字體");

        // 驗證最小字體下限為 6pt
        Assert.True(command.ActualFontSize >= 6, "字體不應縮小至低於 6pt");
    }

    /// <summary>
    /// T030: 驗證正常長度文字不縮小
    /// [ref: FR-008]
    ///
    /// 驗收條件：文字未超過欄位寬度時，保持原始字體大小
    /// </summary>
    [Fact]
    public void Render_NormalText_NoShrink()
    {
        // Arrange - 建立啟用 AutoShrinkFont 的欄位
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "NormalTextField",
            FieldType = FieldType.Text,
            DataSource = "nvr_cust",
            IsConstant = false,
            AutoShrinkFont = true,
            MinFontSize = 6,
            X = 10, Y = 10, Width = 90, Height = 5,  // 寬欄位
            FontSize = 11
        });

        var record = new DataRecord
        {
            NvrCust = "Short"  // 短文字
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        var command = commands[0];

        // 短文字不應縮小
        Assert.Equal(11, command.ActualFontSize);
        Assert.False(command.RequiresWrap);
    }

    /// <summary>
    /// T031: 驗證超長文字達到最小字體後標記需要截斷
    /// [ref: FR-008, SC-009]
    ///
    /// 驗收條件：
    /// 1. 當縮至 6pt 仍無法容納時，RequiresWrap = true
    /// 2. 這表示渲染時應允許換行或截斷加省略號
    ///
    /// 6pt 時字寬係數 = 6 × 0.035 = 0.21mm
    /// 中文字: 0.21mm/字
    /// 100 個中文字 ≈ 21mm，需要欄位 < 21mm 才會標記截斷
    /// </summary>
    [Fact]
    public void Render_LongText_ExceedsMinFont_Truncate()
    {
        // Arrange - 建立非常狹窄的欄位
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "VeryLongTextField",
            FieldType = FieldType.Text,
            DataSource = "nvr_cust",
            IsConstant = false,
            AutoShrinkFont = true,
            MinFontSize = 6,
            X = 10, Y = 10, Width = 5, Height = 5,  // 極狹窄欄位 (5mm)
            FontSize = 11
        });

        var record = new DataRecord
        {
            // 超級長文字 (100+ 中文字)，即使 6pt 也無法容納於 5mm
            // 100 字 × 0.21mm = 21mm >> 5mm
            NvrCust = "這是一段非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常長的文字內容測試"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        var command = commands[0];

        // 驗證已達最小字體 6pt
        Assert.Equal(6, command.ActualFontSize);

        // 驗證標記需要換行/截斷
        Assert.True(command.RequiresWrap, "超長文字達到最小字體後應標記 RequiresWrap = true");
    }

    /// <summary>
    /// T032/T033: 驗證多個欄位各自獨立縮小處理
    /// [ref: FR-008, spec.md User Story 5 Acceptance Scenario 2]
    ///
    /// 驗收條件：
    /// 1. 多個欄位都需要縮小字體時，各欄位獨立處理
    /// 2. 一個欄位的縮小不影響其他欄位
    ///
    /// 估算: 11pt 中文 0.385mm/字, 6pt 中文 0.21mm/字
    /// </summary>
    [Fact]
    public void Render_MultipleFields_IndependentShrink()
    {
        // Arrange - 建立含多個 AutoShrinkFont 欄位的模板
        var template = new LabelTemplate
        {
            Code = "TEST",
            Name = "Test Template",
            WidthMm = 100,
            HeightMm = 60,
            Fields = new List<LabelField>
            {
                // 欄位 1: 長文字，需要縮小 (60字 × 0.385 ≈ 23mm > 15mm)
                new()
                {
                    Name = "Field1",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_cust",
                    IsConstant = false,
                    AutoShrinkFont = true,
                    MinFontSize = 6,
                    X = 10, Y = 10, Width = 15, Height = 5,  // 15mm
                    FontSize = 11
                },
                // 欄位 2: 短文字，不需要縮小
                new()
                {
                    Name = "Field2",
                    FieldType = FieldType.Text,
                    DataSource = "ogb19",
                    IsConstant = false,
                    AutoShrinkFont = true,
                    MinFontSize = 6,
                    X = 10, Y = 20, Width = 90, Height = 5,
                    FontSize = 11
                },
                // 欄位 3: 超長文字，需要截斷 (80字 × 0.21 ≈ 16.8mm > 5mm)
                new()
                {
                    Name = "Field3",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_cust_item_no",
                    IsConstant = false,
                    AutoShrinkFont = true,
                    MinFontSize = 6,
                    X = 10, Y = 30, Width = 5, Height = 5,  // 極狹窄 5mm
                    FontSize = 11
                }
            }
        };

        var record = new DataRecord
        {
            // 欄位 1: 60 中文字，需要縮小 (23mm > 15mm at 11pt)
            NvrCust = "這是一段較長的客戶名稱文字測試這是一段較長的客戶名稱文字測試這是一段較長的客戶名稱",
            Ogb19 = "DOC123",  // 欄位 2: 短，不需縮小
            // 欄位 3: 80 中文字，需要截斷 (16.8mm > 5mm at 6pt)
            NvrCustItemNo = "這是超級無敵非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常非常長的料號測試文字"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Equal(3, commands.Count);

        var field1Command = commands.First(c => c.FieldName == "Field1");
        var field2Command = commands.First(c => c.FieldName == "Field2");
        var field3Command = commands.First(c => c.FieldName == "Field3");

        // 欄位 1: 應縮小但不一定到最小 (需要縮小至字體使寬度 < 15mm)
        Assert.NotNull(field1Command.ActualFontSize);
        Assert.True(field1Command.ActualFontSize < 11, "Field1 應縮小字體");
        Assert.True(field1Command.ActualFontSize >= 6, "Field1 不應低於最小字體");

        // 欄位 2: 不應縮小
        Assert.Equal(11, field2Command.ActualFontSize);
        Assert.False(field2Command.RequiresWrap);

        // 欄位 3: 應達最小字體並標記截斷
        Assert.Equal(6, field3Command.ActualFontSize);
        Assert.True(field3Command.RequiresWrap, "Field3 超長文字應標記 RequiresWrap");
    }

    /// <summary>
    /// 驗證未啟用 AutoShrinkFont 的欄位不縮小
    /// [ref: FR-008]
    /// </summary>
    [Fact]
    public void Render_AutoShrinkFontDisabled_NoShrink()
    {
        // Arrange - AutoShrinkFont = false
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "NoShrinkField",
            FieldType = FieldType.Text,
            DataSource = "nvr_cust",
            IsConstant = false,
            AutoShrinkFont = false,  // 停用自動縮小
            MinFontSize = 6,
            X = 10, Y = 10, Width = 10, Height = 5,  // 狹窄欄位
            FontSize = 11
        });

        var record = new DataRecord
        {
            NvrCust = "這是一段非常非常長的文字內容"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        var command = commands[0];

        // 未啟用 AutoShrinkFont，不應縮小
        Assert.Equal(11, command.ActualFontSize);
        Assert.False(command.RequiresWrap);
    }

    #endregion

    #region User Story 4 Tests - 空值欄位處理 [ref: spec.md Phase 6, FR-009]

    /// <summary>
    /// T020: 驗證 Text 欄位空值時保留版面位置
    /// [ref: FR-009, spec.md User Story 4]
    ///
    /// 驗收條件：
    /// 1. Text 類型欄位空值時不跳過 (Skip = false)
    /// 2. 內容為空字串
    /// 3. 座標位置保留不變
    /// </summary>
    [Fact]
    public void Render_TextFieldWithEmptyValue_ShouldNotSkip()
    {
        // Arrange - Text 類型欄位
        var template = CreateSimpleTemplate(new LabelField
        {
            Name = "CSREMARK",
            FieldType = FieldType.Text,
            DataSource = "nvr_remark10",
            IsConstant = false,
            X = 55, Y = 55, Width = 40, Height = 6,
            FontSize = 11, IsBold = true,
            Alignment = TextAlignment.Left
        });

        var record = new DataRecord
        {
            NvrRemark10 = ""  // 空值
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        Assert.Single(commands);
        var command = commands[0];

        // Text 類型空值不應跳過 [FR-009]
        Assert.False(command.Skip, "Text 欄位空值應保留版面位置，不跳過");
        Assert.Equal(string.Empty, command.Content);

        // 座標應保留原位置
        Assert.Equal(55, command.X);
        Assert.Equal(55, command.Y);
        Assert.Equal(40, command.Width);
        Assert.Equal(6, command.Height);
    }

    /// <summary>
    /// T020: 驗證 QW075551-2 多個空值欄位版面不變
    /// [ref: FR-009, spec.md User Story 4 Acceptance Scenario 2]
    ///
    /// 驗收條件：多個欄位為空時，所有欄位位置維持不變
    /// </summary>
    [Fact]
    public void Render_QW075551_2_MultipleEmptyFields_LayoutPreserved()
    {
        // Arrange - 使用 QW075551-2 模板
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "",           // 空值
            Erpmat = "",             // 空值
            NvrCustItemNo = "",      // 空值
            Ogd09 = "",              // 空值
            NvrRemark10 = ""         // 空值
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 應有 13 個欄位 (標題 1 + 標籤 6 + 大字 6)
        Assert.Equal(13, commands.Count);

        // 所有欄位都不應被跳過 (QW075551-2 無 Barcode/QRCode)
        Assert.All(commands, c => Assert.False(c.Skip, $"欄位 {c.FieldName} 不應被跳過"));

        // 驗證特定欄位座標保留
        var csremarkCommand = commands.FirstOrDefault(c => c.FieldName == "CSREMARK");
        Assert.NotNull(csremarkCommand);
        Assert.Equal(55, csremarkCommand.X);  // 右欄 X=55
        Assert.Equal(string.Empty, csremarkCommand.Content);
    }

    /// <summary>
    /// T021: 驗證常數標籤欄位不受空值影響
    /// [ref: FR-009, spec.md Edge Case]
    ///
    /// 驗收條件：即使動態欄位為空，標籤前綴（如「D/C (LOT NO. ):」）仍正常顯示
    /// </summary>
    [Fact]
    public void Render_QW075551_2_EmptyValue_LabelPrefixPreserved()
    {
        // Arrange - 使用 QW075551-2 模板
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            NvrCustItemNo = "ITEM789",
            Ogd09 = "1000",
            NvrRemark10 = ""  // 只有 D/C 為空
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - D/C 標籤應正常顯示
        var dcLabelCommand = commands.FirstOrDefault(c => c.FieldName == "CSREMARK_Label");
        Assert.NotNull(dcLabelCommand);
        Assert.Equal("D/C (LOT NO. ):", dcLabelCommand.Content);

        // D/C 大字為空但不跳過
        var dcValueCommand = commands.FirstOrDefault(c => c.FieldName == "CSREMARK");
        Assert.NotNull(dcValueCommand);
        Assert.False(dcValueCommand.Skip);
        Assert.Equal(string.Empty, dcValueCommand.Content);
    }

    /// <summary>
    /// T021: 驗證數量欄位為 0 時顯示 "0" 而非空白
    /// [ref: spec.md Edge Case]
    ///
    /// 驗收條件：CSQTY 值為 "0" 時應顯示 "0"，不應視為空值
    /// </summary>
    [Fact]
    public void Render_QW075551_2_ZeroQuantity_DisplaysZero()
    {
        // Arrange - 使用 QW075551-2 模板
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            NvrCustItemNo = "ITEM789",
            Ogd09 = "0",  // 數量為 0
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - CSQTY 應顯示 "0"
        var qtyCommand = commands.FirstOrDefault(c => c.FieldName == "CSQTY");
        Assert.NotNull(qtyCommand);
        Assert.Equal("0", qtyCommand.Content);  // 不應為空
        Assert.False(qtyCommand.Skip);
    }

    #endregion

    #region Phase 7 Tests - QW075551-2 模板驗證 [ref: spec.md, tasks.md T022-T025]

    /// <summary>
    /// T022: 驗證 QW075551-2 模板欄位數量為 13 個
    /// [ref: spec.md FR-004, BuiltInTemplates.cs]
    ///
    /// 欄位組成：
    /// - 標題: 1 個
    /// - 小字行標籤: 6 個 (單號/代碼/ERP料號/規格型號/數量/D/C)
    /// - 大字行數值: 6 個 (對應上述欄位)
    /// 總計: 13 個
    ///
    /// [FIX] 原規格為 19 個，移除 6 個小字行數值欄位避免疊字問題
    /// </summary>
    [Fact]
    public void Template_QW075551_2_FieldCount_Returns13()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(13, template.Fields.Count);

        // 驗證欄位名稱組成
        var fieldNames = template.Fields.Select(f => f.Name).ToList();

        // 標題
        Assert.Contains("Title", fieldNames);

        // 小字行標籤 (6 個)
        Assert.Contains("CSCUSTPO_Label", fieldNames);
        Assert.Contains("CSNUMBER_Label", fieldNames);
        Assert.Contains("ERPPARTNO_Label", fieldNames);
        Assert.Contains("CSCUSTITEMNO_Label", fieldNames);
        Assert.Contains("CSQTY_Label", fieldNames);
        Assert.Contains("CSREMARK_Label", fieldNames);

        // 大字行數值 (6 個)
        Assert.Contains("CSCUSTPO", fieldNames);
        Assert.Contains("CSNUMBER", fieldNames);
        Assert.Contains("ERPPARTNO", fieldNames);
        Assert.Contains("CSCUSTITEMNO", fieldNames);
        Assert.Contains("CSQTY", fieldNames);
        Assert.Contains("CSREMARK", fieldNames);
    }

    /// <summary>
    /// T023: 驗證 QW075551-2 模板標籤尺寸為 100mm × 80mm
    /// [ref: spec.md FR-001, SC-002]
    ///
    /// 驗收條件：PDF 標籤尺寸精確為 100mm × 80mm
    /// </summary>
    [Fact]
    public void Template_QW075551_2_Size_Returns100x80mm()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");

        // Assert
        Assert.NotNull(template);
        Assert.Equal(100, template.WidthMm);
        Assert.Equal(80, template.HeightMm);
    }

    /// <summary>
    /// T023: 驗證 QW075551-2 模板標題為「物料標籤」
    /// [ref: spec.md FR-002, SC-007]
    ///
    /// 驗收條件：標籤標題顯示「物料標籤」，非「出貨標籤 Shipping Label」
    /// </summary>
    [Fact]
    public void Template_QW075551_2_Title_Returns物料標籤()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("物料標籤", template.Name);

        // 驗證 Title 欄位內容
        var titleField = template.Fields.FirstOrDefault(f => f.Name == "Title");
        Assert.NotNull(titleField);
        Assert.Equal("物料標籤", titleField.DataSource);
    }

    /// <summary>
    /// T023: 驗證 QW075551-2 模板具有外框
    /// [ref: spec.md FR-010, SC-008]
    ///
    /// 驗收條件：標籤具有外框（單線矩形邊框）
    /// </summary>
    [Fact]
    public void Template_QW075551_2_HasBorder_ReturnsTrue()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");

        // Assert
        Assert.NotNull(template);
        Assert.True(template.HasBorder, "QW075551-2 應具有外框");
    }

    /// <summary>
    /// T024: 驗證 QW075551-2 模板無任何 Barcode 欄位
    /// [ref: spec.md FR-011, SC-006]
    ///
    /// 驗收條件：標籤無 Barcode 元素
    /// </summary>
    [Fact]
    public void Template_QW075551_2_NoBarcode()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        // Act
        var barcodeFields = template.Fields
            .Where(f => f.FieldType == FieldType.Barcode)
            .ToList();

        // Assert
        Assert.Empty(barcodeFields);
    }

    /// <summary>
    /// T024: 驗證 QW075551-2 模板無任何 QRCode 欄位
    /// [ref: spec.md FR-011, SC-006]
    ///
    /// 驗收條件：標籤無 QR Code 元素
    /// </summary>
    [Fact]
    public void Template_QW075551_2_NoQRCode()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        // Act
        var qrCodeFields = template.Fields
            .Where(f => f.FieldType == FieldType.QRCode)
            .ToList();

        // Assert
        Assert.Empty(qrCodeFields);
    }

    /// <summary>
    /// T024: 驗證 QW075551-2 渲染結果無任何 Barcode/QRCode 指令
    /// [ref: spec.md FR-011, SC-006]
    ///
    /// 驗收條件：渲染後不應有 Barcode 或 QRCode 類型的 RenderCommand
    /// </summary>
    [Fact]
    public void Render_QW075551_2_NoBarcodeOrQRCodeCommands()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            NvrCustItemNo = "ITEM789",
            Ogd09 = "1000",
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - 不應有任何 Barcode 或 QRCode 指令
        var barcodeOrQRCodeCommands = commands
            .Where(c => c.CommandType == RenderCommandType.Barcode ||
                       c.CommandType == RenderCommandType.QRCode)
            .ToList();

        Assert.Empty(barcodeOrQRCodeCommands);
    }

    /// <summary>
    /// T025: 驗證 QW075551-2 CSQTY 欄位設定 UseDisplayValue = true
    /// [ref: spec.md FR-007]
    ///
    /// 驗收條件：CSQTY 欄位使用千分位格式化
    /// </summary>
    [Fact]
    public void Template_QW075551_2_CSQTY_UseDisplayValue_ReturnsTrue()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        // Act
        var csqtyField = template.Fields.FirstOrDefault(f => f.Name == "CSQTY");

        // Assert
        Assert.NotNull(csqtyField);
        Assert.True(csqtyField.UseDisplayValue, "CSQTY 應使用 DisplayValue (千分位)");
    }

    /// <summary>
    /// T025: 驗證 QW075551-2 CSQTY 千分位渲染結果
    /// [ref: spec.md FR-007, SC-004]
    ///
    /// 驗收條件：數量欄位正確顯示千分位格式（如 6733 → 6,733）
    /// </summary>
    [Fact]
    public void Render_QW075551_2_CSQTY_ThousandSeparator()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            NvrCustItemNo = "ITEM789",
            Ogd09 = "6733",  // 原始值
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert - CSQTY 應顯示千分位格式
        var qtyCommand = commands.FirstOrDefault(c => c.FieldName == "CSQTY");
        Assert.NotNull(qtyCommand);
        Assert.Equal("6,733", qtyCommand.Content);  // 千分位格式
    }

    /// <summary>
    /// T025: 驗證 QW075551-2 大數量千分位格式
    /// [ref: spec.md FR-007]
    ///
    /// 驗收條件：大數量（如 1234567）正確顯示為 1,234,567
    /// </summary>
    [Fact]
    public void Render_QW075551_2_CSQTY_LargeNumber_ThousandSeparator()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-2");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            Cscustpo = "PO123",
            Erpmat = "ERP456",
            NvrCustItemNo = "ITEM789",
            Ogd09 = "1234567",
            NvrRemark10 = "REMARK"
        };

        // Act
        var commands = _sut.Render(template, record);

        // Assert
        var qtyCommand = commands.FirstOrDefault(c => c.FieldName == "CSQTY");
        Assert.NotNull(qtyCommand);
        Assert.Equal("1,234,567", qtyCommand.Content);
    }

    #endregion

    #region Helper Methods

    private static LabelTemplate CreateSimpleTemplate(LabelField field)
    {
        return new LabelTemplate
        {
            Code = "TEST",
            Name = "Test Template",
            WidthMm = 100,
            HeightMm = 60,
            Fields = new List<LabelField> { field }
        };
    }

    #endregion
}
