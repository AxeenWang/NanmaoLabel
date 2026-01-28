using System.Text;
using System.Windows;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.ViewModels;
using NanmaoLabelPOC.Views;
using QuestPDF.Infrastructure;

namespace NanmaoLabelPOC;

/// <summary>
/// 應用程式入口點
/// [ref: raw_spec 13.5, 8.2]
///
/// 初始化順序：
/// 1. 設定 QuestPDF License
/// 2. 設定 ExcelDataReader 編碼
/// 3. 建立服務實例 (Poor Man's DI)
/// 4. 建立 MainViewModel
/// 5. 建立 MainWindow 並注入 ViewModel
/// 6. 顯示視窗 (Loaded 事件會自動觸發資料載入)
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

        // 建立服務實例 (Poor Man's DI) [ref: raw_spec 7.2]
        IDataStore dataStore = new DataStore();
        IBarcodeGenerator barcodeGenerator = new BarcodeGenerator();
        ILabelRenderer labelRenderer = new LabelRenderer();
        IPdfExporter pdfExporter = new PdfExporter(labelRenderer, barcodeGenerator);
        IExcelImporter excelImporter = new ExcelImporter();

        // 建立 MainViewModel [ref: raw_spec 8.2]
        var mainViewModel = new MainViewModel(
            dataStore,
            labelRenderer,
            pdfExporter,
            barcodeGenerator,
            excelImporter);

        // 建立並顯示 MainWindow [ref: raw_spec 8.3]
        var mainWindow = new MainWindow();
        mainWindow.SetViewModel(mainViewModel);
        mainWindow.Show();
    }
}
