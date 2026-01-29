using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using ExcelDataReader;
using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// Excel 匯入實作
/// [ref: raw_spec 7.2, 3.1 F-01, 3.3, 13.11, 13.14]
/// </summary>
public partial class ExcelImporter : IExcelImporter
{
    /// <summary>
    /// QR Code 組合欄位（需檢查分號）[ref: raw_spec 3.3, 13.4]
    /// </summary>
    private static readonly HashSet<string> QrCodeFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "pono", "ima902", "nvr_remark10", "erpmat", "cscustpo", "ogd09"
    };

    /// <summary>
    /// 數量欄位（僅允許數字）[ref: raw_spec 13.14]
    /// </summary>
    private static readonly HashSet<string> QuantityFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "ogd09"
    };

    /// <summary>
    /// 欄位名稱驗證正規表達式：允許英數字及底線 [ref: spec.md FR-001]
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z0-9_]+$")]
    private static partial Regex FieldNamePattern();

    /// <summary>
    /// 數字驗證正規表達式：僅允許純數字 [ref: raw_spec 13.14]
    /// </summary>
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsOnlyPattern();

    /// <summary>
    /// 千分位檢測正規表達式 [ref: raw_spec 13.14]
    /// </summary>
    [GeneratedRegex(@"^\d{1,3}(,\d{3})+$")]
    private static partial Regex ThousandSeparatorPattern();

    /// <summary>
    /// 已知的欄位名稱清單（用於識別額外欄位）[ref: raw_spec 附錄 A]
    /// </summary>
    private static readonly HashSet<string> KnownFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "ogb03", "ogb19", "ogb092", "ogb905", "ogd12b", "ogd12e",
        "ima902", "ogd15", "ogd09", "obe25",
        "nvr_cust", "nvrcust",
        "nvr_cust_item_no", "nvrcustitemno",
        "nvr_cust_pn", "nvrcustpn",
        "nvr_remark10", "nvrremark10",
        "pono", "erpmat", "cscustpo"
    };

    static ExcelImporter()
    {
        // ExcelDataReader 需要註冊編碼提供者以支援舊版 Excel 格式
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// 新增訊息至結果 [ref: spec.md FR-003, FR-009]
    /// </summary>
    private static void AddMessage(ImportResult result, MessageSeverity severity, string message, string? fieldName = null)
    {
        result.Messages.Add(new ImportMessage
        {
            Severity = severity,
            Message = message,
            FieldName = fieldName
        });
    }

    /// <inheritdoc />
    public ImportResult Import(string filePath)
    {
        var result = new ImportResult();

        // 檢查檔案是否存在 [ref: raw_spec 8.9, spec.md FR-004, FR-009]
        if (!File.Exists(filePath))
        {
            result.Success = false;
            result.ErrorMessage = "找不到指定的檔案";
            AddMessage(result, MessageSeverity.Error, "找不到指定的檔案");
            return result;
        }

        // 檢查檔案格式 [ref: raw_spec 8.9, spec.md FR-004, FR-009]
        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.ErrorMessage = "檔案格式不正確，請確認為 .xlsx 格式";
            AddMessage(result, MessageSeverity.Error, "檔案格式不正確，請確認為 .xlsx 格式");
            return result;
        }

        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true  // 首列為欄位名稱 [ref: raw_spec 3.3]
                }
            });

            if (dataSet.Tables.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Excel 檔案中沒有資料表";
                AddMessage(result, MessageSeverity.Error, "Excel 檔案中沒有資料表");
                return result;
            }

            var table = dataSet.Tables[0];
            var columnMapping = BuildColumnMapping(table, result);

            if (!result.Success)
            {
                return result;
            }

            // 讀取資料列
            var rowIndex = 0;
            var skippedEmptyRows = 0;
            foreach (DataRow row in table.Rows)
            {
                rowIndex++;
                // T016: 跳過空白列 [ref: raw_spec 3.3, spec.md FR-006, FR-009]
                if (IsEmptyRow(row, columnMapping))
                {
                    skippedEmptyRows++;
                    continue;
                }

                var record = ParseDataRecord(row, columnMapping, result);
                if (record != null)
                {
                    result.Records.Add(record);
                }
            }

            // T016: 若有空白列被略過，新增 Info 訊息 [ref: spec.md FR-006, FR-009]
            if (skippedEmptyRows > 0)
            {
                AddMessage(result, MessageSeverity.Info,
                    $"已略過 {skippedEmptyRows} 列空白資料列");
            }

            result.Success = true;
            return result;
        }
        catch (IOException ex)
        {
            result.Success = false;
            result.ErrorMessage = $"無法讀取檔案：{ex.Message}";
            AddMessage(result, MessageSeverity.Error, $"無法讀取檔案：{ex.Message}");
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"匯入失敗：{ex.Message}";
            AddMessage(result, MessageSeverity.Error, $"匯入失敗：{ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// 建立欄位名稱對應 [ref: raw_spec 3.3, 13.11, spec.md FR-006, FR-009]
    /// </summary>
    private static Dictionary<string, int> BuildColumnMapping(DataTable table, ImportResult result)
    {
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        result.Success = true;

        for (int i = 0; i < table.Columns.Count; i++)
        {
            var columnName = table.Columns[i].ColumnName?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(columnName))
            {
                continue;
            }

            // 欄位名稱驗證：允許英數字及底線 [ref: spec.md FR-001, FR-002]
            if (!FieldNamePattern().IsMatch(columnName))
            {
                // T015: 含空白、連字號、其他特殊符號視為非法欄位 [ref: spec.md FR-002, FR-006, FR-009]
                AddMessage(result, MessageSeverity.Info,
                    $"欄位名稱 '{columnName}' 包含非法字元（僅允許英數字及底線），已忽略",
                    columnName);
                continue;
            }

            // T014: 檢查是否為已知欄位 [ref: spec.md FR-006, FR-009]
            if (!KnownFields.Contains(columnName))
            {
                AddMessage(result, MessageSeverity.Info,
                    $"欄位 '{columnName}' 不在對應清單中，已忽略",
                    columnName);
            }

            // 不分大小寫對應 [ref: raw_spec 3.3, 13.11]
            if (!mapping.ContainsKey(columnName))
            {
                mapping[columnName] = i;
            }
        }

        return mapping;
    }

    /// <summary>
    /// 檢查是否為空白列 [ref: raw_spec 3.3]
    /// </summary>
    private static bool IsEmptyRow(DataRow row, Dictionary<string, int> columnMapping)
    {
        foreach (var colIndex in columnMapping.Values)
        {
            var value = GetCellValue(row, colIndex);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 解析資料紀錄
    /// </summary>
    private static DataRecord? ParseDataRecord(DataRow row, Dictionary<string, int> columnMapping, ImportResult result)
    {
        var record = new DataRecord
        {
            Id = Guid.NewGuid().ToString()  // 為新紀錄產生 UUID [ref: raw_spec 附錄 B.2]
        };

        // 對應欄位值
        record.Ogb03 = GetFieldValue(row, columnMapping, "ogb03", result);
        record.Ogb19 = GetFieldValue(row, columnMapping, "ogb19", result);
        record.Ogb092 = GetFieldValue(row, columnMapping, "ogb092", result);
        record.Ogb905 = GetFieldValue(row, columnMapping, "ogb905", result);
        record.Ogd12b = GetFieldValue(row, columnMapping, "ogd12b", result);
        record.Ogd12e = GetFieldValue(row, columnMapping, "ogd12e", result);
        record.Ima902 = GetFieldValue(row, columnMapping, "ima902", result);
        record.Ogd15 = GetFieldValue(row, columnMapping, "ogd15", result);
        record.Ogd09 = GetQuantityFieldValue(row, columnMapping, "ogd09", result);
        record.Obe25 = GetDateFieldValue(row, columnMapping, "obe25", result);
        record.NvrCust = GetFieldValue(row, columnMapping, "nvr_cust", result);
        record.NvrCustItemNo = GetFieldValue(row, columnMapping, "nvr_cust_item_no", result);
        record.NvrCustPn = GetFieldValue(row, columnMapping, "nvr_cust_pn", result);
        record.NvrRemark10 = GetFieldValue(row, columnMapping, "nvr_remark10", result);
        record.Pono = GetFieldValue(row, columnMapping, "pono", result);
        record.Erpmat = GetFieldValue(row, columnMapping, "erpmat", result);
        record.Cscustpo = GetFieldValue(row, columnMapping, "cscustpo", result);

        // 檢查分號警告 [ref: raw_spec 3.3, 13.4]
        CheckSemicolonWarnings(record, result);

        return record;
    }

    /// <summary>
    /// 取得欄位值（通用）
    /// </summary>
    private static string GetFieldValue(DataRow row, Dictionary<string, int> columnMapping, string fieldName, ImportResult result)
    {
        // 嘗試不同的欄位名稱格式（支援底線或不含底線）
        var normalizedName = fieldName.Replace("_", "");

        int colIndex;
        if (columnMapping.TryGetValue(fieldName, out colIndex) ||
            columnMapping.TryGetValue(normalizedName, out colIndex))
        {
            var value = GetCellValue(row, colIndex);
            return value?.Trim() ?? string.Empty;  // Trim 前後空白 [ref: raw_spec 3.3]
        }

        return string.Empty;  // 空白欄位允許 [ref: raw_spec 3.3]
    }

    /// <summary>
    /// 取得數量欄位值 [ref: raw_spec 13.14, spec.md FR-005, FR-009, FR-010]
    /// </summary>
    private static string GetQuantityFieldValue(DataRow row, Dictionary<string, int> columnMapping, string fieldName, ImportResult result)
    {
        var value = GetFieldValue(row, columnMapping, fieldName, result);

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // T017: 檢查是否含千分位 [ref: raw_spec 13.14, spec.md FR-005, FR-009, FR-010]
        if (ThousandSeparatorPattern().IsMatch(value))
        {
            AddMessage(result, MessageSeverity.Warning,
                $"數量欄位 '{fieldName}' 包含千分位格式 '{value}'，已自動移除",
                fieldName);
            // 移除千分位後繼續處理 [ref: spec.md FR-010]
            value = value.Replace(",", "");
        }

        // 驗證是否為純數字 [ref: raw_spec 13.14]
        if (!string.IsNullOrEmpty(value) && !DigitsOnlyPattern().IsMatch(value))
        {
            AddMessage(result, MessageSeverity.Warning,
                $"數量欄位 '{fieldName}' 值 '{value}' 包含非數字字元",
                fieldName);
        }

        return value;
    }

    /// <summary>
    /// 取得日期欄位值 [ref: raw_spec 3.3, 13.2]
    /// </summary>
    private static string GetDateFieldValue(DataRow row, Dictionary<string, int> columnMapping, string fieldName, ImportResult result)
    {
        var normalizedName = fieldName.Replace("_", "");

        int colIndex;
        if (!columnMapping.TryGetValue(fieldName, out colIndex) &&
            !columnMapping.TryGetValue(normalizedName, out colIndex))
        {
            return string.Empty;
        }

        var cellValue = row[colIndex];

        // 處理 DateTime 類型
        if (cellValue is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");  // 統一格式 [ref: raw_spec 3.3, 13.2]
        }

        // 處理字串類型
        var value = cellValue?.ToString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // 嘗試解析日期並統一格式
        if (DateTime.TryParse(value, out var parsedDate))
        {
            return parsedDate.ToString("yyyy-MM-dd");
        }

        // 嘗試常見日期格式
        string[] dateFormats = { "yyyy/MM/dd", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };
        foreach (var format in dateFormats)
        {
            if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd");
            }
        }

        // 無法解析則保留原值
        return value;
    }

    /// <summary>
    /// 取得儲存格值（強制轉為字串）[ref: raw_spec 3.1 備註]
    /// </summary>
    private static string GetCellValue(DataRow row, int colIndex)
    {
        if (colIndex < 0 || colIndex >= row.Table.Columns.Count)
        {
            return string.Empty;
        }

        var cellValue = row[colIndex];

        if (cellValue == null || cellValue == DBNull.Value)
        {
            return string.Empty;
        }

        // 所有內容視為字串（保留前導零）[ref: raw_spec 3.1 備註]
        return cellValue.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 檢查 QR Code 組合欄位的分號警告 [ref: raw_spec 3.3, 13.4, spec.md FR-005, FR-009]
    /// </summary>
    private static void CheckSemicolonWarnings(DataRecord record, ImportResult result)
    {
        var fieldsToCheck = new Dictionary<string, string>
        {
            { "pono", record.Pono },
            { "ima902", record.Ima902 },
            { "nvr_remark10", record.NvrRemark10 },
            { "erpmat", record.Erpmat },
            { "cscustpo", record.Cscustpo },
            { "ogd09", record.Ogd09 }
        };

        foreach (var (fieldName, value) in fieldsToCheck)
        {
            if (!string.IsNullOrEmpty(value) && value.Contains(';'))
            {
                // T018: 分號警告 [ref: spec.md FR-005, FR-009]
                AddMessage(result, MessageSeverity.Warning,
                    $"欄位 '{fieldName}' 值 '{value}' 包含分號，可能影響 QR Code",
                    fieldName);
            }
        }
    }
}
