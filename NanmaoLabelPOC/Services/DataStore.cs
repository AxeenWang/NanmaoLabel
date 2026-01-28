using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Services;

/// <summary>
/// 資料儲存實作，負責 data.json 的讀寫操作
/// [ref: raw_spec 7.2, 3.1 F-01~F-02, 13.5, 附錄 B.1]
///
/// JSON 結構：
/// {
///   "version": "1.0",
///   "lastModified": "2025-01-26T14:30:52+08:00",
///   "records": [...]
/// }
/// </summary>
public class DataStore : IDataStore
{
    /// <summary>
    /// 資料目錄路徑 [ref: raw_spec 2.3]
    /// </summary>
    private const string DataDirectory = @".\data";

    /// <summary>
    /// 資料檔案名稱 [ref: raw_spec 2.3]
    /// </summary>
    private const string DataFileName = "data.json";

    /// <summary>
    /// 資料格式版本 [ref: raw_spec 附錄 B.1]
    /// </summary>
    private const string DataVersion = "1.0";

    /// <summary>
    /// JSON 序列化選項
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>
    /// 快取的資料紀錄
    /// </summary>
    private List<DataRecord> _records = new();

    /// <inheritdoc />
    public string DataFilePath => Path.Combine(DataDirectory, DataFileName);

    /// <inheritdoc />
    public bool DataFileExists => File.Exists(DataFilePath);

    /// <inheritdoc />
    public IReadOnlyList<DataRecord> Load()
    {
        if (!DataFileExists)
        {
            _records = new List<DataRecord>();
            return _records.AsReadOnly();
        }

        try
        {
            var json = File.ReadAllText(DataFilePath);
            var dataFile = JsonSerializer.Deserialize<DataFile>(json, JsonOptions);

            if (dataFile?.Records == null)
            {
                _records = new List<DataRecord>();
            }
            else
            {
                _records = dataFile.Records.ToList();
            }

            return _records.AsReadOnly();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("無法載入資料檔案：JSON 格式錯誤", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("無法載入資料檔案", ex);
        }
    }

    /// <inheritdoc />
    public void Save(IEnumerable<DataRecord> records)
    {
        EnsureDataDirectoryExists();

        var recordList = records.ToList();

        // 為新紀錄產生 UUID [ref: raw_spec 附錄 B.2]
        foreach (var record in recordList)
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                record.Id = Guid.NewGuid().ToString();
            }
        }

        var dataFile = new DataFile
        {
            Version = DataVersion,
            LastModified = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"), // ISO 8601 含時區 [ref: raw_spec 13.5]
            Records = recordList
        };

        var json = JsonSerializer.Serialize(dataFile, JsonOptions);
        File.WriteAllText(DataFilePath, json);

        _records = recordList;
    }

    /// <inheritdoc />
    public IReadOnlyList<DataRecord> GetAll()
    {
        return _records.AsReadOnly();
    }

    /// <summary>
    /// 確保資料目錄存在 [ref: raw_spec 2.3]
    /// </summary>
    private static void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }

    /// <summary>
    /// data.json 檔案結構 [ref: raw_spec 附錄 B.1]
    /// </summary>
    private class DataFile
    {
        /// <summary>
        /// 資料格式版本
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = DataVersion;

        /// <summary>
        /// 最後修改時間 (ISO 8601)
        /// </summary>
        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; } = string.Empty;

        /// <summary>
        /// 資料紀錄陣列
        /// </summary>
        [JsonPropertyName("records")]
        public List<DataRecord> Records { get; set; } = new();
    }
}
