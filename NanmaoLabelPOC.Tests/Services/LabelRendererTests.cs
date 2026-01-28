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

    #region Full Template Tests

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

        // Assert - 應有 17 個欄位
        Assert.Equal(17, commands.Count);

        // 驗證關鍵欄位
        var customerCommand = commands.First(c => c.FieldName == "CSCUSTOMER");
        Assert.Equal("XinshengIntel", customerCommand.Content);

        var qtyCommand = commands.First(c => c.FieldName == "CSQTY");
        Assert.Equal("6,733", qtyCommand.Content); // Display Value

        var barcodeCommand = commands.First(c => c.FieldName == "CSCUSTPN");
        Assert.Equal(RenderCommandType.Barcode, barcodeCommand.CommandType);
        Assert.Equal("E110-X0", barcodeCommand.Content); // Raw Value

        var qrCommand = commands.First(c => c.FieldName == "QRCODE");
        Assert.Equal(RenderCommandType.QRCode, qrCommand.CommandType);
        Assert.Equal("511-251020041;FC7800280H;6733;00U35NVVR", qrCommand.Content);
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
