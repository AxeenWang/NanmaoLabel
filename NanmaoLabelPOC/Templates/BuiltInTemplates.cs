using NanmaoLabelPOC.Models;

namespace NanmaoLabelPOC.Templates;

/// <summary>
/// 內建標籤格式定義
/// [ref: raw_spec 5.1, 5.2, 5.3]
///
/// POC 階段內建兩種標籤格式：
/// - QW075551-1: 出貨標籤（17 欄位）
/// - QW075551-2: 出貨標籤（14 欄位）
///
/// 所有座標單位為 mm [ref: raw_spec 13.1]
/// </summary>
public static class BuiltInTemplates
{
    /// <summary>
    /// 取得所有內建標籤格式
    /// </summary>
    public static IReadOnlyList<LabelTemplate> GetAll()
    {
        return new List<LabelTemplate>
        {
            CreateQW075551_1(),
            CreateQW075551_2()
        }.AsReadOnly();
    }

    /// <summary>
    /// 根據代碼取得標籤格式
    /// </summary>
    /// <param name="code">標籤代碼</param>
    /// <returns>標籤格式，若不存在則回傳 null</returns>
    public static LabelTemplate? GetByCode(string code)
    {
        return code switch
        {
            "QW075551-1" => CreateQW075551_1(),
            "QW075551-2" => CreateQW075551_2(),
            _ => null
        };
    }

    /// <summary>
    /// 建立 QW075551-1 出貨標籤格式
    /// [ref: raw_spec 5.1, data-model.md 5. QW075551-1, Delta Spec FR-001]
    ///
    /// 欄位數: 14 (原 17，移除 MoLabel/DeviceLabel/RemarkLabel 獨立標籤 [FR-013])
    /// 尺寸: 100mm × 80mm [FR-001]
    /// 外框: 有 [FR-003]
    /// QR Code pattern: {pono};{ima902};{ogd09};{nvr_remark10}
    /// Remarks 區段: QR Code 左側，CSMO/OUTDEVICENO/CSREMARK 右側並排 [FR-013, FR-014]
    /// </summary>
    private static LabelTemplate CreateQW075551_1()
    {
        return new LabelTemplate
        {
            Code = "QW075551-1",
            Name = "出貨標籤",
            WidthMm = 100,
            HeightMm = 80,  // [FR-001] 60 → 80
            HasBorder = true,  // [FR-003] 標籤外框
            Fields = new List<LabelField>
            {
                // Item 1: 標題 [ref: raw_spec 5.1]
                new()
                {
                    Name = "Title",
                    FieldType = FieldType.Text,
                    DataSource = "出貨標籤 Shipping Label",
                    IsConstant = true,
                    X = 5, Y = 2, Width = 90, Height = 6,
                    FontSize = 14, IsBold = true,
                    Alignment = TextAlignment.Center
                },

                // Item 2: 標籤 "Customer\n客戶名稱" (雙行顯示)
                // [FR-019] 欄位前綴標籤中英文分行
                new()
                {
                    Name = "CustomerLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Customer\n客戶名稱",  // [FR-019] 雙行顯示
                    IsConstant = true,
                    X = 5, Y = 10, Width = 20, Height = 8,  // Height 調整為 8mm
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 3: CSCUSTOMER (客戶名稱) <- nvr_cust
                new()
                {
                    Name = "CSCUSTOMER",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_cust",
                    IsConstant = false,
                    X = 28, Y = 10, Width = 67, Height = 5,  // [FR-002] 座標調整
                    FontSize = 11, IsBold = true,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 4: 標籤 "Date"
                new()
                {
                    Name = "DateLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Date",
                    IsConstant = true,
                    X = 5, Y = 19, Width = 10, Height = 4,  // [FR-002] Y 調整為 19
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 5: FINDPRTDC (日期) <- obe25
                // [FR-006] 日期格式 yyyy-MM-dd → yyyy/MM/dd (由 LabelRenderer 處理)
                new()
                {
                    Name = "FINDPRTDC",
                    FieldType = FieldType.Text,
                    DataSource = "obe25",
                    IsConstant = false,
                    X = 16, Y = 19, Width = 25, Height = 5,  // [FR-002] Y 調整為 19
                    FontSize = 11, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 6: 標籤 "Q'ty"
                new()
                {
                    Name = "QtyLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Q'ty",
                    IsConstant = true,
                    X = 55, Y = 19, Width = 10, Height = 4,  // [FR-002] Y 調整為 19
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 7: CSQTY (數量) <- ogd09 (Display Value 加千分位)
                new()
                {
                    Name = "CSQTY",
                    FieldType = FieldType.Text,
                    DataSource = "ogd09",
                    IsConstant = false,
                    UseDisplayValue = true,  // 使用千分位格式化 [ref: raw_spec 13.13]
                    X = 66, Y = 19, Width = 29, Height = 5,  // [FR-002] Y 調整為 19
                    FontSize = 11, IsBold = true,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 8: 標籤 "Product NO.\n產品型號" (雙行顯示)
                // [FR-004] 修正 "Peoduct" typo 為 "Product"
                // [FR-019] 欄位前綴標籤中英文分行
                new()
                {
                    Name = "ProductNoLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Product NO.\n產品型號",  // [FR-004, FR-019] 修正 typo + 雙行顯示
                    IsConstant = true,
                    X = 5, Y = 26, Width = 20, Height = 8,  // Height 調整為 8mm, Y 調整為 26
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 9: CSCUSTITEMNO (客戶料號) <- nvr_cust_item_no
                new()
                {
                    Name = "CSCUSTITEMNO",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_cust_item_no",
                    IsConstant = false,
                    X = 5, Y = 35, Width = 90, Height = 5,  // [FR-002] Y 調整為 35
                    FontSize = 11, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 10: CSCUSTPN (客戶 P/N) <- nvr_cust_pn (Text, 原為 Barcode)
                // [FR-005, FR-007] 由 Barcode 改為 Text，移除 Code 128 條碼
                new()
                {
                    Name = "CSCUSTPN",
                    FieldType = FieldType.Text,  // [FR-005] Barcode → Text
                    DataSource = "nvr_cust_pn",
                    IsConstant = false,
                    X = 5, Y = 41, Width = 90, Height = 5,  // [FR-002] 座標調整
                    FontSize = 11, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 11: 標籤 "Remarks" (新增統一標籤)
                // [FR-013] Remarks 區段統一標籤，取代原 MO:/Device:/Remark: 獨立標籤
                new()
                {
                    Name = "RemarksLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Remarks",
                    IsConstant = true,
                    X = 5, Y = 50, Width = 20, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 12: CSMO (製令單號) <- pono
                // [FR-014] 位於 QR Code 右側
                new()
                {
                    Name = "CSMO",
                    FieldType = FieldType.Text,
                    DataSource = "pono",
                    IsConstant = false,
                    X = 28, Y = 55, Width = 67, Height = 4,  // [FR-014] QR Code 右側
                    FontSize = 10, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 13: OUTDEVICENO (裝置編號) <- ima902
                // [FR-014] 位於 QR Code 右側
                new()
                {
                    Name = "OUTDEVICENO",
                    FieldType = FieldType.Text,
                    DataSource = "ima902",
                    IsConstant = false,
                    X = 28, Y = 60, Width = 67, Height = 4,  // [FR-014] QR Code 右側
                    FontSize = 10, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 14: CSREMARK (備註) <- nvr_remark10
                // [FR-014] 位於 QR Code 右側
                new()
                {
                    Name = "CSREMARK",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_remark10",
                    IsConstant = false,
                    X = 28, Y = 65, Width = 67, Height = 4,  // [FR-014] QR Code 右側
                    FontSize = 10, IsBold = false,
                    Alignment = TextAlignment.Left,
                    AutoShrinkFont = true  // [FR-008] 長文字縮小
                },

                // Item 15: QRCODE (組合欄位) <- {pono};{ima902};{ogd09};{nvr_remark10}
                // [FR-009] 內容格式: CSMO;OUTDEVICENO;CSQTY;CSREMARK
                // [FR-010] 使用 Raw Value（無千分位）
                // [FR-012] 位置移至左下角 (X=5, Y=55)
                // [FR-013] 與 CSMO/OUTDEVICENO/CSREMARK 並排
                // [ref: raw_spec 13.4, 13.15] - 空值保留位置 (A;;C)
                new()
                {
                    Name = "QRCODE",
                    FieldType = FieldType.QRCode,
                    DataSource = string.Empty,  // QR Code 使用 CombinePattern
                    IsConstant = false,
                    UseDisplayValue = false,  // [FR-010] QR Code 必須使用 Raw Value（無千分位）
                    CombinePattern = "{pono};{ima902};{ogd09};{nvr_remark10}",  // [FR-009] 使用 data source 欄位
                    X = 5, Y = 55, Width = 20, Height = 20,  // [FR-012] 左下角位置
                    Alignment = TextAlignment.Left
                }
            }
        };
    }

    /// <summary>
    /// 建立 QW075551-2 出貨標籤格式
    /// [ref: raw_spec 5.2, data-model.md 6. QW075551-2]
    ///
    /// 欄位數: 14
    /// 尺寸: 100mm × 60mm
    /// QR Code pattern: {cscustpo};{erpmat};{ogd09}
    /// </summary>
    private static LabelTemplate CreateQW075551_2()
    {
        return new LabelTemplate
        {
            Code = "QW075551-2",
            Name = "出貨標籤",
            WidthMm = 100,
            HeightMm = 60,
            Fields = new List<LabelField>
            {
                // Item 1: 標題 [ref: raw_spec 5.2]
                new()
                {
                    Name = "Title",
                    FieldType = FieldType.Text,
                    DataSource = "出貨標籤 Shipping Label",
                    IsConstant = true,
                    X = 5, Y = 2, Width = 90, Height = 6,
                    FontSize = 14, IsBold = true,
                    Alignment = TextAlignment.Center
                },

                // Item 2: 標籤 "Customer PO:"
                new()
                {
                    Name = "CustomerPoLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Customer PO:",
                    IsConstant = true,
                    X = 5, Y = 10, Width = 30, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 3: CSCUSTPO (客戶採購單號) <- cscustpo
                new()
                {
                    Name = "CSCUSTPO",
                    FieldType = FieldType.Text,
                    DataSource = "cscustpo",
                    IsConstant = false,
                    X = 36, Y = 10, Width = 59, Height = 5,
                    FontSize = 11, IsBold = true,
                    Alignment = TextAlignment.Left
                },

                // Item 4: 標籤 "CS Number:"
                new()
                {
                    Name = "CsNumberLabel",
                    FieldType = FieldType.Text,
                    DataSource = "CS Number:",
                    IsConstant = true,
                    X = 5, Y = 17, Width = 25, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 5: CSNUMBER (常數值 "17008")
                new()
                {
                    Name = "CSNUMBER",
                    FieldType = FieldType.Text,
                    DataSource = "17008",
                    IsConstant = true,  // 固定值 [ref: raw_spec 5.2]
                    X = 31, Y = 17, Width = 15, Height = 5,
                    FontSize = 11, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 6: 標籤 "Q'ty"
                new()
                {
                    Name = "QtyLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Q'ty",
                    IsConstant = true,
                    X = 55, Y = 17, Width = 10, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 7: CSQTY (數量) <- ogd09 (Display Value 加千分位)
                new()
                {
                    Name = "CSQTY",
                    FieldType = FieldType.Text,
                    DataSource = "ogd09",
                    IsConstant = false,
                    UseDisplayValue = true,  // 使用千分位格式化 [ref: raw_spec 13.13]
                    X = 66, Y = 17, Width = 29, Height = 5,
                    FontSize = 11, IsBold = true,
                    Alignment = TextAlignment.Left
                },

                // Item 8: 標籤 "ERP Part NO."
                new()
                {
                    Name = "ErpPartNoLabel",
                    FieldType = FieldType.Text,
                    DataSource = "ERP Part NO.",
                    IsConstant = true,
                    X = 5, Y = 24, Width = 30, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 9: ERPPARTNO (ERP 料號) <- erpmat (Barcode, Raw Value)
                new()
                {
                    Name = "ERPPARTNO",
                    FieldType = FieldType.Barcode,
                    DataSource = "erpmat",
                    IsConstant = false,
                    UseDisplayValue = false,  // Barcode 必須使用 Raw Value [ref: raw_spec 13.13]
                    X = 5, Y = 29, Width = 60, Height = 10,
                    Alignment = TextAlignment.Left
                },

                // Item 10: 標籤 "Customer Item:"
                new()
                {
                    Name = "CustomerItemLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Customer Item:",
                    IsConstant = true,
                    X = 5, Y = 42, Width = 30, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 11: CSCUSTITEMNO (客戶料號) <- nvr_cust_item_no
                new()
                {
                    Name = "CSCUSTITEMNO",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_cust_item_no",
                    IsConstant = false,
                    X = 36, Y = 42, Width = 30, Height = 4,
                    FontSize = 10, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 12: 標籤 "Remark:"
                new()
                {
                    Name = "RemarkLabel",
                    FieldType = FieldType.Text,
                    DataSource = "Remark:",
                    IsConstant = true,
                    X = 5, Y = 48, Width = 15, Height = 4,
                    FontSize = 9, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 13: CSREMARK (備註) <- nvr_remark10
                new()
                {
                    Name = "CSREMARK",
                    FieldType = FieldType.Text,
                    DataSource = "nvr_remark10",
                    IsConstant = false,
                    X = 21, Y = 48, Width = 45, Height = 4,
                    FontSize = 10, IsBold = false,
                    Alignment = TextAlignment.Left
                },

                // Item 14: QRCODE (組合欄位) <- {cscustpo};{erpmat};{ogd09}
                // [ref: raw_spec 13.4, 13.15] - 空值保留位置 (A;;C)
                new()
                {
                    Name = "QRCODE",
                    FieldType = FieldType.QRCode,
                    DataSource = string.Empty,  // QR Code 使用 CombinePattern
                    IsConstant = false,
                    UseDisplayValue = false,  // QR Code 必須使用 Raw Value [ref: raw_spec 13.13]
                    CombinePattern = "{cscustpo};{erpmat};{ogd09}",
                    X = 75, Y = 40, Width = 20, Height = 20,
                    Alignment = TextAlignment.Left
                }
            }
        };
    }
}
