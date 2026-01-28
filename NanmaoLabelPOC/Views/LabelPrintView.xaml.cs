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
}
