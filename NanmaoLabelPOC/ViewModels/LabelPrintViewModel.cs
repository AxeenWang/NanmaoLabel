using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.Templates;

namespace NanmaoLabelPOC.ViewModels;

/// <summary>
/// 標籤列印分頁 ViewModel
/// [ref: raw_spec 8.4, 7.1]
///
/// 功能：
/// - 顯示資料清單 [ref: raw_spec 8.4]
/// - 單擊更新預覽 [ref: raw_spec 13.6]
/// - 雙擊輸出 PDF（500ms 防抖）[ref: raw_spec 8.8, 13.6]
/// - 標籤格式切換 [ref: raw_spec F-05]
/// </summary>
public partial class LabelPrintViewModel : ObservableObject
{
    private readonly IDataStore _dataStore;
    private readonly ILabelRenderer _labelRenderer;
    private readonly IPdfExporter _pdfExporter;

    /// <summary>
    /// 雙擊防抖計時器
    /// [ref: raw_spec 8.8, 13.6]
    /// </summary>
    private readonly DispatcherTimer _debounceTimer;

    /// <summary>
    /// 防抖時間 500ms [ref: raw_spec 8.8]
    /// </summary>
    private const int DebounceMilliseconds = 500;

    /// <summary>
    /// 是否正在防抖期間
    /// </summary>
    private bool _isDebouncing;

    public LabelPrintViewModel(
        IDataStore dataStore,
        ILabelRenderer labelRenderer,
        IPdfExporter pdfExporter)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        _labelRenderer = labelRenderer ?? throw new ArgumentNullException(nameof(labelRenderer));
        _pdfExporter = pdfExporter ?? throw new ArgumentNullException(nameof(pdfExporter));

        // 初始化內建標籤格式 [ref: raw_spec 5.1, 5.2]
        AvailableTemplates = new ObservableCollection<LabelTemplate>(BuiltInTemplates.GetAll());
        _selectedTemplate = AvailableTemplates.FirstOrDefault();

        // 初始化防抖計時器
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceMilliseconds)
        };
        _debounceTimer.Tick += OnDebounceTimerTick;

        // 初始化資料
        Records = new ObservableCollection<DataRecord>();
    }

    #region Properties

    /// <summary>
    /// 資料清單
    /// </summary>
    public ObservableCollection<DataRecord> Records { get; }

    /// <summary>
    /// 可用的標籤格式清單
    /// [ref: raw_spec 5.1, 5.2]
    /// </summary>
    public ObservableCollection<LabelTemplate> AvailableTemplates { get; }

    /// <summary>
    /// 選取的資料紀錄
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedRecord))]
    [NotifyPropertyChangedFor(nameof(PreviewCommands))]
    [NotifyCanExecuteChangedFor(nameof(ExportPdfCommand))]
    private DataRecord? _selectedRecord;

    /// <summary>
    /// 選取的標籤格式
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewCommands))]
    private LabelTemplate? _selectedTemplate;

    /// <summary>
    /// 是否有選取的資料紀錄
    /// </summary>
    public bool HasSelectedRecord => SelectedRecord != null;

    /// <summary>
    /// 是否有資料
    /// </summary>
    public bool HasRecords => Records.Count > 0;

    /// <summary>
    /// 最後批次輸出的檔案路徑（供對話框使用）
    /// </summary>
    [ObservableProperty]
    private string? _lastBatchExportPath;

    /// <summary>
    /// 預覽渲染指令
    /// [ref: raw_spec 2.5]
    /// </summary>
    public IReadOnlyList<RenderCommand>? PreviewCommands
    {
        get
        {
            if (SelectedRecord == null || SelectedTemplate == null)
                return null;

            return _labelRenderer.Render(SelectedTemplate, SelectedRecord);
        }
    }

    /// <summary>
    /// 狀態列訊息
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "就緒";

    /// <summary>
    /// 資料筆數顯示
    /// </summary>
    public string RecordCountText => $"共 {Records.Count} 筆資料";

    /// <summary>
    /// 最後一次驗證發現的缺失欄位
    /// [ref: raw_spec 3.3, 8.9]
    /// T069: 必要欄位缺失警告
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<string>? _missingRequiredFields;

    /// <summary>
    /// 是否有缺失的必要欄位
    /// </summary>
    public bool HasMissingFields => MissingRequiredFields?.Count > 0;

    /// <summary>
    /// 缺失欄位警告訊息
    /// [ref: raw_spec 8.9] 「資料缺失：{欄位名稱}，無法產生標籤」
    /// </summary>
    public string? MissingFieldsWarning => HasMissingFields
        ? $"資料缺失：{string.Join("、", MissingRequiredFields!)}，無法產生標籤"
        : null;

    /// <summary>
    /// 檔案覆蓋確認回調
    /// [ref: raw_spec 8.9]
    /// T066: 檔案覆蓋確認對話框
    /// </summary>
    public Func<string, bool>? ConfirmOverwriteCallback { get; set; }

    /// <summary>
    /// 必要欄位缺失警告回調
    /// [ref: raw_spec 8.9]
    /// T069: 必要欄位缺失警告
    /// </summary>
    public Action<string>? ShowWarningCallback { get; set; }

    #endregion

    #region Commands

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

            OnPropertyChanged(nameof(HasRecords));
            OnPropertyChanged(nameof(RecordCountText));
            BatchExportCommand.NotifyCanExecuteChanged();
            StatusMessage = $"已載入 {Records.Count} 筆資料";
        }
        catch (Exception ex)
        {
            StatusMessage = $"載入失敗：{ex.Message}";
        }
    }

    /// <summary>
    /// 輸出 PDF 命令
    /// [ref: raw_spec 3.1 F-08]
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportPdf))]
    private void ExportPdf()
    {
        if (SelectedRecord == null || SelectedTemplate == null)
            return;

        // T069: 必要欄位缺失檢查 [ref: raw_spec 3.3, 8.9]
        var missingFields = _labelRenderer.ValidateRequiredFields(SelectedTemplate, SelectedRecord);
        if (missingFields.Count > 0)
        {
            var warningMessage = $"資料缺失：{string.Join("、", missingFields)}，無法產生標籤";
            ShowWarningCallback?.Invoke(warningMessage);
            StatusMessage = $"⚠️ {warningMessage}";
            return;
        }

        try
        {
            // 產生預設輸出路徑
            var fileName = _pdfExporter.GenerateDefaultFileName(SelectedRecord);
            var outputPath = Path.Combine(_pdfExporter.OutputDirectory, fileName);

            // T066: 檔案覆蓋確認 [ref: raw_spec 8.9]
            // T065: 確保輸出目錄存在 [ref: raw_spec 2.3]
            if (_pdfExporter.FileExists(outputPath))
            {
                var confirmOverwrite = ConfirmOverwriteCallback?.Invoke(outputPath) ?? true;
                if (!confirmOverwrite)
                {
                    StatusMessage = "已取消輸出";
                    return;
                }
            }

            var actualPath = _pdfExporter.ExportSingle(SelectedTemplate, SelectedRecord, outputPath);
            // 狀態列訊息 [ref: raw_spec 8.8, 13.6]
            StatusMessage = $"✅ PDF 已輸出：{actualPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 輸出失敗：{ex.Message}";
        }
    }

    private bool CanExportPdf() => SelectedRecord != null && SelectedTemplate != null;

    /// <summary>
    /// 批次輸出全部命令
    /// [ref: raw_spec 3.3, TC-12]
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanBatchExport))]
    private void BatchExport()
    {
        if (!HasRecords || SelectedTemplate == null)
            return;

        try
        {
            // 產生預設輸出路徑
            var fileName = _pdfExporter.GenerateBatchFileName();
            var outputPath = Path.Combine(_pdfExporter.OutputDirectory, fileName);

            // T066: 檔案覆蓋確認 [ref: raw_spec 8.9]
            // T065: 確保輸出目錄存在 [ref: raw_spec 2.3]
            if (_pdfExporter.FileExists(outputPath))
            {
                var confirmOverwrite = ConfirmOverwriteCallback?.Invoke(outputPath) ?? true;
                if (!confirmOverwrite)
                {
                    StatusMessage = "已取消批次輸出";
                    LastBatchExportPath = null;
                    return;
                }
            }

            var actualPath = _pdfExporter.ExportBatch(SelectedTemplate, Records, outputPath);
            LastBatchExportPath = actualPath;
            // 狀態列訊息
            StatusMessage = $"✅ 批次輸出完成：{actualPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 批次輸出失敗：{ex.Message}";
            LastBatchExportPath = null;
        }
    }

    private bool CanBatchExport() => HasRecords && SelectedTemplate != null;

    /// <summary>
    /// 雙擊輸出 PDF（含防抖）
    /// [ref: raw_spec 8.8, 13.6]
    /// </summary>
    [RelayCommand]
    private void DoubleClickExport()
    {
        if (_isDebouncing)
            return;

        // 啟動防抖
        _isDebouncing = true;
        _debounceTimer.Start();

        // 執行輸出
        if (CanExportPdf())
        {
            ExportPdf();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// 選取資料項目（單擊）
    /// [ref: raw_spec 13.6]
    /// </summary>
    public void SelectRecord(DataRecord? record)
    {
        SelectedRecord = record;
    }

    /// <summary>
    /// 刷新預覽
    /// </summary>
    public void RefreshPreview()
    {
        OnPropertyChanged(nameof(PreviewCommands));
    }

    /// <summary>
    /// 重新載入資料（供外部呼叫，如分頁切換時）
    /// </summary>
    public void Refresh()
    {
        LoadData();
    }

    /// <summary>
    /// 防抖計時器結束
    /// </summary>
    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
        _isDebouncing = false;
    }

    #endregion
}
