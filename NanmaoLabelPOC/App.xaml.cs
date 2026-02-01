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
/// 1. 啟動效能監控 [ref: 憲章 IV] T075
/// 2. 設定 QuestPDF License
/// 3. 設定 ExcelDataReader 編碼
/// 4. 建立服務實例 (Poor Man's DI)
/// 5. 建立 MainViewModel
/// 6. 建立 MainWindow 並注入 ViewModel
/// 7. 顯示視窗 (Loaded 事件會自動觸發資料載入)
/// 8. 結束啟動計時並驗證 [ref: 憲章 IV] T075
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 條碼生成服務實例（供 View 層存取）
    /// [ref: raw_delta_label_display §7.2] T004
    ///
    /// 因使用 Poor Man's DI，View 層需透過此靜態屬性取得服務實例
    /// </summary>
    public static IBarcodeGenerator? BarcodeGenerator { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // T075: 啟動效能監控 [ref: 憲章 IV]
        PerformanceMonitor.StartApplication();

        base.OnStartup(e);

        // 設定 QuestPDF Community License [ref: raw_spec 7.1]
        QuestPDF.Settings.License = LicenseType.Community;

        // 設定 ExcelDataReader 編碼支援
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 建立服務實例 (Poor Man's DI) [ref: raw_spec 7.2]
        IDataStore dataStore = new DataStore();
        IBarcodeGenerator barcodeGenerator = new BarcodeGenerator();
        BarcodeGenerator = barcodeGenerator; // T004: 供 View 層存取
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

        // T075: 記錄啟動完成時間 [ref: 憲章 IV]
        // 注意：完整的啟動時間包括 MainWindow_Loaded 中的資料載入
        // 這裡先記錄視窗顯示的時間，實際驗證在 MainWindow_Loaded 完成後
        mainWindow.Loaded += (_, _) =>
        {
            PerformanceMonitor.EndStartup();
        };
    }
}
