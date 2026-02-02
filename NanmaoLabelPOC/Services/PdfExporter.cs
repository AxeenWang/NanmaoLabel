using System.IO;
using NanmaoLabelPOC.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// PDF 輸出實作
/// [ref: raw_spec 3.1 F-08, 5.1, 5.3, 6.1, 13.1, 13.2, 13.6, 13.17]
///
/// 規格：
/// - 頁面尺寸: 100mm × 60mm [ref: raw_spec 5.1]
/// - 字型: 微軟正黑體 (Microsoft JhengHei) [ref: raw_spec 5.3]
/// - 字型嵌入: PDF 輸出時嵌入 [ref: raw_spec 5.3]
/// - 條碼下方文字: 8pt, 置中 [ref: raw_spec 6.1]
/// - 輸出目錄: .\output\ [ref: raw_spec 2.3]
/// - 檔名格式: Label_{ogb19}_{yyyyMMdd_HHmmss}.pdf [ref: raw_spec 13.6]
/// </summary>
public class PdfExporter : IPdfExporter
{
    private readonly ILabelRenderer _labelRenderer;
    private readonly IBarcodeGenerator _barcodeGenerator;

    /// <summary>
    /// 條碼下方文字字型大小 (pt) [ref: raw_spec 6.1]
    /// </summary>
    private const float BarcodeTextFontSize = 8f;

    /// <summary>
    /// 標籤外框線條粗細 (pt) [ref: FR-003]
    /// </summary>
    private const float BorderThickness = 0.5f;

    /// <summary>
    /// 預設輸出目錄 [ref: raw_spec 2.3]
    /// </summary>
    public string OutputDirectory => Path.Combine(".", "output");

    /// <summary>
    /// 字型名稱: 微軟正黑體 [ref: raw_spec 5.3]
    /// </summary>
    private const string FontFamily = "Microsoft JhengHei";

    public PdfExporter(ILabelRenderer labelRenderer, IBarcodeGenerator barcodeGenerator)
    {
        _labelRenderer = labelRenderer ?? throw new ArgumentNullException(nameof(labelRenderer));
        _barcodeGenerator = barcodeGenerator ?? throw new ArgumentNullException(nameof(barcodeGenerator));
    }

    /// <inheritdoc />
    public string ExportSingle(LabelTemplate template, DataRecord record, string? outputPath = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(record);

        // 決定輸出路徑
        var finalPath = outputPath ?? GetDefaultOutputPath(record);

        // 確保輸出目錄存在 [ref: raw_spec 2.3]
        var directory = Path.GetDirectoryName(finalPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 取得渲染指令
        var commands = _labelRenderer.Render(template, record);

        // 產生 PDF
        var document = CreateDocument(template, commands);
        document.GeneratePdf(finalPath);

        return finalPath;
    }

    /// <inheritdoc />
    public string GenerateDefaultFileName(DataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        // 格式: Label_{ogb19}_{yyyyMMdd_HHmmss}.pdf [ref: raw_spec 13.6]
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var ogb19 = string.IsNullOrWhiteSpace(record.Ogb19) ? "NoId" : SanitizeFileName(record.Ogb19);
        return $"Label_{ogb19}_{timestamp}.pdf";
    }

    /// <summary>
    /// 取得預設輸出路徑
    /// </summary>
    private string GetDefaultOutputPath(DataRecord record)
    {
        // 確保輸出目錄存在 [ref: raw_spec 2.3]
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }

        var fileName = GenerateDefaultFileName(record);
        return Path.Combine(OutputDirectory, fileName);
    }

    /// <summary>
    /// 清理檔名中的非法字元
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    /// <summary>
    /// 建立 PDF 文件
    /// </summary>
    private IDocument CreateDocument(LabelTemplate template, IReadOnlyList<RenderCommand> commands)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // 頁面尺寸: 100mm × 60mm [ref: raw_spec 5.1]
                page.Size((float)template.WidthMm, (float)template.HeightMm, Unit.Millimetre);
                page.Margin(0);

                // 使用 Layers 來定位元素
                page.Content().Layers(layers =>
                {
                    // 底層：白色背景
                    layers.Layer().Background(Colors.White);

                    // 標籤外框 [FR-003]
                    // 單線矩形邊框，0.5pt，無分隔線
                    if (template.HasBorder)
                    {
                        layers.Layer().Border(BorderThickness).BorderColor(Colors.Black);
                    }

                    // 渲染每個指令
                    foreach (var command in commands)
                    {
                        if (command.Skip)
                            continue;

                        layers.Layer().Element(element =>
                        {
                            RenderCommand(element, command, template);
                        });
                    }
                });
            });
        });
    }

    /// <summary>
    /// 渲染單一指令
    /// </summary>
    private void RenderCommand(IContainer container, RenderCommand command, LabelTemplate template)
    {
        // 座標和尺寸使用 mm [ref: raw_spec 13.1]
        var x = (float)command.X;
        var y = (float)command.Y;
        var width = (float)command.Width;
        var height = (float)command.Height;

        // 使用 Padding 來定位元素
        container
            .PaddingLeft(x, Unit.Millimetre)
            .PaddingTop(y, Unit.Millimetre)
            .Width(width, Unit.Millimetre)
            .Height(height, Unit.Millimetre)
            .Element(inner =>
            {
                switch (command.CommandType)
                {
                    case RenderCommandType.Text:
                        RenderText(inner, command);
                        break;

                    case RenderCommandType.Barcode:
                        RenderBarcode(inner, command);
                        break;

                    case RenderCommandType.QRCode:
                        RenderQRCode(inner, command);
                        break;
                }
            });
    }

    /// <summary>
    /// 渲染文字
    /// [ref: raw_spec 5.3, 13.2, FR-008]
    ///
    /// 支援長文字縮小字體處理：
    /// - 使用 ActualFontSize（若有計算結果）
    /// - RequiresWrap = true 時允許換行或截斷加省略號
    /// </summary>
    private static void RenderText(IContainer container, RenderCommand command)
    {
        if (string.IsNullOrEmpty(command.Content))
            return;

        // 優先使用計算後的 ActualFontSize [FR-008]
        var fontSize = (float)(command.ActualFontSize ?? command.FontSize ?? 10);

        container
            .AlignMiddle()
            .Text(text =>
            {
                text.DefaultTextStyle(style =>
                {
                    var styledText = style
                        .FontFamily(FontFamily)
                        .FontSize(fontSize);

                    if (command.IsBold)
                        styledText = styledText.Bold();

                    return styledText;
                });

                // 設定對齊方式
                switch (command.Alignment)
                {
                    case TextAlignment.Center:
                        text.AlignCenter();
                        break;
                    case TextAlignment.Right:
                        text.AlignRight();
                        break;
                    default:
                        text.AlignLeft();
                        break;
                }

                // 處理溢出 [FR-008]
                // RequiresWrap = true: 允許換行，最多兩行，超過時截斷加省略號
                // RequiresWrap = false: 單行，超過時裁切並加 Ellipsis [ref: raw_spec 13.2]
                if (command.RequiresWrap)
                {
                    // 允許換行，最多兩行
                    text.ClampLines(2, "...");
                }
                else
                {
                    text.ClampLines(1, "...");
                }

                text.Span(command.Content);
            });
    }

    /// <summary>
    /// 渲染 Code 128 條碼
    /// [ref: raw_spec 6.1]
    /// </summary>
    private void RenderBarcode(IContainer container, RenderCommand command)
    {
        if (string.IsNullOrEmpty(command.Content))
            return;

        // 條碼高度：預留 3mm 給下方文字
        var textHeightMm = 3.0;
        var barcodeHeightMm = command.Height - textHeightMm;

        // 生成條碼圖片
        var barcodeBytes = _barcodeGenerator.GenerateCode128(command.Content, barcodeHeightMm);
        if (barcodeBytes.Length == 0)
            return;

        container
            .Column(column =>
            {
                // 條碼圖片
                column.Item()
                    .Height((float)barcodeHeightMm, Unit.Millimetre)
                    .AlignLeft()
                    .Image(barcodeBytes)
                    .FitHeight();

                // 條碼下方文字 [ref: raw_spec 6.1]
                column.Item()
                    .Height((float)textHeightMm, Unit.Millimetre)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(style => style
                            .FontFamily(FontFamily)
                            .FontSize(BarcodeTextFontSize));
                        text.AlignCenter();
                        text.Span(command.Content);
                    });
            });
    }

    /// <summary>
    /// 渲染 QR Code
    /// [ref: raw_spec 6.2]
    /// </summary>
    private void RenderQRCode(IContainer container, RenderCommand command)
    {
        if (string.IsNullOrEmpty(command.Content))
            return;

        // 使用較小的邊長確保正方形
        var sizeMm = Math.Min(command.Width, command.Height);

        // 生成 QR Code 圖片
        var qrBytes = _barcodeGenerator.GenerateQRCode(command.Content, sizeMm);
        if (qrBytes.Length == 0)
            return;

        container
            .Width((float)sizeMm, Unit.Millimetre)
            .Height((float)sizeMm, Unit.Millimetre)
            .AlignCenter()
            .AlignMiddle()
            .Image(qrBytes)
            .FitArea();
    }

    /// <inheritdoc />
    public string ExportBatch(LabelTemplate template, IEnumerable<DataRecord> records, string? outputPath = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(records);

        var recordList = records.ToList();
        if (recordList.Count == 0)
        {
            throw new ArgumentException("批次輸出至少需要一筆資料", nameof(records));
        }

        // 決定輸出路徑
        var finalPath = outputPath ?? GetDefaultBatchOutputPath();

        // 確保輸出目錄存在 [ref: raw_spec 2.3]
        var directory = Path.GetDirectoryName(finalPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 產生多頁 PDF
        var document = CreateBatchDocument(template, recordList);
        document.GeneratePdf(finalPath);

        return finalPath;
    }

    /// <inheritdoc />
    public string GenerateBatchFileName()
    {
        // 格式: Labels_Batch_{yyyyMMdd_HHmmss}.pdf [ref: raw_spec 13.6]
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"Labels_Batch_{timestamp}.pdf";
    }

    /// <summary>
    /// 取得批次輸出預設路徑
    /// </summary>
    private string GetDefaultBatchOutputPath()
    {
        // 確保輸出目錄存在 [ref: raw_spec 2.3]
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }

        var fileName = GenerateBatchFileName();
        return Path.Combine(OutputDirectory, fileName);
    }

    /// <inheritdoc />
    public bool FileExists(string outputPath)
    {
        return File.Exists(outputPath);
    }

    /// <summary>
    /// 建立批次 PDF 文件（多頁）
    /// [ref: raw_spec 3.3 批次輸出規格]
    /// </summary>
    private IDocument CreateBatchDocument(LabelTemplate template, IReadOnlyList<DataRecord> records)
    {
        return Document.Create(container =>
        {
            // 每筆資料一頁 [ref: raw_spec 3.3]
            foreach (var record in records)
            {
                var commands = _labelRenderer.Render(template, record);

                container.Page(page =>
                {
                    // 頁面尺寸: 100mm × 60mm [ref: raw_spec 5.1]
                    page.Size((float)template.WidthMm, (float)template.HeightMm, Unit.Millimetre);
                    page.Margin(0);

                    // 使用 Layers 來定位元素
                    page.Content().Layers(layers =>
                    {
                        // 底層：白色背景
                        layers.Layer().Background(Colors.White);

                        // 標籤外框 [FR-003]
                        // 單線矩形邊框，0.5pt，無分隔線
                        if (template.HasBorder)
                        {
                            layers.Layer().Border(BorderThickness).BorderColor(Colors.Black);
                        }

                        // 渲染每個指令
                        foreach (var command in commands)
                        {
                            if (command.Skip)
                                continue;

                            layers.Layer().Element(element =>
                            {
                                RenderCommand(element, command, template);
                            });
                        }
                    });
                });
            }
        });
    }
}
