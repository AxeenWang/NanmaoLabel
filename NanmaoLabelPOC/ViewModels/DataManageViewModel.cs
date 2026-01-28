using System.Collections.ObjectModel;
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
/// - 資料儲存 [ref: raw_spec F-13]
/// - IsDirty 追蹤 [ref: raw_spec 8.10]
/// </summary>
public partial class DataManageViewModel : ObservableObject
{
    private readonly IDataStore _dataStore;
    private readonly IExcelImporter _excelImporter;

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
    /// </summary>
    public string StatusMessage
    {
        get
        {
            if (IsDirty)
            {
                return $"已修改（未儲存）　共 {Records.Count} 筆資料";
            }
            return $"共 {Records.Count} 筆資料";
        }
    }

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
    /// 儲存命令
    /// [ref: raw_spec F-13]
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            _dataStore.Save(Records);
            IsDirty = false;
            OnPropertyChanged(nameof(StatusMessage));
        }
        catch (Exception)
        {
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

    #endregion
}
