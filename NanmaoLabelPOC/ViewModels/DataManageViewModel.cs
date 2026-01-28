using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;

namespace NanmaoLabelPOC.ViewModels;

/// <summary>
/// 資料管理分頁 ViewModel
/// [ref: raw_spec 8.5, 3.2]
///
/// 功能：
/// - Excel 匯入 [ref: raw_spec F-01, 3.3]
/// - 資料新增/刪除/編輯 [ref: raw_spec F-10~F-12]
/// - 資料儲存 [ref: raw_spec F-13]
/// - IsDirty 追蹤 [ref: raw_spec 8.10]
/// </summary>
public partial class DataManageViewModel : ObservableObject
{
    private readonly IDataStore _dataStore;
    private readonly IExcelImporter _excelImporter;

    /// <summary>
    /// 數字驗證正規表達式 [ref: raw_spec 13.14]
    /// </summary>
    private static readonly Regex DigitsOnlyPattern = new(@"^\d*$", RegexOptions.Compiled);

    public DataManageViewModel(IDataStore dataStore, IExcelImporter excelImporter)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        _excelImporter = excelImporter ?? throw new ArgumentNullException(nameof(excelImporter));

        Records = new ObservableCollection<DataRecord>();
    }

    #region Properties

    /// <summary>
    /// 資料清單
    /// </summary>
    public ObservableCollection<DataRecord> Records { get; }

    /// <summary>
    /// 選取的資料紀錄
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedRecord))]
    [NotifyCanExecuteChangedFor(nameof(DeleteRecordCommand))]
    private DataRecord? _selectedRecord;

    /// <summary>
    /// 是否有選取的資料紀錄
    /// </summary>
    public bool HasSelectedRecord => SelectedRecord != null;

    /// <summary>
    /// 是否有未儲存的變更 [ref: raw_spec 8.10]
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isDirty;

    /// <summary>
    /// 是否有資料
    /// </summary>
    public bool HasRecords => Records.Count > 0;

    /// <summary>
    /// 狀態列訊息 [ref: raw_spec 8.10]
    /// T057: 未儲存時顯示 "⚠️ 已修改（未儲存）　共 N 筆資料"
    /// </summary>
    public string StatusMessage
    {
        get
        {
            if (IsDirty)
            {
                return $"⚠️ 已修改（未儲存）　共 {Records.Count} 筆資料";
            }
            return $"共 {Records.Count} 筆資料";
        }
    }

    /// <summary>
    /// 最後儲存的訊息（供 View 顯示）
    /// T058: 儲存成功後顯示 "✅ 儲存成功"
    /// </summary>
    [ObservableProperty]
    private string? _lastSaveMessage;

    /// <summary>
    /// 資料筆數顯示
    /// </summary>
    public string RecordCountText => $"共 {Records.Count} 筆資料";

    /// <summary>
    /// 最後的匯入結果（供 View 顯示警告）
    /// </summary>
    [ObservableProperty]
    private ImportResult? _lastImportResult;

    #endregion

    #region Commands

    /// <summary>
    /// 匯入 Excel
    /// [ref: raw_spec F-01, 3.3]
    /// </summary>
    public ImportResult? ImportExcel(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var result = _excelImporter.Import(filePath);
        LastImportResult = result;

        if (result.Success)
        {
            // 覆蓋模式 [ref: raw_spec 13.5]
            Records.Clear();
            foreach (var record in result.Records)
            {
                Records.Add(record);
            }

            IsDirty = true;  // 匯入後標記為已修改
            OnPropertyChanged(nameof(HasRecords));
            OnPropertyChanged(nameof(RecordCountText));
            OnPropertyChanged(nameof(StatusMessage));
        }

        return result;
    }

    /// <summary>
    /// 新增資料命令 [ref: raw_spec F-10]
    /// T052: 新增一筆空白資料列
    /// </summary>
    [RelayCommand]
    private void AddRecord()
    {
        var newRecord = new DataRecord
        {
            Id = Guid.NewGuid().ToString()
        };
        Records.Add(newRecord);
        SelectedRecord = newRecord;
        MarkDirty();
        OnPropertyChanged(nameof(HasRecords));
        OnPropertyChanged(nameof(RecordCountText));
    }

    /// <summary>
    /// 刪除資料命令 [ref: raw_spec F-12]
    /// T053: 刪除選取的資料列（需在 View 層顯示確認對話框）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteRecord))]
    private void DeleteRecord()
    {
        if (SelectedRecord == null)
            return;

        var recordToDelete = SelectedRecord;
        var index = Records.IndexOf(recordToDelete);
        Records.Remove(recordToDelete);

        // 選取下一筆或上一筆
        if (Records.Count > 0)
        {
            SelectedRecord = Records[Math.Min(index, Records.Count - 1)];
        }
        else
        {
            SelectedRecord = null;
        }

        MarkDirty();
        OnPropertyChanged(nameof(HasRecords));
        OnPropertyChanged(nameof(RecordCountText));
    }

    private bool CanDeleteRecord() => SelectedRecord != null;

    /// <summary>
    /// 儲存命令
    /// [ref: raw_spec F-13]
    /// T058: 儲存成功後顯示 "✅ 儲存成功"
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            _dataStore.Save(Records);
            IsDirty = false;
            LastSaveMessage = "✅ 儲存成功";
            OnPropertyChanged(nameof(StatusMessage));
        }
        catch (Exception)
        {
            LastSaveMessage = null;
            // 錯誤處理在 View 層
            throw;
        }
    }

    private bool CanSave() => IsDirty;

    /// <summary>
    /// 載入資料命令
    /// </summary>
    [RelayCommand]
    private void LoadData()
    {
        try
        {
            var records = _dataStore.Load();
            Records.Clear();
            foreach (var record in records)
            {
                Records.Add(record);
            }

            IsDirty = false;
            OnPropertyChanged(nameof(HasRecords));
            OnPropertyChanged(nameof(RecordCountText));
            OnPropertyChanged(nameof(StatusMessage));
        }
        catch
        {
            // 載入失敗時清空
            Records.Clear();
            IsDirty = false;
            OnPropertyChanged(nameof(HasRecords));
            OnPropertyChanged(nameof(RecordCountText));
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// 標記資料已變更
    /// </summary>
    public void MarkDirty()
    {
        if (!IsDirty)
        {
            IsDirty = true;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    /// <summary>
    /// 刷新 UI 狀態
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(HasRecords));
        OnPropertyChanged(nameof(RecordCountText));
        OnPropertyChanged(nameof(StatusMessage));
    }

    /// <summary>
    /// 驗證數量欄位 [ref: raw_spec 13.14]
    /// T059: 數量欄位僅允許數字
    /// </summary>
    /// <param name="value">欄位值</param>
    /// <returns>是否為有效數字格式</returns>
    public static bool ValidateQuantityField(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;  // 空值允許

        return DigitsOnlyPattern.IsMatch(value);
    }

    /// <summary>
    /// 驗證必要欄位 [ref: raw_spec 3.3]
    /// T060: 檢查必要欄位是否填寫
    /// </summary>
    /// <param name="record">資料紀錄</param>
    /// <returns>驗證結果，包含錯誤訊息清單</returns>
    public static (bool IsValid, List<string> Errors) ValidateRequiredFields(DataRecord record)
    {
        var errors = new List<string>();

        // 標籤必要欄位檢查 [ref: raw_spec 3.3]
        // 單據編號 (ogb19) 為識別欄位
        if (string.IsNullOrWhiteSpace(record.Ogb19))
        {
            errors.Add("單據編號 (ogb19)");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// 儲存前驗證所有資料
    /// </summary>
    /// <returns>驗證結果</returns>
    public (bool IsValid, List<string> Errors) ValidateAllRecords()
    {
        var allErrors = new List<string>();

        for (int i = 0; i < Records.Count; i++)
        {
            var record = Records[i];

            // T059: 數量欄位驗證
            if (!ValidateQuantityField(record.Ogd09))
            {
                allErrors.Add($"第 {i + 1} 筆：數量欄位 '{record.Ogd09}' 包含非數字字元");
            }

            // T060: 必要欄位驗證
            var (isValid, fieldErrors) = ValidateRequiredFields(record);
            if (!isValid)
            {
                allErrors.Add($"第 {i + 1} 筆：缺少必要欄位 - {string.Join(", ", fieldErrors)}");
            }
        }

        return (allErrors.Count == 0, allErrors);
    }

    #endregion
}
