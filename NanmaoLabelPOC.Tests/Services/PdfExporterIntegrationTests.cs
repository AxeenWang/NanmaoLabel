using System.IO;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.Templates;
using QuestPDF.Infrastructure;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// PdfExporter 整合測試
/// 用於驗證 PDF 輸出與參考圖片一致
/// </summary>
public class PdfExporterIntegrationTests
{
    private readonly PdfExporter _sut;

    public PdfExporterIntegrationTests()
    {
        // 設定 QuestPDF 社群授權
        QuestPDF.Settings.License = LicenseType.Community;

        var labelRenderer = new LabelRenderer();
        var barcodeGenerator = new BarcodeGenerator();
        _sut = new PdfExporter(labelRenderer, barcodeGenerator);
    }

    /// <summary>
    /// 輸出 QW075551-1 標籤 PDF 供視覺驗證
    /// 輸出位置：./output/test_qw075551_1.pdf
    /// </summary>
    [Fact]
    public void ExportSingle_QW075551_1_OutputPdfForVisualVerification()
    {
        // Arrange
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        // 使用與參考圖片相同的測試資料
        var record = new DataRecord
        {
            NvrCust = "XinshengIntelligent",
            Obe25 = "2025-11-17",
            Ogd09 = "1367",
            NvrCustItemNo = "HTE4A0SP-32GE",
            NvrCustPn = "PSUF22H17-B064SS",
            Pono = "521-251216041",
            Ima902 = "PC7800280L-00000-00387",
            NvrRemark10 = "4222310"
        };

        // 輸出到 .dao 目錄供視覺驗證
        var outputDir = @"D:\Daomen\NanmaoLabel\.dao";
        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, "test_qw075551_1.pdf");

        // Act
        var resultPath = _sut.ExportSingle(template, record, outputPath);

        // Assert
        Assert.True(File.Exists(resultPath));

        // 輸出檔案路徑供手動視覺驗證
        Console.WriteLine($"PDF 已輸出至: {Path.GetFullPath(resultPath)}");
        Console.WriteLine("請手動檢查 PDF 是否與參考圖片 (.dao/出貨標籤預覽_QW075551-1.png) 一致");
    }

    [Fact]
    public void Debug_RenderCommands_Output()
    {
        var template = BuiltInTemplates.GetByCode("QW075551-1");
        Assert.NotNull(template);

        var record = new DataRecord
        {
            NvrCust = "XinshengIntelligent",
            Obe25 = "2025-11-17",
            Ogd09 = "1367",
            NvrCustItemNo = "HTE4A0SP-32GE",
            NvrCustPn = "PSUF22H17-B064SS",
            Pono = "521-251216041",
            Ima902 = "PC7800280L-00000-00387",
            NvrRemark10 = "4222310"
        };

        var renderer = new LabelRenderer();
        var commands = renderer.Render(template, record);

        Console.WriteLine($"Total commands: {commands.Count}");
        Console.WriteLine();
        foreach (var cmd in commands)
        {
            Console.WriteLine($"Field: {cmd.FieldName}");
            Console.WriteLine($"  Type: {cmd.CommandType}");
            Console.WriteLine($"  Content: [{cmd.Content}]");
            Console.WriteLine($"  Position: X={cmd.X}, Y={cmd.Y}, W={cmd.Width}, H={cmd.Height}");
            Console.WriteLine($"  Skip: {cmd.Skip}");
            Console.WriteLine();
        }

        // 驗證關鍵欄位有內容
        var cscustomer = commands.FirstOrDefault(c => c.FieldName == "CSCUSTOMER");
        Assert.NotNull(cscustomer);
        Assert.Equal("XinshengIntelligent", cscustomer.Content);

        var cscustitemno = commands.FirstOrDefault(c => c.FieldName == "CSCUSTITEMNO");
        Assert.NotNull(cscustitemno);
        Assert.Equal("HTE4A0SP-32GE", cscustitemno.Content);

        var cscustpn = commands.FirstOrDefault(c => c.FieldName == "CSCUSTPN");
        Assert.NotNull(cscustpn);
        Assert.Equal("PSUF22H17-B064SS", cscustpn.Content);
    }
}
