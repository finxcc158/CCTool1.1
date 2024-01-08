using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Range = Aspose.Cells.Range;
using NPOI.SS.Formula.Functions;
using ActiproSoftware.Windows;
using NPOI.XSSF.Streaming.Values;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace CCTool.Scripts.ToolManagers
{
    public class OfficeTool
    {
        // 打开工作薄
        public static Workbook OpenWorkbook(string excelFile)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            LoadOptions loadOptinos = new LoadOptions(LoadFormat.Xlsx);
            Workbook wb = new Workbook(excelFile, loadOptinos);
            return wb;
        }
        
        // 获取TXT文件的编码类型
        public static System.Text.Encoding GetEncodingType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        // 通过给定的文件流，判断文件的编码类型 
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }

        // 判断是否是不带 BOM 的 UTF8 格式 
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

        // Excel文件属性映射【输入映射字典dict】
        public static void ExcelAttributeMapper(string excelPath, int sheet_in_col, int sheet_map_col, Dictionary<string, string> dict, int startRow = 0)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 逐行处理
            for (int i = startRow; i <= sheet.Cells.MaxDataRow; i++)
            {
                //  获取目标cell
                Cell inCell = sheet.Cells[i, sheet_in_col];
                Cell mapCell = sheet.Cells[i, sheet_map_col];
                // 属性映射
                if (inCell is not null && dict.ContainsKey(inCell.StringValue))
                {
                    mapCell.Value =dict[inCell.StringValue];   // 赋值
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件属性映射【输入映射字典dict】
        public static void ExcelAttributeMapperDouble(string excelPath, int sheet_in_col, int sheet_map_col, Dictionary<string, string> dict, int startRow = 0)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 逐行处理
            for (int i = startRow; i <= sheet.Cells.MaxDataRow; i++)
            {
                //  获取目标cell
                Cell inCell = sheet.Cells[i, sheet_in_col];
                Cell mapCell = sheet.Cells[i, sheet_map_col];
                // 属性映射
                if (inCell is not null && dict.ContainsKey(inCell.StringValue))
                {
                    mapCell.Value = double.Parse(dict[inCell.StringValue]);   // 赋值
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件属性映射_列【输入映射字典dict】
        public static void ExcelAttributeMapperCol(string excelPath, int sheet_in_row, int sheet_map_row, Dictionary<string, string> dict, int startCol = 0)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 逐行处理
            for (int i = startCol; i <= sheet.Cells.MaxDataColumn; i++)
            {
                //  获取目标cell
                Cell inCell = sheet.Cells[sheet_in_row, i];
                Cell mapCell = sheet.Cells[sheet_map_row, i];
                // 属性映射
                if (inCell is not null && dict.ContainsKey(inCell.StringValue))
                {
                    mapCell.Value = dict[inCell.StringValue];   // 赋值
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件属性映射_列【输入映射字典dict】
        public static void ExcelAttributeMapperColDouble(string excelPath, int sheet_in_row, int sheet_map_row, Dictionary<string, string> dict, int startCol = 0)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 逐行处理
            for (int i = startCol; i <= sheet.Cells.MaxDataColumn; i++)
            {
                //  获取目标cell
                Cell inCell = sheet.Cells[sheet_in_row, i];
                Cell mapCell = sheet.Cells[sheet_map_row, i];
                // 属性映射
                if (inCell is not null && dict.ContainsKey(inCell.StringValue))
                {
                    mapCell.Value = double.Parse(dict[inCell.StringValue]);   // 赋值
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // 复制Excel表中sheet
        public static void ExcelCopySheet(string excelFile, string oldSheet, string newSheet)
        {
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheet(oldSheet);
            // 复制sheet
            sheet.CopySheet(newSheet);
            // 保存工作薄
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // 删除Excel表中sheet
        public static void ExcelDeleteSheet(string excelPath, string sheetName)
        {
            // 创建文件流
            FileStream fs = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheet(sheetName);
            // 删除表
            wb.Remove(sheet);
            // 保存工作薄
            wb.Write(new FileStream(excelPath, FileMode.Create, FileAccess.Write));
        }

        // Excel文件cell写入
        public static void ExcelWriteCell(string excelPath, int row, int col, string cell_value)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取cell
            Cell cell = sheet.Cells[row,col];
            // 写入cell值
            cell.Value =cell_value;
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel设置列单元格的格式
        public static void ExcelSetColStyle(string excelPath, int col, int startRow, int styleNumber = 4, int digit = 2)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            
            for (int i = startRow; i <= sheet.Cells.MaxDataRow; i++)
            {
                // 获取cell
                Cell cell = sheet.Cells[i, col];
                // 获取style
                Style style = cell.GetStyle();
                // 数字型
                style.Number = styleNumber;
                // 小数位数
                if (digit == 1) { style.Custom = "0.0"; }
                else if (digit == 2) { style.Custom = "0.00"; }
                else if (digit == 3) { style.Custom = "0.000"; }
                else if (digit == 4) { style.Custom = "0.0000"; }
            }

            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }


        // Excel文件删除行(无合并格的情况)
        private static void ExcelDelectRowSimple(string excelPath, int row)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 删除行
            sheet.Cells.DeleteRow(row);

            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件删除行(无合并格的情况)
        private static void ExcelDelectRowSimple(string excelPath, List<int> rows)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 删除行
            foreach (var row in rows)
            {
                sheet.Cells.DeleteRow(row);
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件删除列(无合并格的情况)
        private static void ExcelDelectColSimple(string excelPath, int col)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 删除列
            sheet.Cells.DeleteColumn(col);
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // Excel文件删除多列(无合并格的情况)
        private static void ExcelDelectColSimple(string excelPath, List<int> cols)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 删除列
            foreach (var col in cols)
            {
                sheet.Cells.DeleteColumn(col);
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
        }

        // 删除sheet表中的所有合并格，并填充默认值
        private static List<CellRangeAddress> ExcelRemoveMerge(string excelPath)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 获取所有合并区域
            List<CellRangeAddress> mergeRanges = sheet.MergedRegions;
            // 设置一个List<CellRangeAddress>
            List<CellRangeAddress> upDataRanges = new List<CellRangeAddress>();
            // 检查并清除合并区域
            if (mergeRanges.Count > 0)
            {
                for (int i = mergeRanges.Count - 1; i >= 0; i--)
                {
                    // 合并格的四至
                    CellRangeAddress region = mergeRanges[i];
                    int firstRow = region.FirstRow;
                    int lastRow = region.LastRow;
                    int firstCol = region.FirstColumn;
                    int lastCol = region.LastColumn;
                    // 判定要处理的区域
                    for (int row = firstRow; row <= lastRow; row++)
                    {
                        for (int col = firstCol; col <= lastCol; col++)
                        {
                            if (row != firstRow || col != firstCol)
                            {
                                IRow r = sheet.GetRow(row);
                                ICell c = r.GetCell(col);
                                // 如果c是空值，则赋一个默认值
                                c ??= r.CreateCell(col);
                                // 设置拥有合并区域的单元格的值为合并区域的值
                                ICell mergedCell = sheet.GetRow(firstRow).GetCell(firstCol);

                                if (mergedCell != null)
                                {
                                    c.SetCellValue(mergedCell.StringCellValue); // 可根据需要选择相应的数据类型
                                }
                            }
                        }
                    }
                    // 计入
                    upDataRanges.Add(region);
                    // 清除合并区域
                    sheet.RemoveMergedRegion(i);
                }
            }
            // 保存工作簿
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
            // 返回
            return upDataRanges;
        }

        // 合并单元格（依据输入的CellRangeAddress和delectRowrow进行判断）
        private static void ExcelMergeFromAddressRow(string excelPath, List<CellRangeAddress> mergeRanges, int delectRow)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 检查并更新合并区域
            foreach (CellRangeAddress mergeRange in mergeRanges)
            {
                // 如果合并格只有一行，且就是删除行，则不纳入合并的处理范围
                if (!(mergeRange.LastRow == mergeRange.FirstRow && mergeRange.LastRow == delectRow))
                {
                    // 如果合并格删除行的影响范围内
                    if (delectRow <= mergeRange.LastRow)
                    {
                        mergeRange.LastRow -= 1;
                    }
                    if (delectRow < mergeRange.FirstRow)
                    {
                        mergeRange.FirstRow -= 1;
                    }
                    // 重新合并单元格   判断合并单元格的格子数，不是单格才合并
                    if (mergeRange.NumberOfCells > 1)
                    {
                        sheet.AddMergedRegion(mergeRange);
                    }
                }
            }
            // 保存工作簿
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // 合并单元格（依据输入的CellRangeAddress和delectRowrow进行判断）
        private static void ExcelMergeFromAddressRow(string excelPath, List<CellRangeAddress> mergeRanges, List<int> delectRows)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 检查并更新合并区域
            foreach (CellRangeAddress mergeRange in mergeRanges)
            {
                bool isOver = false;
                foreach (int delectRow in delectRows)
                {
                    // 如果合并格只有一行，且就是删除行，则不纳入合并的处理范围
                    if (!(mergeRange.LastRow == mergeRange.FirstRow && mergeRange.LastRow == delectRow))
                    {
                        // 如果合并格删除行的影响范围内
                        if (delectRow <= mergeRange.LastRow)
                        {
                            mergeRange.LastRow -= 1;
                        }
                        if (delectRow < mergeRange.FirstRow)
                        {
                            mergeRange.FirstRow -= 1;
                        }
                    }
                    else
                    {
                        isOver = true;
                    }
                }
                // 重新合并单元格   判断合并单元格的格子数，不是单格才合并
                if (isOver==false && mergeRange.NumberOfCells > 1)
                {
                    sheet.AddMergedRegion(mergeRange);
                }
            }

            // 保存工作簿
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // 合并单元格（依据输入的CellRangeAddress和delectRowrow进行判断）
        private static void ExcelMergeFromAddressCol(string excelPath, List<CellRangeAddress> mergeRanges, int delectCol)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 检查并更新合并区域
            foreach (CellRangeAddress mergeRange in mergeRanges)
            {
                // 如果合并格只有一行，且就是删除行，则不纳入合并的处理范围
                if (!(mergeRange.LastColumn == mergeRange.FirstColumn && mergeRange.LastColumn == delectCol))
                {
                    // 如果合并格删除行的影响范围内
                    if (delectCol <= mergeRange.LastColumn)
                    {
                        mergeRange.LastColumn -= 1;
                    }
                    if (delectCol < mergeRange.FirstColumn)
                    {
                        mergeRange.FirstColumn -= 1;
                    }
                    // 重新合并单元格   判断合并单元格的格子数，不是单格才合并
                    if (mergeRange.NumberOfCells > 1)
                    {
                        sheet.AddMergedRegion(mergeRange);
                    }
                }
            }
            // 保存工作簿
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // 合并单元格（依据输入的CellRangeAddress和delectRowrow进行判断）
        private static void ExcelMergeFromAddressCol(string excelPath, List<CellRangeAddress> mergeRanges, List<int> delectCols)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 检查并更新合并区域
            foreach (CellRangeAddress mergeRange in mergeRanges)
            {
                bool isOver = false;
                // 循环每个删除列
                foreach (var delectCol in delectCols)
                {
                    // 如果合并格只有一行，且就是删除行，则不纳入合并的处理范围
                    if (!(mergeRange.LastColumn == mergeRange.FirstColumn && mergeRange.LastColumn == delectCol))
                    {
                        // 如果合并格删除行的影响范围内
                        if (delectCol <= mergeRange.LastColumn)
                        {
                            mergeRange.LastColumn -= 1;
                        }
                        if (delectCol < mergeRange.FirstColumn)
                        {
                            mergeRange.FirstColumn -= 1;
                        }
                    }
                    else
                    {
                        isOver = true;
                    }
                }
                // 重新合并单元格   判断合并单元格的格子数，不是单格才合并
                if (isOver == false && mergeRange.NumberOfCells > 1)
                {
                    sheet.AddMergedRegion(mergeRange);
                }
            }
            // 保存工作簿
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // Excel文件删除行
        public static void ExcelDeleteRow(string excelPath, int delectRow)
        {
            // 删除sheet表中的所有合并格，并填充默认值
            List<CellRangeAddress> mergeRanges = ExcelRemoveMerge(excelPath);
            // 删除行
            ExcelDelectRowSimple(excelPath, delectRow);
            // 合并单元格（依据输入的CellRangeAddress和delectRowrow进行判断）
            ExcelMergeFromAddressRow(excelPath, mergeRanges, delectRow);
        }

        // Excel文件删除多行
        public static void ExcelDeleteRow(string excelPath, List<int> delectRows)
        {
            // 删除sheet表中的所有合并格，并填充默认值
            List<CellRangeAddress> mergeRanges = ExcelRemoveMerge(excelPath);
            // 删除多行
            ExcelDelectRowSimple(excelPath, delectRows);
            // 合并单元格（依据输入的CellRangeAddress和delectCol进行判断）
            ExcelMergeFromAddressRow(excelPath, mergeRanges, delectRows);
        }

        // Excel文件删除列
        public static void ExcelDeleteCol(string excelPath, int delectCol)
        {
            // 删除sheet表中的所有合并格，并填充默认值
            List<CellRangeAddress> mergeRanges = ExcelRemoveMerge(excelPath);
            // 删除列
            ExcelDelectColSimple(excelPath, delectCol);
            // 合并单元格（依据输入的CellRangeAddress和delectCol进行判断）
            ExcelMergeFromAddressCol(excelPath, mergeRanges, delectCol);
        }

        // Excel文件删除多列
        public static void ExcelDeleteCol(string excelPath, List<int> delectCols)
        {
            // 删除sheet表中的所有合并格，并填充默认值
            List<CellRangeAddress> mergeRanges = ExcelRemoveMerge(excelPath);
            // 删除多列
            ExcelDelectColSimple(excelPath, delectCols);
            // 合并单元格（依据输入的CellRangeAddress和delectCol进行判断）
            ExcelMergeFromAddressCol(excelPath, mergeRanges, delectCols);
        }

        // 删除Excel表中的0值行【指定1个列】
        private static List<int> ExcelDeleteNullRowResult(string excelPath, int deleteCol, int startRow = 0)
        {
            List<int> list = new List<int>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];

            // 强制更新表内的公式单元格
            wb.CalculateFormula();

            // 找出0值行
            for (int i = sheet.Cells.MaxDataRow; i >= startRow; i--)
            {
                Cell cell = sheet.Cells.GetCell(i, deleteCol);
                if (cell == null)  // 值为空则纳入
                {
                    list.Add(i);
                }
                else
                {
                    string str = cell.StringValue;
                    if (str == "")  // 值为0也纳入
                    {
                        list.Add(i);
                    }
                    else if (double.Parse(str) == 0)
                    {
                        list.Add(i);
                    }
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();

            // 返回值
            return list;
        }

        // 删除Excel表中的0值行【指定多个列】
        private static List<int> ExcelDeleteNullRowResult(string excelPath, List<int> deleteCols, int startRow = 0)
        {
            List<int> list = new List<int>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];

            // 强制更新表内的公式单元格
            wb.CalculateFormula();

            // 找出0值行
            for (int i = sheet.Cells.MaxDataRow; i >= startRow; i--)
            {
                // 设置一个flag
                bool isNull = true;
                // 循环查找各列的值
                foreach (var deleteCol in deleteCols)
                {
                    Cell cell = sheet.Cells.GetCell(i, deleteCol);
                    if (cell != null)  // 值不为空
                    {
                        string str = cell.StringValue;
                        if (str != "") // 值不为0
                        {
                            if (double.Parse(str) != 0)
                            {
                                isNull = false;
                                break;
                            }
                        }
                    }
                }
                // 输出删除列
                if (isNull)
                {
                    list.Add(i);
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();
            // 返回值
            return list;
        }

        // 删除Excel表中的0值行【指定1个列】
        private static List<int> ExcelDeleteNullColResult(string excelPath, int deleteRow, int startCol = 0)
        {
            List<int> list = new List<int>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];

            // 强制更新表内的公式单元格
            wb.CalculateFormula();

            // 找出0值行
            for (int i = sheet.Cells.MaxDataColumn; i >= startCol; i--)
            {
                Cell cell = sheet.Cells.GetCell(deleteRow, i);
                if (cell == null)  // 值为空则纳入
                {
                    list.Add(i);
                }
                else
                {
                    string str = cell.StringValue;
                    if (str == "")  // 值为0也纳入
                    {
                        list.Add(i);
                    }
                    else if (double.Parse(str) == 0)
                    {
                        list.Add(i);
                    }
                }
            }
            // 保存
            wb.Save(excelPath);
            wb.Dispose();

            // 返回值
            return list;
        }

        // 删除Excel表中的0值行【指定多个列】
        private static List<int> ExcelDeleteNullColResult(string excelPath, List<int> deleteRows, int startCol = 0)
        {
            List<int> list = new List<int>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];

            // 强制更新表内的公式单元格
            wb.CalculateFormula();

            // 找出0值行
            for (int i = sheet.Cells.MaxDataColumn; i >= startCol; i--)
            {
                // 设置一个flag
                bool isNull = true;
                // 循环查找各列的值
                foreach (var deleteRow in deleteRows)
                {
                    Cell cell = sheet.Cells.GetCell(deleteRow, i);
                    if (cell != null)  // 值不为空
                    {
                        string str = cell.StringValue;
                        if (str != "") // 值不为0
                        {
                            if (double.Parse(str) != 0)
                            {
                                isNull = false;
                                break;
                            }
                        }
                    }
                }
                // 输出删除列
                if (isNull)
                {
                    list.Add(i);
                }
            }
            // 保存
            wb.Save(excelFile);
            wb.Dispose();

            // 返回值
            return list;
        }

        // 删除Excel表中的0值行【指定1个列】
        public static void ExcelDeleteNullRow(string excelPath, int deleteCol, int startRow = 0)
        {
            // 要删除行
            List<int> deleleRows = ExcelDeleteNullRowResult(excelPath, deleteCol, startRow);
            // 删除行
            ExcelDeleteRow(excelPath, deleleRows);
        }

        // 删除Excel表中的0值行【指定多个列】
        public static void ExcelDeleteNullRow(string excelPath, List<int> deleteCols, int startRow = 0)
        {
            // 要删除行
            List<int> deleleRows = ExcelDeleteNullRowResult(excelPath, deleteCols, startRow);
            // 删除行
            ExcelDeleteRow(excelPath, deleleRows);
        }

        // 删除Excel表中的0值列【指定1个列】
        public static void ExcelDeleteNullCol(string excelPath, int deleteRow,int startCol = 0)
        {
            // 要删除列
            List<int> deleleRows = ExcelDeleteNullColResult(excelPath, deleteRow, startCol);
            // 删除列
            ExcelDeleteCol(excelPath, deleleRows);
        }

        // 删除Excel表中的0值列【指定多个列】
        public static void ExcelDeleteNullCol(string excelPath, List<int> deleteRows, int startCol = 0)
        {
            // 要删除列
            List<int> deleleCols = ExcelDeleteNullColResult(excelPath, deleteRows, startCol);
            // 删除列
            ExcelDeleteCol(excelPath, deleleCols);
        }

        // 复制Excel表中的行
        public static void ExcelCopyRow(string excelPath, int sourceRowIndex, int targetRowIndex)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 创建文件流
            FileStream fs = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // 打开工作薄
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            // 获取工作表
            ISheet sheet = wb.GetSheetAt(sheetIndex);
            // 复制行
            SheetUtil.CopyRow(sheet, sourceRowIndex, targetRowIndex);
            // 保存工作薄
            wb.Write(new FileStream(excelFile, FileMode.Create, FileAccess.Write));
        }

        // 从Excel文件中获取Dictionary
        public static Dictionary<string, string> GetDictFromExcel(string excelPath, int col1 = 0, int col2 = 1)
        {
            // 定义字典
            Dictionary<string, string> dict = new Dictionary<string, string>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取key和value值
            for (int i = 0; i <= sheet.Cells.MaxDataRow; i++)
            {
                Cell key = sheet.Cells[i, col1];
                Cell value = sheet.Cells[i, col2];
                if (key != null && value != null)
                {
                    if (!dict.ContainsKey(key.StringValue))
                    {
                        if (key.StringValue != "" && value.StringValue != "")   // 空值不纳入
                        {
                            dict.Add(key.StringValue, value.StringValue);
                        }
                    }
                }
            }
            wb.Dispose();
            // 返回dict
            return dict;
        }

        // 从Excel文件中获取Dictionary
        public static Dictionary<string, string> GetDictFromExcelAll(string excelPath, int col1 = 0, int col2 = 1)
        {
            // 定义字典
            Dictionary<string, string> dict = new Dictionary<string, string>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取key和value值
            for (int i = 0; i <= sheet.Cells.MaxDataRow; i++)
            {
                Cell key = sheet.Cells[i,col1];
                Cell value = sheet.Cells[i, col2];
                if (key!=null && value!=null)
                {
                    if (!dict.ContainsKey(key.StringValue))
                    {
                        dict.Add(key.StringValue, value.StringValue);
                    }
                }
            }
            wb.Dispose();
            // 返回dict
            return dict;
        }

        // 从Excel文件中获取List
        public static List<string> GetListFromExcel(string excelPath, int col, int startRow =0)
        {
            // 定义列表
            List<string> list = new List<string>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取key和value值
            for (int i = startRow; i <= sheet.Cells.MaxDataRow; i++)
            {
                Cell cell = sheet.Cells[i,col];
                if (cell != null)
                {
                    string strValue = cell.StringValue;
                    if (!list.Contains(strValue) && strValue != "")
                    {
                        list.Add(strValue);
                    }
                }
            }
            wb.Dispose();
            // 返回list
            return list;
        }

        // 从Excel文件中获取List
        public static List<string> GetListFromExcelAll(string excelPath, int col, int startRow = 0)
        {
            // 定义列表
            List<string> list = new List<string>();
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取key和value值
            for (int i = startRow; i <= sheet.Cells.MaxDataRow; i++)
            {
                Cell cell = sheet.Cells[i, col];
                if (cell != null)
                {
                    string strValue = cell.StringValue;
                    list.Add(strValue);
                }
            }
            wb.Dispose();
            // 返回list
            return list;
        }

        // 从Excel文件中获取Cellvalue
        public static string GetCellFromExcel(string excelPath, int row, int col)
        {
            // 定义value
            string value = "";
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 获取value值
            Cell cell = sheet.Cells[row, col];
            if (cell != null)
            {
                value = cell.StringValue;
            }
            wb.Dispose();
            // 返回value
            return value;
        }

        // Excel指定范围导出JPG图片
        public static void ExcelImportToJPG(string excelPath, string outputPath)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];
            // 设置打印属性
            ImageOrPrintOptions imgOptions = new ImageOrPrintOptions();
            // 在一页内打印
            imgOptions.OnePagePerSheet = true;
            // 只打印区域内
            imgOptions.OnlyArea = true;
            // 打印
            SheetRender render = new SheetRender(sheet, imgOptions);
            render.ToImage(0, outputPath);
            // 保存
            wb.Save(excelPath);
            wb.Dispose();
        }

        // Excel文件导出图片(阿来来)
        public static void Excel2Pic(string excelPath)
        {
            // 获取工作薄、工作表
            string excelFile = excelPath.GetExcelPath();
            int sheetIndex = excelPath.GetExcelSheetIndex();
            // 打开工作薄
            Workbook wb = OpenWorkbook(excelFile);
            // 打开工作表
            Worksheet sheet = wb.Worksheets[sheetIndex];

            sheet.PageSetup.LeftMargin = 1;
            sheet.PageSetup.RightMargin = 1;
            sheet.PageSetup.BottomMargin = 1;
            sheet.PageSetup.TopMargin = 1;

            ImageOrPrintOptions imgOptions = new ImageOrPrintOptions();

            imgOptions.OnePagePerSheet = true;
            imgOptions.PrintingPage = PrintingPageType.IgnoreBlank;

            SheetRender sr = new SheetRender(sheet, imgOptions);
            string parentDirectory = System.IO.Directory.GetParent(excelPath).FullName;
            string wbName = wb.FileName[(wb.FileName.LastIndexOf(@"\") + 1)..].Replace(".xslx","");

            string pathsave = parentDirectory + $@"\{wbName}.jpg";
            sr.ToImage(0, pathsave);

            // 保存
            wb.Dispose();
        }
    }
}
