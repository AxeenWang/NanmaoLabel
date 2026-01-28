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
    public LabelPrintView()
    {
        InitializeComponent();
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
        var result = MessageBox.Show(
            $"批次輸出完成！\n\n輸出檔案：{outputPath}\n\n是否開啟資料夾？",
            "批次輸出完成",
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
