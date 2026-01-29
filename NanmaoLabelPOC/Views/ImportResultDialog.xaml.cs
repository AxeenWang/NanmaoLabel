using System.Windows;
using NanmaoLabelPOC.Services;

namespace NanmaoLabelPOC.Views;

/// <summary>
/// 匯入結果對話框
/// [ref: spec.md FR-006, FR-007, FR-008]
///
/// T019/T020: 自訂對話框結構
/// T021: 匯入筆數、Error 區段、Warning 區段
/// T022: Info 區段使用 Expander 預設收合
/// T023: 大量訊息摘要
/// </summary>
public partial class ImportResultDialog : Window
{
    /// <summary>
    /// Info 訊息顯示上限 [ref: spec.md Edge Cases]
    /// </summary>
    private const int MaxInfoDisplay = 10;

    public ImportResultDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 使用 ImportResult 初始化對話框
    /// </summary>
    /// <param name="result">匯入結果</param>
    public void SetResult(ImportResult result)
    {
        // T021: 設定主訊息 [ref: spec.md FR-007]
        if (result.Success)
        {
            StatusIcon.Text = "✅";
            MainMessage.Text = $"匯入成功，共 {result.RecordCount} 筆資料";
            Title = "匯入成功";
        }
        else
        {
            StatusIcon.Text = "❌";
            MainMessage.Text = result.ErrorMessage ?? "匯入失敗";
            Title = "匯入失敗";
        }

        // T021: 設定訊息統計 [ref: spec.md FR-008]
        SetMessageStats(result);

        // T021: 設定 Error 區段 [ref: spec.md FR-004]
        SetErrorSection(result);

        // T021: 設定 Warning 區段 [ref: spec.md FR-005]
        SetWarningSection(result);

        // T022/T023: 設定 Info 區段 [ref: spec.md FR-006]
        SetInfoSection(result);
    }

    /// <summary>
    /// 設定訊息統計 [ref: spec.md FR-008]
    /// </summary>
    private void SetMessageStats(ImportResult result)
    {
        var hasStats = result.ErrorCount > 0 || result.WarningCount > 0 || result.InfoCount > 0;

        if (!hasStats)
        {
            MessageStats.Visibility = Visibility.Collapsed;
            return;
        }

        MessageStats.Visibility = Visibility.Visible;

        ErrorCountText.Text = result.ErrorCount > 0 ? $"錯誤: {result.ErrorCount}" : string.Empty;
        ErrorCountText.Visibility = result.ErrorCount > 0 ? Visibility.Visible : Visibility.Collapsed;

        WarningCountText.Text = result.WarningCount > 0 ? $"警告: {result.WarningCount}" : string.Empty;
        WarningCountText.Visibility = result.WarningCount > 0 ? Visibility.Visible : Visibility.Collapsed;

        InfoCountText.Text = result.InfoCount > 0 ? $"資訊: {result.InfoCount}" : string.Empty;
        InfoCountText.Visibility = result.InfoCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// 設定 Error 區段 [ref: spec.md FR-004]
    /// </summary>
    private void SetErrorSection(ImportResult result)
    {
        var errors = result.Messages
            .Where(m => m.Severity == MessageSeverity.Error)
            .Select(m => $"• {m.Message}")
            .ToList();

        if (errors.Count == 0)
        {
            ErrorSection.Visibility = Visibility.Collapsed;
            return;
        }

        ErrorSection.Visibility = Visibility.Visible;
        ErrorList.ItemsSource = errors;
    }

    /// <summary>
    /// 設定 Warning 區段 [ref: spec.md FR-005]
    /// </summary>
    private void SetWarningSection(ImportResult result)
    {
        var warnings = result.Messages
            .Where(m => m.Severity == MessageSeverity.Warning)
            .Select(m => $"• {m.Message}")
            .ToList();

        if (warnings.Count == 0)
        {
            WarningSection.Visibility = Visibility.Collapsed;
            return;
        }

        WarningSection.Visibility = Visibility.Visible;
        WarningList.ItemsSource = warnings;
    }

    /// <summary>
    /// 設定 Info 區段 [ref: spec.md FR-006]
    /// T022: 使用 Expander 預設收合
    /// T023: 大量訊息摘要
    /// </summary>
    private void SetInfoSection(ImportResult result)
    {
        var infos = result.Messages
            .Where(m => m.Severity == MessageSeverity.Info)
            .ToList();

        if (infos.Count == 0)
        {
            InfoExpander.Visibility = Visibility.Collapsed;
            return;
        }

        InfoExpander.Visibility = Visibility.Visible;

        // T023: 大量訊息處理 [ref: spec.md Edge Cases]
        if (infos.Count > MaxInfoDisplay)
        {
            // 只顯示前 10 條
            var displayInfos = infos
                .Take(MaxInfoDisplay)
                .Select(m => $"• {m.Message}")
                .ToList();

            InfoList.ItemsSource = displayInfos;

            // 顯示溢出提示
            InfoOverflowText.Text = $"... 及另外 {infos.Count - MaxInfoDisplay} 條資訊";
            InfoOverflowText.Visibility = Visibility.Visible;
        }
        else
        {
            InfoList.ItemsSource = infos.Select(m => $"• {m.Message}").ToList();
            InfoOverflowText.Visibility = Visibility.Collapsed;
        }

        // 更新 Expander Header 中的摘要
        UpdateInfoHeader(infos.Count);
    }

    /// <summary>
    /// 更新 Info Expander 標題中的摘要文字
    /// </summary>
    private void UpdateInfoHeader(int count)
    {
        // 由於 HeaderTemplate 的限制，我們直接修改 Header 屬性
        InfoExpander.Header = $"資訊（共 {count} 條，點擊展開）";
    }

    /// <summary>
    /// 確定按鈕點擊
    /// </summary>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
