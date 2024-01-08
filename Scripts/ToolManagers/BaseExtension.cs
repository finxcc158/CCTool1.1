using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.Internal.Mapping.Symbology;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Asn1.X509;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.ToolManagers
{
    public static class BaseExtension
    {
        // 获取Excel的文件名
        public static string GetAttFormTxtJson(this string txtPath, string attName)
        {
            // 读取文件内容
            string filePath = txtPath;
            string jsonContent = File.ReadAllText(filePath);

            // 解析 JSON 数据
            JObject jsonObject = JObject.Parse(jsonContent);

            // 获取 "dpi" 属性的值
            string Value = (string)jsonObject[attName];

            return Value;
        }

        // 获取Excel的文件名
        public static string GetExcelPath(this string excelPath)
        {
            // 如果是完整路径（包含sheet$）
            if (excelPath.Contains('$'))
            {
                // 获取最后一个"\"的位置
                int index = excelPath.LastIndexOf("\\");
                // 获取exl文件名
                string excel_name = excelPath[..index];
                // 返回exl文件名
                return excel_name;
            }
            // 如果只excel文件路径
            else
            {
                return excelPath;
            }
        }

        // 获取Excel的表序号
        public static int GetExcelSheetIndex(this string excelPath)
        {
            // 如果是完整路径（包含sheet$）
            if (excelPath.Contains('$'))
            {
                // 获取最后一个"\"的位置
                int index = excelPath.LastIndexOf("\\");
                // 获取exl文件名
                string excelName = excelPath[..index];
                // 获取表名
                string sheet_name = excelPath.Substring(index + 1, excelPath.Length - index - 2);

                // 创建文件流
                FileStream fs = File.Open(excelName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                // 打开工作薄
                XSSFWorkbook wb = new XSSFWorkbook(fs);
                // 获取第一个工作表
                ISheet sheet = wb.GetSheet(sheet_name);
                //  返回index
                return wb.GetSheetIndex(sheet);
            }
            // 如果只excel文件路径
            else
            {
                return 0;
            }
        }

        // 通过路径或图层名获取输入的工作空间
        public static string TargetPath(this string filePath)
        {
            // 兼容两种符号
            string inputData = filePath.Replace(@"/", @"\");
            // 如果是GDB文件路径
            if (inputData.Contains(".gdb"))
            {
                // 获取最后一个".gdb"的位置
                int index = inputData.LastIndexOf(@".gdb");
                // 获取gdb文件名
                string gdbPath = inputData[..(index + 4)];
                // 返回gdb路径
                return gdbPath;
            }
            // 如果是SHP文件路径
            else if (inputData.Contains(".shp"))
            {
                // 获取最后一个"\"的位置
                int index = inputData.LastIndexOf(@"\");
                // 获取要素名
                string shpPath = inputData[..index];
                // 返回shp文件路径
                return shpPath;
            }
            // 如果是图层名
            else
            {
                // 获取图层数据的完整路径
                string fullPath = inputData.LayerSourcePath();
                // 递归返回工作空间
                return fullPath.TargetPath();
            }
        }

        // 通过路径或图层名获取输入的要素名称
        public static string TargetName(this string filePath)
        {
            // 兼容两种符号
            string inputData = filePath.Replace(@"/", @"\");
            // 如果是GDB文件路径
            if (inputData.Contains(".gdb"))
            {
                // 获取最后一个"\"的位置
                int index = filePath.LastIndexOf(@"\");
                // 获取要素名
                string targetName = filePath[(index + 1)..];
                // 返回
                return targetName;
            }
            // 如果是SHP文件路径
            else if (inputData.Contains(".shp"))
            {
                // 获取最后一个"\"的位置
                int index = inputData.LastIndexOf(@"\");
                // 获取要素名
                string targetName = inputData[(index + 1)..];
                // 返回shp文件路径
                return targetName;
            }
            // 如果是图层名
            else
            {
                // 获取图层数据的完整路径
                string fullPath = inputData.LayerSourcePath();
                // 递归返回工作空间
                return fullPath.TargetName();
            }
        }

        // 通过图层名获取数据的完整路径
        public static string LayerSourcePath(this string layerName)
        {
            Map map = MapView.Active.Map;
            Dictionary<FeatureLayer, string> dic_ly = map.AllFeatureLayersDic();
            Dictionary<StandaloneTable, string> dic_table = map.AllStandaloneTablesDic();

            // 获取完整路径
            string targetPath = "";
            // 如果是图层
            if (dic_ly.Values.Contains(layerName))
            {
                foreach (var item in dic_ly)
                {
                    if (item.Value == layerName)
                    {
                        targetPath = item.Key.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    }
                }
            }
            // 如果是独立表
            else if (dic_table.Values.Contains(layerName))
            {
                foreach (var item in dic_table)
                {
                    if (item.Value == layerName)
                    {
                        targetPath = item.Key.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    }
                }
            }

            // shp的情况，需要加上.shp
            if (!targetPath.Contains(".gdb"))
            {
                targetPath += ".shp";
            }
            // 返回完整路径
            return targetPath;
        }

        // 通过图层获取数据的完整路径
        public static string LayerSourcePath(this FeatureLayer featureLayer)
        {
            string layerPath = featureLayer.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
            // shp的情况，需要加上.shp
            if (!layerPath.Contains(".gdb"))
            {
                layerPath += ".shp";
            }
            // 返回完整路径
            return layerPath;
        }

        // 通过路径或图层名获取目标要素的属性表
        public static Table TargetTable(this string filePath)
        {
            // 获取目录的路径和名称
            string targetPath = filePath.TargetPath();
            string targetName = filePath.TargetName();
            // 如果是GDB数据
            if (filePath.Contains(".gdb"))
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(targetPath))))
                {
                    Table table = geodatabase.OpenDataset<Table>(targetName);
                    return table;
                }
            }
            // 如果是SHP数据
            else if (filePath.Contains(".shp"))
            {
                FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new Uri(targetPath), FileSystemDatastoreType.Shapefile);
                using (FileSystemDatastore shapefile = new FileSystemDatastore(connectionPath))
                {
                    Table table = shapefile.OpenDataset<Table>(targetName);
                    return table;
                }
            }
            else
            {
                // 获取图层的完整路径
                string layerSourcePath = filePath.LayerSourcePath();
                Table table = layerSourcePath.TargetTable();
                return table;
            }
        }

        // 获取输入文件夹下的所有文件
        public static List<string> GetAllFiles(this string folder_path, string key_word = "no match")
        {
            List<string> filePaths = new List<string>();

            // 获取当前文件夹下的所有文件
            string[] files = Directory.GetFiles(folder_path);
            // 判断是否包含关键字
            if (key_word == "no match")
            {
                filePaths.AddRange(files);
            }
            else
            {
                foreach (string file in files)
                {
                    // 检查文件名是否包含指定扩展名
                    if (Path.GetExtension(file).Equals(key_word, StringComparison.OrdinalIgnoreCase))
                    {
                        filePaths.Add(file);
                    }
                }
            }

            // 获取当前文件夹下的所有子文件夹
            string[] subDirectories = Directory.GetDirectories(folder_path);

            // 递归遍历子文件夹下的文件
            foreach (string subDirectory in subDirectories)
            {
                filePaths.AddRange(GetAllFiles(subDirectory, key_word));
            }

            return filePaths;
        }

        // 获取输入文件夹下的所有GDB文件
        public static List<string> GetAllGDBFilePaths(this string folderPath)
        {
            List<string> gdbFilePaths = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            // 检查文件夹是否存在
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("指定的文件夹路径不存在！");
            }

            // 查找所有GDB数据库文件（.gdb文件夹）
            DirectoryInfo[] gdbDirectories = directoryInfo.GetDirectories("*.gdb", SearchOption.AllDirectories);
            foreach (DirectoryInfo gdbDirectory in gdbDirectories)
            {
                // 获取GDB数据库的路径
                string gdbPath = gdbDirectory.FullName.Replace(@"/", @"\");

                // 添加到列表中
                gdbFilePaths.Add(gdbPath);
            }

            return gdbFilePaths;
        }

        //  获取字段的所有唯一值
        public static List<string> GetFieldValues(this string targetPath, string fieldName)
        {
            List<string> fieldValues = new List<string>();
            // 获取Table
            Table table = targetPath.TargetTable();
            // 逐行找出错误
            using RowCursor rowCursor = table.Search();
            while (rowCursor.MoveNext())
            {
                using ArcGIS.Core.Data.Row row = rowCursor.Current;
                // 获取value
                var va = row[fieldName];
                if (va != null)
                {
                    string result = va.ToString();
                    // 如果不在列表中，就加入
                    if (!fieldValues.Contains(result))
                    {
                        fieldValues.Add(va.ToString());
                    }
                }
            }
            return fieldValues;
        }

        //  获取字段的所有唯一值(含同一值个数)
        public static Dictionary<string, long> GetFieldValuesDic(this string targetPath, string fieldName)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();
            // 获取Table
            Table table = targetPath.TargetTable();
            // 逐行找出错误
            using RowCursor rowCursor = table.Search();
            while (rowCursor.MoveNext())
            {
                using ArcGIS.Core.Data.Row row = rowCursor.Current;
                // 获取value
                var va = row[fieldName];
                if (va != null)
                {
                    string result = va.ToString();
                    // 如果不在列表中，就加入
                    if (!dic.ContainsKey(result))
                    {
                        dic.Add(va.ToString(), 1);
                    }
                    // 如果已有，则加数
                    else
                    {
                        dic[result] += 1;
                    }
                }
            }
            return dic;
        }

        // 富文本添加信息框文字
        public static void AddMessage(this RichTextBox tb_message, string add_text, SolidColorBrush solidColorBrush = null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (solidColorBrush == null)
                {
                    solidColorBrush = Brushes.Black;
                }
                // 创建一个新的TextRange对象，范围为新添加的文字
                TextRange newRange = new TextRange(tb_message.Document.ContentEnd, tb_message.Document.ContentEnd)
                {
                    Text = add_text
                };
                // 设置新添加文字的颜色
                newRange.ApplyPropertyValue(TextElement.ForegroundProperty, solidColorBrush);
                // 设置新添加文字的样式
                newRange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            });
        }


        // 通过路径或图层名获取目标要素FeatureClass
        public static FeatureClass TargetFeatureClass(this string filePath)
        {
            // 获取目录的路径和名称
            string targetPath = filePath.TargetPath();
            string targetName = filePath.TargetName();
            // 如果是GDB数据
            if (filePath.Contains(".gdb"))
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(targetPath))))
                {
                    FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(targetName);
                    return featureClass;
                }
            }
            // 如果是SHP数据
            else if (filePath.Contains(".shp"))
            {
                FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new Uri(targetPath), FileSystemDatastoreType.Shapefile);
                using (FileSystemDatastore shapefile = new FileSystemDatastore(connectionPath))
                {
                    FeatureClass featureClass = shapefile.OpenDataset<FeatureClass>(targetName);
                    return featureClass;
                }
            }
            else
            {
                // 获取图层的完整路径
                string layerSourcePath = filePath.LayerSourcePath();
                FeatureClass featureClass = layerSourcePath.TargetFeatureClass();
                return featureClass;
            }
        }

        // 通过图层名获取目标要素的FeatureLayer
        public static FeatureLayer TargetFeatureLayer(this string layerName)
        {
            Object ob = layerName.GetLayerFromFullName();
            if (ob is FeatureLayer)
            {
                return (FeatureLayer)ob;
            }
            else { return null; }
        }

        // 通过图层名获取目标要素的StandaloneTable
        public static StandaloneTable TargetStandaloneTable(this string layerName)
        {
            Object ob = layerName.GetLayerFromFullName();
            if (ob is StandaloneTable)
            {
                return (StandaloneTable)ob;
            }
            else { return null; }
        }

        // 获取独立图层名
        public static string GetLayerSingleName(this string fullPath)
        {
            string singleName = fullPath;
            // 如果是多层次的，获取最后一层
            if (fullPath.Contains(@"\"))
            {
                singleName = fullPath[(fullPath.LastIndexOf(@"\") + 1)..];
            }
            return singleName;
        }

        // 获取GDB数据库里的所有FeatureClass路径
        public static List<string> GetFeatureClassPathFromGDB(this string gdbPath)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath)));
            // 获取所有要素类
            IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();
            foreach (FeatureClassDefinition featureClass in featureClasses)
            {
                using (FeatureClass fc = gdb.OpenDataset<FeatureClass>(featureClass.GetName()))
                {
                    // 获取要素类路径
                    string fc_path = fc.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    result.Add(fc_path);
                }
            }
            return result;
        }

        // 获取GDB数据库里的所有表格路径
        public static List<string> GetStandaloneTablePathFromGDB(this string gdbPath)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath)));
            // 获取所有独立表
            IReadOnlyList<TableDefinition> tables = gdb.GetDefinitions<TableDefinition>();
            foreach (TableDefinition tableDef in tables)
            {
                using (Table table = gdb.OpenDataset<Table>(tableDef.GetName()))
                {
                    // 获取要素类路径
                    string fc_path = table.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    result.Add(fc_path);
                }
            }
            return result;
        }

        // 获取数据库下的所有要素类和独立表的完整路径
        public static List<string> GetFeatureClassAndTablePath(this string gdbPath)
        {
            // 获取要素类的完整路径
            List<string> fcs = gdbPath.GetFeatureClassPathFromGDB();
            // 获取独立表的完整路径
            List<string> tbs = gdbPath.GetStandaloneTablePathFromGDB();
            // 合并列表
            fcs.AddRange(tbs);
            return fcs;
        }

        // 获取数据库下的所有要素类的名称
        public static List<string> GetFeatureClassNameFromGDB(this string gdb_path)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素类
            IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();
            foreach (FeatureClassDefinition featureClass in featureClasses)
            {
                string fc_name = featureClass.GetName();
                result.Add(fc_name);
            }

            return result;
        }

        // 获取数据库下的所有独立表的名称
        public static List<string> GetTableName(this string gdb_path)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有独立表
            IReadOnlyList<TableDefinition> tables = gdb.GetDefinitions<TableDefinition>();
            foreach (TableDefinition tableDef in tables)
            {
                string tb_name = tableDef.GetName();
                result.Add(tb_name);
            }
            return result;
        }

        // 获取数据库下的所有要素类和独立表的名称
        public static List<string> GetFeatureClassAndTableName(this string gdb_path)
        {
            // 获取要素类的完整路径
            List<string> fcs = gdb_path.GetFeatureClassNameFromGDB();
            // 获取独立表的完整路径
            List<string> tbs = gdb_path.GetTableName();
            // 合并列表
            fcs.AddRange(tbs);
            return fcs;
        }

        // 获取数据库下的所有栅格的完整路径
        public static List<string> GetRasterPath(this string gdb_path)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素类
            IReadOnlyList<RasterDatasetDefinition> rasterDefinitions = gdb.GetDefinitions<RasterDatasetDefinition>();
            foreach (RasterDatasetDefinition rasterDefinition in rasterDefinitions)
            {
                using (RasterDataset rd = gdb.OpenDataset<RasterDataset>(rasterDefinition.GetName()))
                {
                    // 获取要素类路径
                    string rd_path = rd.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    result.Add(rd_path);
                }
            }
            return result;
        }

        // 获取数据库下的所有要素数据集的完整路径
        public static List<string> GetDataBasePath(this string gdb_path)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素类
            IReadOnlyList<FeatureDatasetDefinition> featureDatases = gdb.GetDefinitions<FeatureDatasetDefinition>();
            foreach (FeatureDatasetDefinition featureDatase in featureDatases)
            {
                using (FeatureDataset fd = gdb.OpenDataset<FeatureDataset>(featureDatase.GetName()))
                {
                    // 获取要素类路径
                    string fd_path = fd.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                    result.Add(fd_path);
                }
            }

            return result;
        }

        // 获取数据库下的所有要素数据集的名称
        public static List<string> GetDataBaseName(this string gdb_path)
        {
            List<string> result = new List<string>();
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素类
            IReadOnlyList<FeatureDatasetDefinition> featureDatases = gdb.GetDefinitions<FeatureDatasetDefinition>();
            foreach (FeatureDatasetDefinition featureDatase in featureDatases)
            {
                // 获取要素类路径
                string fd_name = featureDatase.GetName();
                result.Add(fd_name);
            }

            return result;
        }

        // 保留小数位数，非四舍五入
        public static double KeepDigits(this double value, int digit)
        {
            double digit_value = 0;
            string txt_value = value.ToString();
            string txt_digit = txt_value[..(txt_value.IndexOf(".") + 1 + digit)];
            digit_value = double.Parse(txt_digit);

            return digit_value;
        }

        // 填充小数位数，不够就用0代替
        public static string RoundWithFill(this double value, int digit)
        {
            // 要填充的0的位数
            int lengZero = 0;

            // 先Round一下
            double va = Math.Round(value, digit);
            // 转成字符串处理
            string txt_value = va.ToString();
            // 分析一下，如果只有整数位，即没有小数点的情况
            if (!txt_value.Contains("."))
            {
                lengZero = 3;
                txt_value += ".";
            }
            // 有小数位数的时候
            else
            {
                lengZero = digit - (txt_value.Length - (txt_value.IndexOf(".") + 1));
            }

            txt_value += new string('0', lengZero);
            // 返回值
            return txt_value;
        }

        // 提取特定文字【model包括：中文、英文、数字、特殊符号】
        public static string GetWord(this string txt_in, string model = "中文")
        {
            string chinesePattern = "[\u4e00-\u9fa5]"; // 匹配中文字符的正则表达式
            string englishPattern = "[a-zA-Z]"; // 匹配英文字符的正则表达式
            string digitPattern = @"\d"; // 匹配数字的正则表达式
            string specialCharPattern = @"[^a-zA-Z0-9\u4e00-\u9fa5\s]"; // 匹配特殊符号的正则表达式

            string txt = "";

            if (model == "中文")
            {
                Regex chineseRegex = new Regex(chinesePattern);
                txt = ExtractMatches(txt_in, chineseRegex);
            }
            else if (model == "英文")
            {
                Regex englishRegex = new Regex(englishPattern);
                txt = ExtractMatches(txt_in, englishRegex);
            }
            else if (model == "数字")
            {
                Regex digitRegex = new Regex(digitPattern);
                txt = ExtractMatches(txt_in, digitRegex);
            }
            else if (model == "特殊符号")
            {
                Regex specialCharRegex = new Regex(specialCharPattern);
                txt = ExtractMatches(txt_in, specialCharRegex);
            }
            return txt;
        }
        // 正则匹配
        public static string ExtractMatches(string input, Regex regex)
        {
            string result = "";
            MatchCollection matches = regex.Matches(input);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                result += match.Value;
            }
            return result;
        }

        // 获取要素类的坐标系
        public static SpatialReference GetSpatialReference(this string fcPath)
        {
            // 获取FeatureClass
            FeatureClass featureClass = fcPath.TargetFeatureClass();
            SpatialReference sr = featureClass.GetDefinition().GetSpatialReference();

            // 返回坐标系
            return sr;
        }

        // 获取要素类的坐标系
        public static SpatialReference GetSpatialReferenceFromDataBase(this string gdbPath, string dateBaseName)
        {
            // 获取FeatureClass
            using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
            {
                FeatureDataset db = geodatabase.OpenDataset<FeatureDataset>(dateBaseName);
                return db.GetDefinition().GetSpatialReference();
            }
        }

        // 对重复要素进行数字标记
        public static List<string> AddNumbersToDuplicates(this List<string> stringList)
        {
            // 使用Dictionary来跟踪每个字符串的出现次数
            Dictionary<string, int> stringCount = new Dictionary<string, int>();

            // 遍历字符串列表
            for (int i = 0; i < stringList.Count; i++)
            {
                string currentString = stringList[i];

                // 检查字符串是否已经在Dictionary中存在
                if (stringCount.ContainsKey(currentString))
                {
                    // 获取该字符串的出现次数
                    int count = stringCount[currentString];

                    // 在当前字符串后添加数字
                    stringList[i] = $"{currentString}：{count + 1}";

                    // 更新Dictionary中的计数
                    stringCount[currentString] = count + 1;
                }
                else
                {
                    // 如果字符串在Dictionary中不存在，将其添加，并将计数设置为1
                    stringCount.Add(currentString, 1);
                    // 在当前字符串后添加数字
                    stringList[i] = $"{currentString}：{1}";
                }
            }
            // 去除单个要素的数字标记
            foreach (var item in stringCount)
            {
                if (item.Value == 1)
                {
                    for (int i = 0; i < stringList.Count; i++)
                    {
                        if (stringList[i] == item.Key + "：1")
                        {
                            stringList[i] = item.Key;
                        }
                    }
                }
            }

            // 返回字符串列表
            return stringList;
        }

        // 获取地图中的所有要素图层【带图层结构】【字典】
        public static Dictionary<FeatureLayer, string> AllFeatureLayersDic(this Map map)
        {
            Dictionary<FeatureLayer, string> dic = new Dictionary<FeatureLayer, string>();
            List<string> layers = new List<string>();
            List<FeatureLayer> lys = new List<FeatureLayer>();
            // 获取所有要素图层
            List<FeatureLayer> featureLayers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();

            foreach (FeatureLayer featureLayer in featureLayers)
            {
                List<object> list = featureLayer.GetLayerFullName(map, featureLayer.Name);

                layers.Add((string)list[1]);
                lys.Add(featureLayer);
            }
            // 标记重复
            layers.AddNumbersToDuplicates();
            // 加入字典
            for (int i = 0; i < lys.Count; i++)
            {
                dic.Add(lys[i], layers[i]);
            }
            // 返回值
            return dic;
        }

        // 获取地图中的所有独立表【带图层结构】【字典】
        public static Dictionary<StandaloneTable, string> AllStandaloneTablesDic(this Map map)
        {
            Dictionary<StandaloneTable, string> dic = new Dictionary<StandaloneTable, string>();
            List<string> layers = new List<string>();
            List<StandaloneTable> lys = new List<StandaloneTable>();
            // 获取所有要素图层
            List<StandaloneTable> standaloneTables = map.GetStandaloneTablesAsFlattenedList().ToList();
            foreach (StandaloneTable standaloneTable in standaloneTables)
            {
                List<object> list = standaloneTable.GetLayerFullName(map, standaloneTable.Name);

                layers.Add((string)list[1]);
                lys.Add(standaloneTable);
            }
            // 标记重复
            layers.AddNumbersToDuplicates();
            // 加入字典
            for (int i = 0; i < lys.Count; i++)
            {
                dic.Add(lys[i], layers[i]);
            }
            // 返回值
            return dic;
        }

        // 获取地图中的所有要素图层【带图层结构】
        public static List<string> AllFeatureLayers(this Map map)
        {
            List<string> layers = new List<string>();
            // 获取所有要素图层
            List<FeatureLayer> featureLayers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();

            foreach (FeatureLayer featureLayer in featureLayers)
            {
                List<object> list = featureLayer.GetLayerFullName(map, featureLayer.Name);

                layers.Add((string)list[1]);
            }
            return layers.AddNumbersToDuplicates();
        }

        // 获取图层的图斑个数
        public static long GetFeatureCount(this FeatureLayer featureLayer)
        {
            // 遍历面要素类中的所有要素
            using (var cursor = featureLayer.Search())
            {
                int featureCount = 0;
                while (cursor.MoveNext())
                {
                    featureCount++;
                }
                return featureCount;
            }
        }

        // 获取地图中的所有独立表【带图层结构】
        public static List<string> AllStandaloneTables(this Map map)
        {
            List<string> layers = new List<string>();
            // 获取所有要素图层
            List<StandaloneTable> standaloneTables = map.GetStandaloneTablesAsFlattenedList().ToList();
            foreach (StandaloneTable standaloneTable in standaloneTables)
            {
                List<object> list = standaloneTable.GetLayerFullName(map, standaloneTable.Name);

                layers.Add((string)list[1]);
            }
            return layers.AddNumbersToDuplicates();
        }

        // 获取图层的完整名称
        public static List<object> GetLayerFullName(this Object layer, Map map, string lyName)
        {
            List<object> result = new List<object>();
            // 如果是图层
            if (layer is Layer)
            {
                // 如果父对象是Map，直接返回图层名
                if (((Layer)layer).Parent is Map)
                {
                    result.Add(layer);
                    result.Add(lyName);

                    return result;
                }
                else
                {
                    // 如果父对象是不是Map，则找到父对象图层，并循环查找上一个层级
                    Layer paLayer = (Layer)((Layer)layer).Parent;

                    List<object> list = paLayer.GetLayerFullName(map, @$"{paLayer}\{lyName}");

                    return list;
                }
            }
            // 如果是独立表
            else if (layer is StandaloneTable)
            {
                // 如果父对象是Map，直接返回图层名
                if (((StandaloneTable)layer).Parent is Map)
                {
                    result.Add(layer);
                    result.Add(lyName);

                    return result;
                }
                else
                {
                    // 如果父对象是不是Map，则找到父对象图层，并循环查找上一个层级
                    Layer paLayer = (Layer)((StandaloneTable)layer).Parent;

                    List<object> list = paLayer.GetLayerFullName(map, @$"{paLayer}\{lyName}");

                    return list;
                }
            }
            else
            {
                return null;
            }
        }

        // 从图层的完整名称获取图层
        public static Object GetLayerFromFullName(this string layerFullName)
        {
            List<Object> result = new List<object>();

            // 获取当前地图
            Map map = MapView.Active.Map;
            Dictionary<FeatureLayer, string> dicFeatureLayer = map.AllFeatureLayersDic();
            Dictionary<StandaloneTable, string> dicStandaloneTable = map.AllStandaloneTablesDic();
            // 查找要素图层
            foreach (var layer in dicFeatureLayer)
            {
                if (layerFullName == layer.Value)
                {
                    result.Add(layer.Key);
                }
            }

            // 查找独立表
            foreach (var layer in dicStandaloneTable)
            {
                if (layerFullName == layer.Value)
                {
                    result.Add(layer.Key);
                }
            }

            // 返回值
            return result[0];
        }

        // 从ListBox【选中的CheckBox】中获取string列表
        public static List<string> ItemsAsString(this ListBox listBox)
        {
            List<string> strList = new List<string>();

            foreach (CheckBox item in listBox.Items)
            {
                if (item.IsChecked == true)
                {
                    strList.Add(item.Content.ToString());
                }
            }
            return strList;
        }

        // 重排界址点，从西北角开始，并理清顺逆时针
        public static List<List<MapPoint>> ReshotMapPoint(this Polygon polygon)
        {
            List<List<MapPoint>> result = new List<List<MapPoint>>();

            // 获取面要素的所有点（内外环）
            List<List<MapPoint>> mapPoints = polygon.MapPointsFromPolygon();
            // 获取每个环的最西北点
            List<List<double>> NWPoints = polygon.NWPointsFromPolygon();

            // 每个环进行处理
            for (int j = 0; j < mapPoints.Count; j++)
            {
                List<MapPoint> newVertices = new List<MapPoint>();
                List<MapPoint> vertices = mapPoints[j];
                // 获取要素的最西北点坐标
                double XMin = NWPoints[j][0];
                double YMax = NWPoints[j][1];

                // 找出西北点【离西北角（Xmin,Ymax）最近的点】
                int targetIndex = 0;
                double maxDistance = 10000000;
                for (int i = 0; i < vertices.Count; i++)
                {
                    // 计算和西北角的距离
                    double distance = Math.Sqrt(Math.Pow(vertices[i].X - XMin, 2) + Math.Pow(vertices[i].Y - YMax, 2));
                    // 如果小于上一个值，则保存新值，直到找出最近的点
                    if (distance < maxDistance)
                    {
                        targetIndex = i;
                        maxDistance = distance;
                    }
                }

                // 根据最近点重排顺序
                newVertices = vertices.GetRange(targetIndex, vertices.Count - targetIndex);
                vertices.RemoveRange(targetIndex, vertices.Count - targetIndex);
                newVertices.AddRange(vertices);

                // 判断顺逆时针，如果有问题就调整反向
                bool isClockwise = newVertices.IsColckwise();
                if (!isClockwise && j == 0)    // 如果是外环，且逆时针，则调整反向
                {
                    newVertices.ReversePoint();
                }
                if (isClockwise && j > 0)    // 如果是内环，且顺时针，则调整反向
                {
                    newVertices.ReversePoint();
                }

                // 在末尾加起始点
                newVertices.Add(newVertices[0]);

                result.Add(newVertices);
            }
            // 返回值
            return result;
        }

        // 重排界址点，从西北角开始，并理清顺逆时针，并返回面要素
        public static Polygon ReshotMapPointReturnPolygon(this Polygon polygon)
        {
            List<List<MapPoint>> mapPoints = polygon.ReshotMapPoint();

            // 判断是否有内环，如果没有内环
            if (mapPoints.Count == 1)
            {
                Polygon resultPolygon = PolygonBuilderEx.CreatePolygon(mapPoints[0]);
                // 返回值
                return resultPolygon;
            }
            // 如果有内环
            else
            {
                // 生成外环点集
                List<Coordinate2D> outerPts = new List<Coordinate2D>();
                foreach (MapPoint pt in mapPoints[0])
                {
                    outerPts.Add(new Coordinate2D(pt.X, pt.Y));
                }
                // 创建外环面
                PolygonBuilderEx pb = new PolygonBuilderEx(outerPts);

                // 移除外环，剩下内环进行处理
                mapPoints.RemoveAt(0);
                // 收集内环集合
                foreach (List<MapPoint> innerPoints in mapPoints)
                {
                    // 获取内环点集
                    List<Coordinate2D> innerPts = new List<Coordinate2D>();
                    foreach (MapPoint pt in innerPoints)
                    {
                        innerPts.Add(new Coordinate2D(pt.X, pt.Y));
                    }
                    // 将内环点集加入到外环面处理
                    pb.AddPart(innerPts);
                }
                // 返回最终的面
                return pb.ToGeometry();
            }
        }

        // 获取面要素的所有点【isAddFirst为true时，把第一个点加到末尾】
        public static List<List<MapPoint>> MapPointsFromPolygon(this Polygon polygon, bool isAddFirst = false)
        {
            List<List<MapPoint>> mapPoints = new List<List<MapPoint>>();

            // 获取面要素的部件（内外环）
            var parts = polygon.Parts.ToList();
            foreach (ReadOnlySegmentCollection collection in parts)
            {
                List<MapPoint> points = new List<MapPoint>();
                // 每个环进行处理（第一个为外环，其它为内环）
                foreach (Segment segment in collection)
                {
                    MapPoint mapPoint = segment.StartPoint;     // 获取点
                    points.Add(mapPoint);
                }
                // 是否追加第一个点
                if (isAddFirst)
                {
                    points.Add(points[0]);
                }

                mapPoints.Add(points);
            }
            return mapPoints;
        }

        // 获取面要素的所有环的最西北点坐标
        public static List<List<double>> NWPointsFromPolygon(this Polygon polygon)
        {
            List<List<double>> NWPoints = new List<List<double>>();

            // 获取面要素的部件（内外环）
            var parts = polygon.Parts;
            foreach (ReadOnlySegmentCollection collection in parts)
            {
                List<double> point = new List<double>();
                // 初始化西北角的点
                double XMin = 100000000;
                double YMax = 0;

                // 每个环进行处理（第一个为外环，其它为内环）
                foreach (Segment segment in collection)
                {
                    MapPoint mapPoint = segment.StartPoint;     // 获取点
                    if (mapPoint.X < XMin)
                    {
                        XMin = mapPoint.X;
                    }
                    if (mapPoint.Y > YMax)
                    {
                        YMax = mapPoint.Y;
                    }
                }
                point.Add(XMin);
                point.Add(YMax);
                NWPoints.Add(point);
            }
            return NWPoints;
        }

        // 判断点集合形成的面的面积为正值还是负值，用以判断是顺时针还是逆时针
        public static bool IsColckwise(this List<MapPoint> newVertices)
        {
            // 判断界址点是否顺时针排序
            double x1, y1, x2, y2;
            double area = 0;
            for (int i = 0; i < newVertices.Count; i++)
            {
                x1 = newVertices[i].X;
                y1 = newVertices[i].Y;
                if (i == newVertices.Count - 1)
                {
                    x2 = newVertices[0].X;
                    y2 = newVertices[0].Y;
                }
                else
                {
                    x2 = newVertices[i + 1].X;
                    y2 = newVertices[i + 1].Y;
                }
                area += x1 * y2 - x2 * y1;
            }
            if (area > 0)     // 逆时针
            {
                return false;
            }
            else        // 顺时针
            {
                return true;
            }
        }

        // MapPoint第一个点不变，反方向排列
        public static void ReversePoint(this List<MapPoint> newVertices)
        {
            newVertices.Reverse();
            newVertices.Insert(0, newVertices[newVertices.Count - 1]);
            newVertices.RemoveAt(newVertices.Count - 1);
        }

        // 获取子字符串在目标字符中出现的次数
        public static int StringInCount(this string targetString, string subString)
        {
            int count = 0;
            // 获取第一次出现的位置
            int index = targetString.IndexOf(subString);
            while (index != -1)
            {
                count++;
                // 继续从获取到的位置后面开始取值
                index = targetString.IndexOf(subString, index + subString.Length);
            }
            // 返回出现的次数
            return count;
        }

        // 看一下是否当前有选择图斑，如果没有选择图斑，就全图斑处理，如果有选择图斑，就按选择图斑处理
        public static RowCursor GetSelectCursor(this FeatureLayer featureLayer)
        {
            // 看一下是否当前有选择图斑
            RowCursor cursor;
            // 如果没有选择图斑，就全图斑处理
            if (featureLayer.SelectionCount == 0)
            {
                cursor = featureLayer.Search();
            }
            // 如果有选择图斑，就按选择图斑处理
            else
            {
                cursor = featureLayer.GetSelection().Search();
            }
            // 返回
            return cursor;
        }
    }
}
