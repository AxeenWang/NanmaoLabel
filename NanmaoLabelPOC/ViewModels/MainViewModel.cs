using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NanmaoLabelPOC.Services;

namespace NanmaoLabelPOC.ViewModels;

/// <summary>
/// 主視窗 ViewModel
/// [ref: raw_spec 8.2, 8.3]
///
/// 功能：
/// - 程式啟動自動載入 data.json [ref: raw_spec 13.5]
/// - 分頁導航：標籤列印、資料管理 [ref: raw_spec 8.2]
/// - 優雅處理載入失敗 [ref: raw_spec 8.9]
/// - 全螢幕切換 [ref: raw_spec 8.3]
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IDataStore _dataStore;
    private readonly ILabelRenderer _labelRenderer;
    private readonly IPdfExporter _pdfExporter;
    private readonly IBarcodeGenerator _barcodeGenerator;

    /// <summary>
    /// 標籤列印分頁 ViewModel
    /// </summary>
    public LabelPrintViewModel LabelPrintViewModel { get; }

    public MainViewModel(
        IDataStore dataStore,
        ILabelRenderer labelRenderer,
        IPdfExporter pdfExporter,
        IBarcodeGenerator barcodeGenerator)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        _labelRenderer = labelRenderer ?? throw new ArgumentNullException(nameof(labelRenderer));
        _pdfExporter = pdfExporter ?? throw new ArgumentNullException(nameof(pdfExporter));
        _barcodeGenerator = barcodeGenerator ?? throw new ArgumentNullException(nameof(barcodeGenerator));

        // 初始化子 ViewModel
        LabelPrintViewModel = new LabelPrintViewModel(dataStore, labelRenderer, pdfExporter);
    }

    #region Properties

    /// <summary>
    /// 當前選取的分頁索引
    /// [ref: raw_spec 8.2]
    /// </summary>
    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// 是否為全螢幕模式
    /// [ref: raw_spec 8.3]
    /// </summary>
    [ObservableProperty]
    private bool _isFullScreen;

    /// <summary>
    /// 狀態列訊息
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "就緒";

    /// <summary>
    /// 資料筆數顯示
    /// </summary>
    public string RecordCountText => LabelPrintViewModel.RecordCountText;

    #endregion

    #region Commands

    /// <summary>
    /// 切換全螢幕命令
    /// [ref: raw_spec 8.3]
    /// </summary>
    [RelayCommand]
    private void ToggleFullScreen()
    {
        IsFullScreen = !IsFullScreen;
    }

    /// <summary>
    /// 離開全螢幕命令
    /// [ref: raw_spec 8.3]
    /// </summary>
    [RelayCommand]
    private void ExitFullScreen()
    {
        IsFullScreen = false;
    }

    #endregion

    #region Methods

    /// <summary>
    /// 初始化並載入資料
    /// 程式啟動時呼叫 [ref: raw_spec 13.5]
    /// </summary>
    public void Initialize()
    {
        LoadDataOnStartup();
    }

    /// <summary>
    /// 程式啟動自動載入資料
    /// [ref: raw_spec 13.5, TC-01]
    /// </summary>
    private void LoadDataOnStartup()
    {
        try
        {
            LabelPrintViewModel.LoadDataCommand.Execute(null);

            if (LabelPrintViewModel.HasRecords)
            {
                StatusMessage = $"已載入 {LabelPrintViewModel.Records.Count} 筆資料";
            }
            else
            {
                // 無資料狀態 [ref: raw_spec 8.4 空白狀態]
                StatusMessage = "尚無資料，請至「資料管理」分頁匯入 Excel";
            }

            OnPropertyChanged(nameof(RecordCountText));
        }
        catch (Exception ex)
        {
            // 優雅處理載入失敗 [ref: raw_spec 8.9]
            StatusMessage = $"載入失敗：{ex.Message}";
        }
    }

    /// <summary>
    /// 刷新資料（供分頁切換時呼叫）
    /// </summary>
    public void RefreshData()
    {
        LabelPrintViewModel.Refresh();
        OnPropertyChanged(nameof(RecordCountText));
    }

    /// <summary>
    /// 更新狀態列訊息
    /// </summary>
    public void UpdateStatus(string message)
    {
        StatusMessage = message;
    }

    #endregion
}
