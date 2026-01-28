using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.ViewModels;

namespace NanmaoLabelPOC.Views;

/// <summary>
/// 資料管理分頁
/// [ref: raw_spec 8.5]
/// </summary>
public partial class DataManageView : UserControl
{
    public DataManageView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 匯入按鈕點擊
    /// [ref: raw_spec F-01, 8.10, TC-13]
    /// </summary>
    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataManageViewModel viewModel)
            return;

        // T046: OpenFileDialog [ref: raw_spec 8.10]
        var openFileDialog = new OpenFileDialog
        {
            Title = "選擇 Excel 檔案",
            Filter = "Excel 檔案 (*.xlsx)|*.xlsx",
            FilterIndex = 1,
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        var filePath = openFileDialog.FileName;

        // T047: 覆蓋確認對話框 [ref: raw_spec 8.9]
        if (viewModel.Records.Count > 0)
        {
            var confirmResult = MessageBox.Show(
                "匯入將覆蓋現有資料，是否繼續？",
                "確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;
        }

        // 執行匯入
        var result = viewModel.ImportExcel(filePath);

        if (result == null)
            return;

        if (result.Success)
        {
            // T048: 匯入成功訊息 [ref: raw_spec 8.10]
            var successMessage = $"匯入成功，共 {result.RecordCount} 筆資料";

            // T050: 分號警告 [ref: raw_spec 3.3, 13.4]
            // T051: 千分位警告 [ref: raw_spec 13.14]
            if (result.Warnings.Count > 0)
            {
                var warningText = BuildWarningText(result.Warnings);
                MessageBox.Show(
                    $"{successMessage}\n\n{warningText}",
                    "匯入成功（含警告）",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    successMessage,
                    "匯入成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        else
        {
            // T049: 錯誤處理 [ref: raw_spec 8.9]
            MessageBox.Show(
                result.ErrorMessage ?? "匯入失敗",
                "錯誤",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 建立警告訊息文字
    /// </summary>
    private static string BuildWarningText(List<string> warnings)
    {
        if (warnings.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("警告：");

        // 顯示前 5 條警告
        var displayCount = Math.Min(warnings.Count, 5);
        for (int i = 0; i < displayCount; i++)
        {
            sb.AppendLine($"  • {warnings[i]}");
        }

        if (warnings.Count > 5)
        {
            sb.AppendLine($"  ... 及另外 {warnings.Count - 5} 條警告");
        }

        return sb.ToString();
    }
}
