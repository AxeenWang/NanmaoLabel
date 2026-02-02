using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NanmaoLabelPOC.Models;
using NanmaoLabelPOC.Services;
using NanmaoLabelPOC.ViewModels;

// 避免 TextAlignment 命名衝突
using WpfTextAlignment = System.Windows.TextAlignment;

namespace NanmaoLabelPOC.Views;

/// <summary>
/// 標籤列印分頁
/// [ref: raw_spec 8.4]
/// </summary>
public partial class LabelPrintView : UserControl
{
    #region Constants (T003)

    /// <summary>
    /// ListView 項目高度（含間距）
    /// [ref: raw_spec 8.4, 8.6] 項目高度 50px
    /// T070: ListView 分頁
    /// </summary>
    private const double ItemHeight = 54;

    /// <summary>
    /// 座標轉換縮放比（4:1 縮放 = 400px / 100mm）
    /// [ref: raw_delta_label_display §3.3, spec FR-007] T003
    /// </summary>
    private const double ScaleFactor = 4.0;

    /// <summary>
    /// 字型大小轉換係數（pt 轉 px，含縮放比）
    /// [ref: raw_delta_label_display §3.3] T003
    /// 計算：4.0 / 2.83465 ≈ 1.411
    /// </summary>
    private const double PtToPxFactor = 4.0 / 2.83465;

    /// <summary>
    /// 預覽字型（微軟正黑體）
    /// [ref: raw_spec 13.1] T003
    /// </summary>
    private const string PreviewFontFamily = "Microsoft JhengHei";

    #endregion

    #region Fields (T004)

    /// <summary>
    /// 條碼生成服務（於 OnDataContextChanged 中初始化）
    /// [ref: raw_delta_label_display §1.3.3] T004
    /// </summary>
    private IBarcodeGenerator? _barcodeGenerator;

    #endregion

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
    /// DataContext 變更時設定回調函數與訂閱 ViewModel
    /// [ref: raw_delta_label_display §7.2] T004, T005
    /// </summary>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // T004: 取得 IBarcodeGenerator 實例
        _barcodeGenerator ??= App.BarcodeGenerator;

        if (e.NewValue is LabelPrintViewModel viewModel)
        {
            // T066: 註冊檔案覆蓋確認對話框回調
            viewModel.ConfirmOverwriteCallback = ShowOverwriteConfirmation;
            // T069: 註冊必要欄位缺失警告對話框回調
            viewModel.ShowWarningCallback = ShowWarningDialog;

            // T005: 訂閱 ViewModel 的 PropertyChanged 事件
            SubscribeToViewModel(viewModel);
        }
    }

    /// <summary>
    /// 訂閱 ViewModel 的 PropertyChanged 事件
    /// [ref: raw_delta_label_display §7.2, spec FR-002] T005
    ///
    /// 當 PreviewCommands 變更時觸發 Canvas 渲染
    /// </summary>
    private void SubscribeToViewModel(LabelPrintViewModel viewModel)
    {
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// 處理 ViewModel 的 PropertyChanged 事件
    /// [ref: raw_delta_label_display §2.2] T005
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LabelPrintViewModel.PreviewCommands))
        {
            RenderPreview();
        }
    }

    #region Preview Rendering (T006-T011)

    /// <summary>
    /// 渲染標籤預覽主方法
    /// [ref: spec FR-001, raw_delta_label_display §7.2] T006
    /// </summary>
    private void RenderPreview()
    {
        try
        {
            // 清空 Canvas
            PreviewCanvas.Children.Clear();

            // 取得 ViewModel.PreviewCommands
            if (DataContext is not LabelPrintViewModel viewModel)
                return;

            var commands = viewModel.PreviewCommands;

            // 若為 null 或空，直接返回（空白狀態由 XAML Visibility 綁定處理）
            if (commands == null || commands.Count == 0)
                return;

            // 遍歷渲染
            RenderCommands(commands);
        }
        catch (Exception ex)
        {
            // [ref: spec FR-006] T015: 錯誤處理
            Debug.WriteLine($"[RenderPreview] 標籤渲染失敗: {ex}");

            // 顯示錯誤提示 TextBlock
            PreviewCanvas.Children.Clear();
            var errorText = new TextBlock
            {
                Text = "⚠️ 標籤渲染失敗",
                FontFamily = new FontFamily(PreviewFontFamily),
                FontSize = 14,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(errorText, 120);
            Canvas.SetTop(errorText, 100);
            PreviewCanvas.Children.Add(errorText);
        }
    }

    /// <summary>
    /// 遍歷渲染指令集合
    /// [ref: spec FR-003] T007
    /// </summary>
    private void RenderCommands(IReadOnlyList<RenderCommand> commands)
    {
        foreach (var command in commands)
        {
            // 依 command.Skip 判斷是否略過 [ref: raw_spec 13.15]
            if (command.Skip)
                continue;

            // 依 command.CommandType 分派至對應 Render 方法
            switch (command.CommandType)
            {
                case RenderCommandType.Text:
                    RenderTextCommand(command);
                    break;
                case RenderCommandType.Barcode:
                    RenderBarcodeCommand(command);
                    break;
                case RenderCommandType.QRCode:
                    RenderQRCodeCommand(command);
                    break;
            }
        }
    }

    /// <summary>
    /// 渲染文字指令
    /// [ref: spec FR-003, FR-007] T008
    /// </summary>
    private void RenderTextCommand(RenderCommand command)
    {
        // 計算字體大小
        var fontSize = (command.FontSize ?? 10) * PtToPxFactor;

        // 判斷是否為多行文字
        var isMultiLine = command.Content?.Contains('\n') ?? false;

        // 建立 TextBlock 元素
        var textBlock = new TextBlock
        {
            // 處理換行符：將 \n 轉換為 Environment.NewLine 讓 WPF 正確顯示多行
            Text = command.Content?.Replace("\n", Environment.NewLine) ?? string.Empty,
            FontFamily = new FontFamily(PreviewFontFamily),
            FontSize = fontSize,
            FontWeight = command.IsBold ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = ConvertAlignment(command.Alignment),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.Wrap,  // 啟用換行
            Width = command.Width * ScaleFactor
        };

        // 多行文字設定較緊密的行高，避免超出邊界
        if (isMultiLine)
        {
            textBlock.LineHeight = fontSize * 1.15;  // 行高為字體大小的 1.15 倍
            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        }
        else
        {
            // 單行文字設定固定高度
            textBlock.Height = command.Height * ScaleFactor;
        }

        // 設定座標 [ref: spec FR-007] px = mm × 4
        Canvas.SetLeft(textBlock, command.X * ScaleFactor);
        Canvas.SetTop(textBlock, command.Y * ScaleFactor);

        // 加入 Canvas
        PreviewCanvas.Children.Add(textBlock);
    }

    /// <summary>
    /// 轉換對齊方式（Models.TextAlignment → System.Windows.TextAlignment）
    /// </summary>
    private static WpfTextAlignment ConvertAlignment(Models.TextAlignment alignment)
    {
        return alignment switch
        {
            Models.TextAlignment.Left => WpfTextAlignment.Left,
            Models.TextAlignment.Center => WpfTextAlignment.Center,
            Models.TextAlignment.Right => WpfTextAlignment.Right,
            _ => WpfTextAlignment.Left
        };
    }

    /// <summary>
    /// 渲染條碼指令
    /// [ref: spec FR-003, raw_spec 6.1] T009
    /// </summary>
    private void RenderBarcodeCommand(RenderCommand command)
    {
        if (_barcodeGenerator == null || string.IsNullOrEmpty(command.Content))
            return;

        // 呼叫 IBarcodeGenerator 產生條碼圖片
        // 條碼高度需預留空間給下方文字（約 3mm）
        var barcodeHeightMm = command.Height - 3;
        if (barcodeHeightMm <= 0)
            barcodeHeightMm = command.Height;

        var barcodeBytes = _barcodeGenerator.GenerateCode128(command.Content, barcodeHeightMm);

        // 將 byte[] 轉為 BitmapImage
        var bitmapImage = ByteArrayToBitmapImage(barcodeBytes);
        if (bitmapImage == null)
            return;

        // 建立 Image 元素
        var image = new Image
        {
            Source = bitmapImage,
            Width = command.Width * ScaleFactor,
            Height = barcodeHeightMm * ScaleFactor,
            Stretch = Stretch.Fill
        };

        // 設定座標
        Canvas.SetLeft(image, command.X * ScaleFactor);
        Canvas.SetTop(image, command.Y * ScaleFactor);

        // 加入 Canvas
        PreviewCanvas.Children.Add(image);

        // 條碼下方顯示文字 [ref: raw_spec 6.1]
        var textBlock = new TextBlock
        {
            Text = command.Content,
            FontFamily = new FontFamily(PreviewFontFamily),
            FontSize = 8 * PtToPxFactor,
            TextAlignment = WpfTextAlignment.Center,
            Width = command.Width * ScaleFactor
        };
        Canvas.SetLeft(textBlock, command.X * ScaleFactor);
        Canvas.SetTop(textBlock, (command.Y + barcodeHeightMm) * ScaleFactor);
        PreviewCanvas.Children.Add(textBlock);
    }

    /// <summary>
    /// 渲染 QR Code 指令
    /// [ref: spec FR-003, raw_spec 6.2] T010
    /// </summary>
    private void RenderQRCodeCommand(RenderCommand command)
    {
        if (_barcodeGenerator == null || string.IsNullOrEmpty(command.Content))
            return;

        // QR Code 為正方形，取寬高較小值
        var sizeMm = Math.Min(command.Width, command.Height);

        // 呼叫 IBarcodeGenerator 產生 QR Code 圖片
        var qrBytes = _barcodeGenerator.GenerateQRCode(command.Content, sizeMm);

        // 將 byte[] 轉為 BitmapImage
        var bitmapImage = ByteArrayToBitmapImage(qrBytes);
        if (bitmapImage == null)
            return;

        // 建立 Image 元素
        var image = new Image
        {
            Source = bitmapImage,
            Width = sizeMm * ScaleFactor,
            Height = sizeMm * ScaleFactor,
            Stretch = Stretch.Uniform
        };

        // 設定座標
        Canvas.SetLeft(image, command.X * ScaleFactor);
        Canvas.SetTop(image, command.Y * ScaleFactor);

        // 加入 Canvas
        PreviewCanvas.Children.Add(image);
    }

    /// <summary>
    /// 將 byte[] 轉為 BitmapImage
    /// [ref: raw_delta_label_display §7.2] T011
    /// </summary>
    private static BitmapImage? ByteArrayToBitmapImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        var bitmapImage = new BitmapImage();
        using (var stream = new MemoryStream(bytes))
        {
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
        }
        bitmapImage.Freeze(); // 讓圖片可跨執行緒使用
        return bitmapImage;
    }

    #endregion

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
            catch (Exception ex)
            {
                // 若開啟失敗，嘗試只開啟資料夾
                Debug.WriteLine($"[ShowBatchExportCompletionDialog] 無法選取檔案: {ex.Message}");
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch (Exception innerEx)
                {
                    // 記錄錯誤但不阻擋主流程（符合憲章：空 catch 必須記錄）
                    Debug.WriteLine($"[ShowBatchExportCompletionDialog] 無法開啟資料夾: {innerEx.Message}");
                }
            }
        }
    }
}
