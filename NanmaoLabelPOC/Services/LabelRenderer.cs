using System.Text.RegularExpressions;
using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 標籤渲染實作
/// [ref: raw_spec 2.5, 4.2, 13.2, 13.9, 13.13, 13.15]
///
/// 處理標籤欄位渲染邏輯：
/// - 變數對應：從 DataRecord 取值
/// - 常數值：直接輸出 DataSource
/// - Raw Value：Barcode/QRCode 使用（無格式化）
/// - Display Value：Text 使用（數量加千分位）
/// - Text 溢出：Ellipsis + 裁切
/// - Barcode/QRCode：禁止裁切
/// - QR Code 組合：空值保留分號位置 (A;;C)
/// </summary>
public partial class LabelRenderer : ILabelRenderer
{
    // 組合模式中的變數 placeholder 正規表達式
    // 匹配 {fieldName} 格式
    [GeneratedRegex(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled)]
    private static partial Regex CombinePatternRegex();

    /// <inheritdoc />
    public IReadOnlyList<RenderCommand> Render(LabelTemplate template, DataRecord record)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(record);

        var commands = new List<RenderCommand>();

        foreach (var field in template.Fields)
        {
            var command = RenderField(field, record);
            commands.Add(command);
        }

        return commands.AsReadOnly();
    }

    /// <summary>
    /// 渲染單一欄位
    /// </summary>
    private static RenderCommand RenderField(LabelField field, DataRecord record)
    {
        var commandType = field.FieldType switch
        {
            FieldType.Text => RenderCommandType.Text,
            FieldType.Barcode => RenderCommandType.Barcode,
            FieldType.QRCode => RenderCommandType.QRCode,
            _ => RenderCommandType.Text
        };

        var content = ResolveContent(field, record);
        var skip = ShouldSkip(field, content);

        return new RenderCommand
        {
            CommandType = commandType,
            Content = content,
            X = field.X,
            Y = field.Y,
            Width = field.Width,
            Height = field.Height,
            FontSize = field.FontSize,
            IsBold = field.IsBold,
            Alignment = field.Alignment,
            FieldName = field.Name,
            Skip = skip
        };
    }

    /// <summary>
    /// 解析欄位內容
    /// [ref: raw_spec 4.2, 13.13, 13.4, 13.15]
    /// </summary>
    private static string ResolveContent(LabelField field, DataRecord record)
    {
        // 常數值：直接回傳 DataSource
        // [ref: raw_spec 4.2]
        if (field.IsConstant)
        {
            return field.DataSource;
        }

        // QR Code 組合模式
        // [ref: raw_spec 13.4, 13.15]
        if (field.FieldType == FieldType.QRCode && !string.IsNullOrEmpty(field.CombinePattern))
        {
            return ResolveCombinePattern(field.CombinePattern, record);
        }

        // 變數對應
        // [ref: raw_spec 4.2, 13.13]
        var dataSource = field.DataSource;
        if (string.IsNullOrEmpty(dataSource))
        {
            return string.Empty;
        }

        // Barcode/QRCode 必須使用 Raw Value
        // Text 欄位依據 UseDisplayValue 決定
        // [ref: raw_spec 13.13]
        if (field.FieldType == FieldType.Barcode || field.FieldType == FieldType.QRCode)
        {
            return record.GetRawValue(dataSource);
        }

        return field.UseDisplayValue
            ? record.GetDisplayValue(dataSource)
            : record.GetRawValue(dataSource);
    }

    /// <summary>
    /// 解析 QR Code 組合模式
    /// [ref: raw_spec 13.4, 13.15]
    ///
    /// 規則：
    /// - 將 {fieldName} 替換為對應欄位的 Raw Value
    /// - 空值保留分號位置，產生 A;;C 格式
    /// - 欄位使用 Raw Value（無千分位）
    /// </summary>
    private static string ResolveCombinePattern(string pattern, DataRecord record)
    {
        return CombinePatternRegex().Replace(pattern, match =>
        {
            var fieldName = match.Groups[1].Value;
            // QR Code 組合使用 Raw Value [ref: raw_spec 13.13]
            return record.GetRawValue(fieldName);
        });
    }

    /// <summary>
    /// 判斷是否應略過此渲染
    /// [ref: raw_spec 13.15]
    ///
    /// 條碼/QR Code 內容為空時應略過（不產生空白圖片）
    /// T067: 條碼空內容略過
    /// </summary>
    private static bool ShouldSkip(LabelField field, string content)
    {
        // 條碼內容為空時略過 [T067]
        // [ref: raw_spec 13.15]
        if (field.FieldType == FieldType.Barcode && string.IsNullOrWhiteSpace(content))
        {
            return true;
        }

        // QR Code 若整個內容為空或僅有分號時，可能需要略過 [T068]
        // 但依據 raw_spec 13.15，空值欄位保留分號位置
        // 因此只有當全部欄位都為空（只剩分號）才考慮略過
        // 這裡保守處理：只有完全空白才略過
        if (field.FieldType == FieldType.QRCode)
        {
            var trimmed = content.Replace(";", "").Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    /// <summary>
    /// 驗證資料紀錄是否具備必要欄位
    /// [ref: raw_spec 3.3, 8.9]
    /// T069: 必要欄位缺失警告
    ///
    /// 必要欄位定義：Barcode 類型的欄位（條碼必須有內容才能掃描）
    /// </summary>
    public IReadOnlyList<string> ValidateRequiredFields(LabelTemplate template, DataRecord record)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(record);

        var missingFields = new List<string>();

        foreach (var field in template.Fields)
        {
            // Barcode 欄位為必要欄位（沒有內容無法產生可掃描的條碼）
            if (field.FieldType == FieldType.Barcode && !field.IsConstant)
            {
                var content = record.GetRawValue(field.DataSource);
                if (string.IsNullOrWhiteSpace(content))
                {
                    // 回傳對使用者友善的欄位名稱
                    missingFields.Add(GetFriendlyFieldName(field));
                }
            }
        }

        return missingFields.AsReadOnly();
    }

    /// <summary>
    /// 取得對使用者友善的欄位名稱
    /// [ref: raw_spec 13.21] 繁體中文訊息
    /// </summary>
    private static string GetFriendlyFieldName(LabelField field)
    {
        // 條碼欄位的友善名稱對應
        return field.Name switch
        {
            "CSCUSTPN" => "客戶 P/N (nvr_cust_pn)",
            "ERPPARTNO" => "ERP 料號 (erpmat)",
            _ => field.Name
        };
    }
}
