using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NanmaoLabelPOC.ViewModels;

namespace NanmaoLabelPOC.Views;

/// <summary>
/// 標籤列印分頁
/// [ref: raw_spec 8.4]
/// </summary>
public partial class LabelPrintView : UserControl
{
    /// <summary>
    /// ListView 項目高度（含間距）
    /// [ref: raw_spec 8.4, 8.6] 項目高度 50px
    /// T070: ListView 分頁
    /// </summary>
    private const double ItemHeight = 54;

    public LabelPrintView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 視窗載入時計算每頁筆數
    /// [ref: raw_spec 8.4]
    /// T070: 自動計算每頁筆數
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CalculatePageSize();
    }

    /// <summary>
    /// 視窗大小變化時重新計算每頁筆數
    /// [ref: raw_spec 8.4]
    /// T070: 依視窗高度自動計算
    /// </summary>
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CalculatePageSize();
    }

    /// <summary>
    /// 計算每頁可顯示的筆數
    /// [ref: raw_spec 8.4]
    /// T070: 項目高度 50px，自動計算每頁筆數
    /// </summary>
    private void CalculatePageSize()
    {
        if (DataContext is not LabelPrintViewModel viewModel)
            return;

        // 取得 ListView 可用高度
        var listViewHeight = RecordListView.ActualHeight;
        if (listViewHeight <= 0)
        {
            // 估算可用高度（控件尚未完成佈局）
            listViewHeight = ActualHeight - 200;
        }

        // 計算每頁可顯示筆數（至少 1 筆）
        var pageSize = Math.Max(1, (int)Math.Floor(listViewHeight / ItemHeight));
        viewModel.PageSize = pageSize;
    }

    /// <summary>
    /// DataContext 變更時設定回調函數
    /// </summary>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LabelPrintViewModel viewModel)
        {
            // T066: 註冊檔案覆蓋確認對話框回調
            viewModel.ConfirmOverwriteCallback = ShowOverwriteConfirmation;
            // T069: 註冊必要欄位缺失警告對話框回調
            viewModel.ShowWarningCallback = ShowWarningDialog;
        }
    }

    /// <summary>
    /// 顯示檔案覆蓋確認對話框
    /// [ref: raw_spec 8.9]
    /// T066: 「檔案已存在，是否覆蓋？」
    /// </summary>
    private static bool ShowOverwriteConfirmation(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var result = MessageBox.Show(
            $"檔案已存在，是否覆蓋？\n\n{fileName}",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// 顯示警告對話框
    /// [ref: raw_spec 8.9, 13.21]
    /// T069: 「資料缺失：{欄位名稱}，無法產生標籤」
    /// </summary>
    private static void ShowWarningDialog(string message)
    {
        MessageBox.Show(
            message,
            "警告",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    /// <summary>
    /// 處理 ListView 雙擊事件
    /// [ref: raw_spec 8.8, 13.6]
    ///
    /// 雙擊觸發 PDF 輸出（含 500ms 防抖）
    /// </summary>
    private void RecordListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // 標記事件已處理，避免雙擊地獄 [ref: raw_spec 8.8]
        e.Handled = true;

        if (DataContext is LabelPrintViewModel viewModel)
        {
            viewModel.DoubleClickExportCommand.Execute(null);
        }
    }

    /// <summary>
    /// 批次輸出按鈕點擊事件
    /// [ref: raw_spec 3.3, 8.8]
    ///
    /// 輸出完成後顯示對話框，可選擇「開啟資料夾」或「確定」
    /// </summary>
    private void BatchExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LabelPrintViewModel viewModel)
            return;

        // 等待 Command 執行完畢（Command 已在 XAML 綁定並執行）
        // 檢查是否有成功輸出的路徑
        if (!string.IsNullOrEmpty(viewModel.LastBatchExportPath))
        {
            ShowBatchExportCompletionDialog(viewModel.LastBatchExportPath);
        }
    }

    /// <summary>
    /// 顯示批次輸出完成對話框
    /// [ref: raw_spec 3.3, 8.8]
    /// </summary>
    private static void ShowBatchExportCompletionDialog(string outputPath)
    {
        // 取得資料夾路徑
        var folderPath = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(folderPath))
            return;

        // 顯示自訂對話框 [ref: raw_spec 3.3]
        // T074: 對話框標題使用「提示」[ref: raw_spec 8.10]
        var result = MessageBox.Show(
            $"批次輸出完成！\n\n輸出檔案：{outputPath}\n\n是否開啟資料夾？",
            "提示",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            // 開啟資料夾並選取檔案
            try
            {
                Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
            }
            catch
            {
                // 若開啟失敗，嘗試只開啟資料夾
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch
                {
                    // 忽略錯誤
                }
            }
        }
    }
}
