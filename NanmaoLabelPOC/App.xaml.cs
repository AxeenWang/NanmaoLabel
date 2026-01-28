using System.Text;
using System.Windows;
using QuestPDF.Infrastructure;

namespace NanmaoLabelPOC;

/// <summary>
/// 應用程式入口點
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 設定 QuestPDF Community License [ref: raw_spec 7.1]
        QuestPDF.Settings.License = LicenseType.Community;

        // 設定 ExcelDataReader 編碼支援
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
