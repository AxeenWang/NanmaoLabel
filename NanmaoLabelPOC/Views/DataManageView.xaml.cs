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
    /// <summary>
    /// 是否正在更新欄位（防止迴圈觸發）
    /// </summary>
    private bool _isUpdatingField = false;

    public DataManageView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 刪除按鈕點擊
    /// T053: 顯示確認對話框後刪除
    /// [ref: raw_spec 8.9]
    /// </summary>
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataManageViewModel viewModel)
            return;

        if (viewModel.SelectedRecord == null)
            return;

        // T053: 確認對話框 [ref: raw_spec 8.9]
        var result = MessageBox.Show(
            "確定要刪除選取的資料嗎？此操作無法復原。",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        viewModel.DeleteRecordCommand.Execute(null);
    }

    /// <summary>
    /// 儲存按鈕點擊
    /// T058: 儲存成功後顯示訊息
    /// T059/T060: 儲存前驗證
    /// [ref: raw_spec 8.9, 8.10, 13.14]
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataManageViewModel viewModel)
            return;

        // T059/T060: 儲存前驗證
        var (isValid, errors) = viewModel.ValidateAllRecords();

        if (!isValid)
        {
            // T060: 必要欄位驗證錯誤對話框 [ref: raw_spec 8.9]
            var errorMessage = "資料驗證失敗：\n\n" + string.Join("\n", errors);
            MessageBox.Show(
                errorMessage,
                "錯誤",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        try
        {
            viewModel.SaveCommand.Execute(null);

            // T058: 儲存成功訊息 [ref: raw_spec 8.10]
            if (!string.IsNullOrEmpty(viewModel.LastSaveMessage))
            {
                MessageBox.Show(
                    viewModel.LastSaveMessage,
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"儲存失敗：{ex.Message}",
                "錯誤",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 欄位編輯變更事件
    /// T054/T055: 編輯時標記 IsDirty
    /// </summary>
    private void EditField_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingField)
            return;

        if (DataContext is DataManageViewModel viewModel && viewModel.HasSelectedRecord)
        {
            viewModel.MarkDirty();
            // 刷新 DataGrid 顯示
            RecordDataGrid.Items.Refresh();
        }
    }

    /// <summary>
    /// 數量欄位變更事件
    /// T059: 數量欄位驗證（僅允許數字）
    /// [ref: raw_spec 13.14]
    /// </summary>
    private void QuantityField_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingField)
            return;

        if (sender is not TextBox textBox)
            return;

        var text = textBox.Text;

        // T059: 驗證數量欄位 [ref: raw_spec 13.14]
        if (!DataManageViewModel.ValidateQuantityField(text))
        {
            // 標記為紅色邊框
            textBox.BorderBrush = System.Windows.Media.Brushes.Red;
            textBox.ToolTip = "數量欄位僅允許輸入數字";
        }
        else
        {
            // 恢復正常邊框
            textBox.ClearValue(TextBox.BorderBrushProperty);
            textBox.ToolTip = null;
        }

        EditField_TextChanged(sender, e);
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
            // TODO: Phase 4 (T024) 將改用 ImportResultDialog
#pragma warning disable CS0618 // Obsolete - 向後相容，Phase 4 遷移
            if (result.Warnings.Count > 0)
            {
                var warningText = BuildWarningText(result.Warnings);
#pragma warning restore CS0618
                // T074: 對話框標題使用「警告」[ref: raw_spec 8.10]
                MessageBox.Show(
                    $"{successMessage}\n\n{warningText}",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                // T074: 對話框標題使用「提示」[ref: raw_spec 8.10]
                MessageBox.Show(
                    successMessage,
                    "提示",
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
