using System.IO;
using System.Text.Json;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using Xunit;

namespace NanmaoLabelPOC.Tests.Services;

/// <summary>
/// DataStore 單元測試
/// [ref: raw_spec 7.2, 憲章 II, T019]
///
/// 測試項目：
/// - Load/Save JSON with version field [ref: raw_spec 附錄 B.1]
/// - lastModified update (ISO 8601)
/// - UUID generation for new records [ref: raw_spec 附錄 B.2]
/// - Auto-create directory
/// </summary>
public class DataStoreTests : IDisposable
{
    private readonly string _testDataDirectory;
    private readonly string _testDataFilePath;
    private readonly DataStore _dataStore;

    public DataStoreTests()
    {
        // 使用暫存目錄避免干擾實際資料
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"NanmaoLabelPOC_Tests_{Guid.NewGuid()}");
        _testDataFilePath = Path.Combine(_testDataDirectory, "data.json");

        // 切換到暫存目錄執行測試
        Directory.CreateDirectory(_testDataDirectory);
        Directory.SetCurrentDirectory(_testDataDirectory);

        _dataStore = new DataStore();
    }

    public void Dispose()
    {
        // 清理暫存目錄
        try
        {
            if (Directory.Exists(_testDataDirectory))
            {
                Directory.Delete(_testDataDirectory, true);
            }
        }
        catch
        {
            // 忽略清理失敗
        }
    }

    #region Load Tests

    /// <summary>
    /// 測試：檔案不存在時應回傳空清單
    /// </summary>
    [Fact]
    public void Load_WhenFileNotExists_ReturnsEmptyList()
    {
        // Arrange - 確保檔案不存在
        Assert.False(_dataStore.DataFileExists);

        // Act
        var result = _dataStore.Load();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// 測試：載入包含 version 欄位的 JSON [ref: raw_spec 附錄 B.1]
    /// </summary>
    [Fact]
    public void Load_JsonWithVersionField_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "version": "1.0",
            "lastModified": "2025-01-26T14:30:52+08:00",
            "records": [
                {
                    "id": "test-uuid-001",
                    "ogb19": "G25A111577",
                    "nvr_cust": "TSMC",
                    "ogd09": "6733"
                }
            ]
        }
        """;
        EnsureDataDirectoryExists();
        File.WriteAllText(_dataStore.DataFilePath, json);

        // Act
        var result = _dataStore.Load();

        // Assert
        Assert.Single(result);
        Assert.Equal("test-uuid-001", result[0].Id);
        Assert.Equal("G25A111577", result[0].Ogb19);
        Assert.Equal("TSMC", result[0].NvrCust);
        Assert.Equal("6733", result[0].Ogd09);
    }

    /// <summary>
    /// 測試：載入無效 JSON 應擲出 InvalidOperationException
    /// </summary>
    [Fact]
    public void Load_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        EnsureDataDirectoryExists();
        File.WriteAllText(_dataStore.DataFilePath, invalidJson);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _dataStore.Load());
        Assert.Contains("JSON 格式錯誤", exception.Message);
    }

    /// <summary>
    /// 測試：載入 records 為 null 的 JSON 應回傳空清單
    /// </summary>
    [Fact]
    public void Load_JsonWithNullRecords_ReturnsEmptyList()
    {
        // Arrange
        var json = """
        {
            "version": "1.0",
            "lastModified": "2025-01-26T14:30:52+08:00",
            "records": null
        }
        """;
        EnsureDataDirectoryExists();
        File.WriteAllText(_dataStore.DataFilePath, json);

        // Act
        var result = _dataStore.Load();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Save Tests

    /// <summary>
    /// 測試：儲存時應包含 version 欄位 [ref: raw_spec 附錄 B.1]
    /// </summary>
    [Fact]
    public void Save_ShouldIncludeVersionField()
    {
        // Arrange
        var records = new List<DataRecord>
        {
            new DataRecord { Ogb19 = "G25A111577" }
        };

        // Act
        _dataStore.Save(records);

        // Assert
        var json = File.ReadAllText(_dataStore.DataFilePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("version", out var version));
        Assert.Equal("1.0", version.GetString());
    }

    /// <summary>
    /// 測試：儲存時應更新 lastModified 為 ISO 8601 格式 [ref: raw_spec 13.5]
    /// </summary>
    [Fact]
    public void Save_ShouldUpdateLastModifiedWithIso8601Format()
    {
        // Arrange
        var records = new List<DataRecord>
        {
            new DataRecord { Ogb19 = "G25A111577" }
        };
        var beforeSave = DateTime.Now.AddSeconds(-1);

        // Act
        _dataStore.Save(records);

        // Assert
        var json = File.ReadAllText(_dataStore.DataFilePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("lastModified", out var lastModified));
        var lastModifiedStr = lastModified.GetString();
        Assert.NotNull(lastModifiedStr);

        // 驗證 ISO 8601 格式（含時區）
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2}$", lastModifiedStr);

        // 驗證時間合理
        var parsedTime = DateTimeOffset.Parse(lastModifiedStr);
        Assert.True(parsedTime >= beforeSave);
    }

    /// <summary>
    /// 測試：為新紀錄產生 UUID [ref: raw_spec 附錄 B.2]
    /// </summary>
    [Fact]
    public void Save_ShouldGenerateUuidForNewRecords()
    {
        // Arrange
        var records = new List<DataRecord>
        {
            new DataRecord { Id = "", Ogb19 = "G25A111577" },      // 空 ID
            new DataRecord { Id = null!, Ogb19 = "G25A111578" }   // null ID
        };

        // Act
        _dataStore.Save(records);

        // Assert
        var loadedRecords = _dataStore.Load();
        Assert.Equal(2, loadedRecords.Count);

        foreach (var record in loadedRecords)
        {
            Assert.False(string.IsNullOrEmpty(record.Id));
            Assert.True(Guid.TryParse(record.Id, out _), $"ID '{record.Id}' 不是有效的 UUID");
        }

        // 確認兩個 UUID 不同
        Assert.NotEqual(loadedRecords[0].Id, loadedRecords[1].Id);
    }

    /// <summary>
    /// 測試：保留已有 UUID 的紀錄 [ref: raw_spec 附錄 B.2]
    /// </summary>
    [Fact]
    public void Save_ShouldPreserveExistingUuids()
    {
        // Arrange
        var existingId = "existing-uuid-12345";
        var records = new List<DataRecord>
        {
            new DataRecord { Id = existingId, Ogb19 = "G25A111577" }
        };

        // Act
        _dataStore.Save(records);

        // Assert
        var loadedRecords = _dataStore.Load();
        Assert.Single(loadedRecords);
        Assert.Equal(existingId, loadedRecords[0].Id);
    }

    /// <summary>
    /// 測試：自動建立資料目錄 [ref: raw_spec 2.3]
    /// </summary>
    [Fact]
    public void Save_ShouldAutoCreateDirectory()
    {
        // Arrange - 確保目錄不存在
        var dataDir = Path.GetDirectoryName(_dataStore.DataFilePath)!;
        if (Directory.Exists(dataDir))
        {
            Directory.Delete(dataDir, true);
        }
        Assert.False(Directory.Exists(dataDir));

        var records = new List<DataRecord>
        {
            new DataRecord { Ogb19 = "G25A111577" }
        };

        // Act
        _dataStore.Save(records);

        // Assert
        Assert.True(Directory.Exists(dataDir));
        Assert.True(File.Exists(_dataStore.DataFilePath));
    }

    /// <summary>
    /// 測試：儲存空清單應產生有效 JSON
    /// </summary>
    [Fact]
    public void Save_EmptyList_ShouldCreateValidJson()
    {
        // Arrange
        var records = new List<DataRecord>();

        // Act
        _dataStore.Save(records);

        // Assert
        var json = File.ReadAllText(_dataStore.DataFilePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("version", out _));
        Assert.True(root.TryGetProperty("lastModified", out _));
        Assert.True(root.TryGetProperty("records", out var recordsElement));
        Assert.Equal(JsonValueKind.Array, recordsElement.ValueKind);
        Assert.Equal(0, recordsElement.GetArrayLength());
    }

    #endregion

    #region GetAll Tests

    /// <summary>
    /// 測試：GetAll 應回傳快取的資料
    /// </summary>
    [Fact]
    public void GetAll_AfterLoad_ReturnsLoadedRecords()
    {
        // Arrange
        var json = """
        {
            "version": "1.0",
            "lastModified": "2025-01-26T14:30:52+08:00",
            "records": [
                { "id": "uuid-001", "ogb19": "G25A111577" },
                { "id": "uuid-002", "ogb19": "G25A111578" }
            ]
        }
        """;
        EnsureDataDirectoryExists();
        File.WriteAllText(_dataStore.DataFilePath, json);
        _dataStore.Load();

        // Act
        var result = _dataStore.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// 測試：GetAll 應回傳儲存後的資料
    /// </summary>
    [Fact]
    public void GetAll_AfterSave_ReturnsSavedRecords()
    {
        // Arrange
        var records = new List<DataRecord>
        {
            new DataRecord { Id = "uuid-001", Ogb19 = "G25A111577" },
            new DataRecord { Id = "uuid-002", Ogb19 = "G25A111578" },
            new DataRecord { Id = "uuid-003", Ogb19 = "G25A111579" }
        };
        _dataStore.Save(records);

        // Act
        var result = _dataStore.GetAll();

        // Assert
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region DataFilePath Tests

    /// <summary>
    /// 測試：DataFilePath 應回傳正確路徑 [ref: raw_spec 2.3]
    /// </summary>
    [Fact]
    public void DataFilePath_ShouldReturnCorrectPath()
    {
        // Act
        var path = _dataStore.DataFilePath;

        // Assert
        Assert.EndsWith("data.json", path);
        Assert.Contains("data", path);
    }

    #endregion

    #region Helper Methods

    private void EnsureDataDirectoryExists()
    {
        var dir = Path.GetDirectoryName(_dataStore.DataFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    #endregion
}
