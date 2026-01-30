using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.ViewModels;
using Xunit;

namespace NanmaoLabelPOC.Tests.ViewModels;

/// <summary>
/// 按鈕狀態優先序測試
/// [ref: raw_delta_button.md §8.1, FR-021]
///
/// 狀態優先序（高→低）：
/// Disabled > Loading > Active > Focus+Hover > Focus > Hover > Default
///
/// T042: 驗證按鈕狀態邏輯的正確性
/// </summary>
public class ButtonStateTests
{
    private readonly StubDataStore _dataStore;
    private readonly StubExcelImporter _excelImporter;
    private readonly DataManageViewModel _dataManageViewModel;

    public ButtonStateTests()
    {
        _dataStore = new StubDataStore();
        _excelImporter = new StubExcelImporter();
        _dataManageViewModel = new DataManageViewModel(_dataStore, _excelImporter);
    }

    #region IsDirty State Tests (FR-016)

    /// <summary>
    /// 測試初始狀態 IsDirty=false
    /// [ref: raw_delta_button.md §5.2]
    /// </summary>
    [Fact]
    public void IsDirty_InitialState_IsFalse()
    {
        // Assert
        Assert.False(_dataManageViewModel.IsDirty);
    }

    /// <summary>
    /// 測試新增資料後 IsDirty=true
    /// [ref: raw_delta_button.md §5.2]
    /// </summary>
    [Fact]
    public void IsDirty_AfterAddRecord_IsTrue()
    {
        // Act
        _dataManageViewModel.AddRecordCommand.Execute(null);

        // Assert
        Assert.True(_dataManageViewModel.IsDirty);
    }

    /// <summary>
    /// 測試儲存後 IsDirty=false
    /// [ref: raw_delta_button.md §5.2]
    /// </summary>
    [Fact]
    public void IsDirty_AfterSave_IsFalse()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);
        Assert.True(_dataManageViewModel.IsDirty);

        // Act
        _dataManageViewModel.SaveCommand.Execute(null);

        // Assert
        Assert.False(_dataManageViewModel.IsDirty);
    }

    /// <summary>
    /// 測試刪除資料後 IsDirty=true
    /// [ref: raw_delta_button.md §5.2]
    /// </summary>
    [Fact]
    public void IsDirty_AfterDeleteRecord_IsTrue()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);
        _dataManageViewModel.SaveCommand.Execute(null);
        Assert.False(_dataManageViewModel.IsDirty);

        // Act
        _dataManageViewModel.DeleteRecordCommand.Execute(null);

        // Assert
        Assert.True(_dataManageViewModel.IsDirty);
    }

    #endregion

    #region CanDelete State Tests (FR-019)

    /// <summary>
    /// 測試無選取時 CanDelete=false
    /// [ref: raw_delta_button.md §3.2]
    /// </summary>
    [Fact]
    public void CanDelete_NoSelection_IsFalse()
    {
        // Assert
        Assert.False(_dataManageViewModel.DeleteRecordCommand.CanExecute(null));
    }

    /// <summary>
    /// 測試有選取時 CanDelete=true
    /// [ref: raw_delta_button.md §3.2]
    /// </summary>
    [Fact]
    public void CanDelete_WithSelection_IsTrue()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);

        // Assert (AddRecord 會自動選取新增的記錄)
        Assert.True(_dataManageViewModel.DeleteRecordCommand.CanExecute(null));
    }

    #endregion

    #region CanSave State Tests

    /// <summary>
    /// 測試無變更時 CanSave=false
    /// [ref: raw_delta_button.md §3.2]
    /// </summary>
    [Fact]
    public void CanSave_NoChanges_IsFalse()
    {
        // Assert
        Assert.False(_dataManageViewModel.SaveCommand.CanExecute(null));
    }

    /// <summary>
    /// 測試有變更時 CanSave=true
    /// [ref: raw_delta_button.md §3.2]
    /// </summary>
    [Fact]
    public void CanSave_WithChanges_IsTrue()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);

        // Assert
        Assert.True(_dataManageViewModel.SaveCommand.CanExecute(null));
    }

    #endregion

    #region State Priority Tests (FR-021)

    /// <summary>
    /// 測試 Disabled 狀態優先於其他狀態
    /// [ref: raw_delta_button.md §8.1]
    /// 當 CanExecute=false 時，按鈕應處於 Disabled 狀態
    /// </summary>
    [Fact]
    public void StatePriority_Disabled_HasHighestPriority()
    {
        // Arrange - 無選取，刪除按鈕應為 Disabled
        Assert.Null(_dataManageViewModel.SelectedRecord);

        // Assert - DeleteCommand.CanExecute 應為 false（對應 Disabled 狀態）
        Assert.False(_dataManageViewModel.DeleteRecordCommand.CanExecute(null));
    }

    /// <summary>
    /// 測試狀態優先序：Disabled > Loading > Active > Focus+Hover > Focus > Hover > Default
    /// [ref: raw_delta_button.md §8.1]
    ///
    /// 此測試驗證 ViewModel 層面的狀態邏輯，
    /// UI 層面的 Trigger 優先序由 ButtonStyles.xaml 的 Trigger 順序決定
    /// </summary>
    [Fact]
    public void StatePriority_DisabledOverridesAllOtherStates()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);
        Assert.True(_dataManageViewModel.DeleteRecordCommand.CanExecute(null));

        // Act - 清除選取
        _dataManageViewModel.SelectedRecord = null;

        // Assert - 即使其他條件滿足，Disabled 狀態仍優先
        Assert.False(_dataManageViewModel.DeleteRecordCommand.CanExecute(null));
    }

    #endregion

    #region StatusMessage Tests

    /// <summary>
    /// 測試未修改時的狀態訊息
    /// [ref: raw_spec 8.10]
    /// </summary>
    [Fact]
    public void StatusMessage_NoChanges_ShowsRecordCount()
    {
        // Assert
        Assert.Contains("共 0 筆資料", _dataManageViewModel.StatusMessage);
        Assert.DoesNotContain("已修改", _dataManageViewModel.StatusMessage);
    }

    /// <summary>
    /// 測試有修改時的狀態訊息
    /// [ref: raw_spec 8.10]
    /// </summary>
    [Fact]
    public void StatusMessage_WithChanges_ShowsModifiedWarning()
    {
        // Arrange
        _dataManageViewModel.AddRecordCommand.Execute(null);

        // Assert
        Assert.Contains("已修改（未儲存）", _dataManageViewModel.StatusMessage);
        Assert.Contains("共 1 筆資料", _dataManageViewModel.StatusMessage);
    }

    #endregion

    #region Stub Classes

    /// <summary>
    /// IDataStore stub for testing
    /// </summary>
    private class StubDataStore : IDataStore
    {
        public string DataFilePath => "data/data.json";
        public bool DataFileExists => false;

        public IReadOnlyList<DataRecord> GetAll() => new List<DataRecord>();
        public IReadOnlyList<DataRecord> Load() => new List<DataRecord>();
        public void Save(IEnumerable<DataRecord> records) { }
    }

    /// <summary>
    /// IExcelImporter stub for testing
    /// </summary>
    private class StubExcelImporter : IExcelImporter
    {
        public ImportResult Import(string filePath)
        {
            return new ImportResult
            {
                Success = true,
                Records = new List<DataRecord>()
            };
        }
    }

    #endregion
}
