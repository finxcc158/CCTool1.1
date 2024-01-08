using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.ToolManagers;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCTool.Scripts.Manager
{
    public class VG
    {
        // 获取要处理的入库村庄
        public static List<string> GetVillageNames()
        {
            // 设置空列表
            List<string> village_names = new List<string>();

            // 获取参数
            string folder_path = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string input_folder_path = folder_path + @"\1-输入文件";        // 输入文件包路径

            // 检查是否存在村规Excel文件
            List<string> list_excel = Directory.GetFiles(input_folder_path, "*.xlsx").ToList();

            if (list_excel.Count > 0)
            {
                foreach (var excel in list_excel)
                {
                    // 获取Excel文件名
                    string excel_name = excel[(excel.LastIndexOf(@"\") + 1)..].Replace(".xlsx", "");
                    string in_gdb = input_folder_path + @"\" + excel_name + @".gdb";    // 输入村庄数据库

                    if (Directory.Exists(in_gdb))
                    {
                        village_names.Add(excel_name);
                    }
                }
            }
            return village_names;
        }


        // 生成目标目录
        public static void CreateTarget(string village_name, string database_name = "")
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";    // 输入的村庄数据库
            string gdb_folder = output_folder_path + @"\" + village_name + @"数据库";  // 输出的数据库文件夹名


            // 创建文件夹【2-输出文件】
            if (!Directory.Exists(output_folder_path))
            {
                Directory.CreateDirectory(output_folder_path);
            }

            // 创建文件夹【**村数据库】
            if (!Directory.Exists(gdb_folder))
            {
                Directory.CreateDirectory(gdb_folder);
            }

            // 创建村规要素类
            if (database_name != "")
            {
                string output_gdb_path = gdb_folder + @"\村庄规划数据库.gdb";   // 输出要素类gdb路径
                // 获取坐标系
                string xzPath = in_gdb + @"\现状用地";
                SpatialReference sr = xzPath.GetSpatialReference();

                // 创建要素类数据库
                if (!Directory.Exists(output_gdb_path))
                {
                    Arcpy.CreateFileGDB(gdb_folder, @"村庄规划数据库");
                }

                // 创建要素数据集
                if (!GisTool.IsHaveDataset(output_gdb_path, database_name))
                {
                    Arcpy.CreateFeatureDataset(output_gdb_path, database_name, sr);
                }
            }
            // 创建村规表
            else
            {
                string output_gdb_path = gdb_folder + @"\规划表格.gdb";   // 输出表格gdb路径

                // 创建要素类数据库
                if (!Directory.Exists(output_gdb_path))
                {
                    Arcpy.CreateFileGDB(gdb_folder, @"规划表格");
                }
            }
        }

        // 分解村庄名称
        public static List<string> CutVillageNames(string name_all)
        {
            List<string> village_names = new List<string>();
            List<string> city_word = new List<string>() { "市", "县", "实验区", "区" };
            List<string> town_word = new List<string>() { "镇", "乡", "街道", "片区" };
            string name_city = "";
            string name_town = "";
            string name_village = "";

            // 获取城市名
            foreach (var city in city_word)
            {
                if (name_all.Contains(city))
                {
                    name_city = name_all[..(name_all.IndexOf(city) + city.Length)];
                }
            }
            // 获取镇名
            foreach (var town in town_word)
            {
                if (name_all.Contains(town))
                {
                    name_town = name_all[(name_all.IndexOf(name_city) + name_city.Length)..(name_all.IndexOf(town) + town.Length)];
                }
            }
            // 获取村名
            name_village = name_all[(name_all.IndexOf(name_town) + name_town.Length)..];
            // 输出结果
            village_names.Add(name_city);
            village_names.Add(name_town);
            village_names.Add(name_village);

            return village_names;
        }

        // 生成表格图集目录
        public static void CreateJPGFolder(string village_name)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string gdb_folder = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出的数据库文件夹名

            // 创建文件夹【2-输出文件】
            if (!Directory.Exists(output_folder_path))
            {
                Directory.CreateDirectory(output_folder_path);
            }

            // 创建文件夹【**村表格+图集】
            if (!Directory.Exists(gdb_folder))
            {
                Directory.CreateDirectory(gdb_folder);
            }
        }

        // 生成村级调查区CJDCQ
        public static void CreateCJDCQ(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\JJYZQ\CJDCQ";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\JJYZQ\CJDCQ";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建CJDCQ要素", Brushes.Gray); }

            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);
            // 融合
            Arcpy.Dissolve(in_gdb + @"\现状用地", def_gdb + @"\vg_dissolve", "ZLDWDM;ZLDWMC");
            // 追加
            Arcpy.Append(def_gdb + @"\vg_dissolve", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];
            // 获取excel单元格值的字典和列表
            Dictionary<string, string> dict = OfficeTool.GetDictFromExcelAll(excel_path + @"\sheet1$");
            // 获取村庄类型
            string village_type = UpdataVillageType(dict["村庄类型"]);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("CJDCQ");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "1000600400";
                        row["JSMJ"] = Math.Round(double.Parse(row["SHAPE_Area"].ToString()), 2);
                        row["CZLX"] = village_type;

                        // 保存
                        row.Store();
                    }
                }
            }

            // 删除中间数据
            Arcpy.Delect(def_gdb + @"\vg_dissolve");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 更新村庄类型【模糊取值，容许填写错误】
        public static string UpdataVillageType(string village_type)
        {
            List<string> list1 = new List<string>() { "集聚", "提升", "中心", "转型", "融合", "城郊", "保护", "开发", "特色", "搬迁", "撤并", "衰退" };
            List<string> list2 = new List<string>() { "集聚提升中心", "转型融合城郊", "保护开发特色", "搬迁撤并衰退" };
            string v_type = "";
            for (int i = 0; i < list1.Count; i++)
            {
                if (village_type.Contains(list1[i]))
                {
                    v_type = list2[i / 3];
                }
            }
            return v_type;
        }

        // 生成村级调查区界线CJDCQJX
        public static void CreateCJDCQJX(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\JJYZQ\CJDCQJX";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\JJYZQ\CJDCQJX";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建CJDCQJX要素", Brushes.Gray); }

            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);
            // 融合
            Arcpy.Dissolve(in_gdb + @"\现状用地", def_gdb + @"\vg_dissolve");
            // 面转线
            Arcpy.PolygonToLine(def_gdb + @"\vg_dissolve", def_gdb + @"\vg_dissolve_line");
            // 追加
            Arcpy.Append(def_gdb + @"\vg_dissolve_line", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("CJDCQJX");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "1000600500";
                        row["JXLX"] = "670500";
                        row["JXXZ"] = "600001";

                        // 保存
                        row.Store();
                    }
                }
            }

            // 删除中间数据
            Arcpy.Delect(def_gdb + @"\vg_dissolve");
            Arcpy.Delect(def_gdb + @"\vg_dissolve_line");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成基期地类图斑JQDLTB
        public static void CreateJQDLTB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\JQXZ\JQDLTB";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\JQXZ\JQDLTB";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建JQDLTB要素", Brushes.Gray); }

            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            // 追加
            Arcpy.Append(in_gdb + @"\现状用地", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("JQDLTB");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003010100";

                        // 保存
                        row.Store();
                    }
                }
            }

            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成现状公服设施点GGJCSSD
        public static void CreateGGJCSSD(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\JQXZ\GGJCSSD";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\JQXZ\GGJCSSD";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建GGJCSSD要素", Brushes.Gray); }

            // 复制对照表
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.公服表.xlsx", def_folder + @"\公服表.xlsx");
            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            // 追加
            Arcpy.Append(in_gdb + @"\现状公服", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 公服编码映射
            GisTool.AttributeMapper(output_fc_path, "SSLXMC", "SSLXDM", def_folder + @"\公服表.xlsx\sheet1$");

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("GGJCSSD");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003010800";

                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除中间数据
            File.Delete(def_folder + @"\公服表.xlsx");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成规划地类图斑GHDLTB
        public static void CreateGHDLTB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\MBNGH\GHDLTB";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\MBNGH\GHDLTB";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建GHDLTB要素", Brushes.Gray); }

            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            // 追加
            Arcpy.Append(in_gdb + @"\规划用地", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("GHDLTB");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        var bjlx = row["SSBJLX"];
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003020210";
                        row["GHDLMJ"] = Math.Round(double.Parse(row["SHAPE_Area"].ToString()), 2);
                        row["TBBH"] = obj;
                        if (bjlx is not null)
                        {
                            if (bjlx.ToString() == "T") { row["SSBJLX"] = ""; }
                        }

                        // 保存
                        row.Store();
                    }
                }
            }
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成现状公服设施点GHGGJCSSD
        public static void CreateGHGGJCSSD(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\MBNGH\GHGGJCSSD";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\MBNGH\GHGGJCSSD";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建GHGGJCSSD要素", Brushes.Gray); }

            // 复制对照表
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.公服表.xlsx", def_folder + @"\公服表.xlsx");
            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            // 追加
            Arcpy.Append(in_gdb + @"\规划公服", output_fc_path);
            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 公服编码映射
            GisTool.AttributeMapper(output_fc_path, "GHSSLXMC", "GHSSLXDM", def_folder + @"\公服表.xlsx\sheet1$");

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("GHGGJCSSD");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003020810";

                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除中间数据
            File.Delete(def_folder + @"\公服表.xlsx");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成管控边界GKBJ
        public static void CreateGKBJ(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\MBNGH\GKBJ";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\MBNGH\GKBJ";   // 输出要素的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建GKBJ要素", Brushes.Gray); }

            // 复制Excel表
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_管控分区.xlsx", def_folder + @"\用地用海代码_管控分区.xlsx");
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.管控边界表.xlsx", def_folder + @"\管控边界表.xlsx");
            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            Arcpy.Dissolve(in_gdb + @"\现状用地", def_gdb + @"\tem_bj");     // 生成边界
            Arcpy.CopyFeatures(in_gdb + @"\规划用地", def_gdb + @"\tem_ghyd");      // 复制规划用地
            //  裁剪生态保护红线
            if (GisTool.IsHaveFeaturClass(in_gdb, "生态保护红线"))
            {
                Arcpy.Clip(in_gdb + @"\生态保护红线", def_gdb + @"\tem_bj", def_gdb + @"\tem_sthx");
            }
            else
            {
                // 没有的话就随便放一个空要素
                Arcpy.Clip(empty_fc_path, def_gdb + @"\tem_bj", def_gdb + @"\tem_sthx");
            }

            //   裁剪永久基本农田
            if (GisTool.IsHaveFeaturClass(in_gdb, "永久基本农田"))
            {
                Arcpy.Clip(in_gdb + @"\永久基本农田", def_gdb + @"\tem_bj", def_gdb + @"\tem_jbnt");
            }
            else
            {
                // 没有的话就随便放一个空要素
                Arcpy.Clip(empty_fc_path, def_gdb + @"\tem_bj", def_gdb + @"\tem_jbnt");
            }

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "更新基本农田和生态红线", Brushes.Gray); }

            // 添加GKBJLXMC字段
            Arcpy.AddField(def_gdb + @"\tem_ghyd", "GKBJLXMC", "TEXT");
            Arcpy.AddField(def_gdb + @"\tem_sthx", "GKBJLXMC", "TEXT");
            Arcpy.AddField(def_gdb + @"\tem_jbnt", "GKBJLXMC", "TEXT");

            // 赋值GKBJLXMC字段
            Arcpy.CalculateField(def_gdb + @"\tem_sthx", "GKBJLXMC", "\"生态保护红线\"");
            Arcpy.CalculateField(def_gdb + @"\tem_jbnt", "GKBJLXMC", "\"永久基本农田\"");

            // 属性映射_用地用海代码_管控分区
            GisTool.AttributeMapper(def_gdb + @"\tem_ghyd", "GHDLBM", "GKBJLXMC", def_folder + @"\用地用海代码_管控分区.xlsx\sheet1$");

            // 更新基本农田和生态红线
            Arcpy.Update(def_gdb + @"\tem_ghyd", def_gdb + @"\tem_sthx", def_gdb + @"\tem_gk1");
            Arcpy.Update(def_gdb + @"\tem_gk1", def_gdb + @"\tem_jbnt", def_gdb + @"\tem_gk2");

            // 计算边界类型【城镇空间、弹性发展区】
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(def_gdb)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("tem_gk2");
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    var bjlx = row["SSBJLX"];
                    if (bjlx is not null)
                    {
                        if (bjlx.ToString() == "Z" || bjlx.ToString() == "z") { row["GKBJLXMC"] = "城镇空间"; }
                        else if (bjlx.ToString() == "T" || bjlx.ToString() == "t") { row["GKBJLXMC"] = "村庄弹性发展区"; }
                    }
                    row.Store();
                }
            }
            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "碎斑融合", Brushes.Gray); }

            // 融合
            Arcpy.Dissolve(def_gdb + @"\tem_gk2", def_gdb + @"\tem_gk3", "GKBJLXMC");
            // 转成单部件
            Arcpy.MultipartToSinglepart(def_gdb + @"\tem_gk3", def_gdb + @"\tem_gk4");
            // 16平方以下的融合（非永农、生态）
            GisTool.FeatureClassEliminate(def_gdb + @"\tem_gk4", def_gdb + @"\tem_gk5", "SHAPE_Area < 16", "GKBJLXMC IN ('生态保护红线','永久基本农田')");
            // 1平方以下的融合
            GisTool.FeatureClassEliminate(def_gdb + @"\tem_gk5", def_gdb + @"\tem_gk6", "SHAPE_Area < 1");

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "检查{80%-100%}", Brushes.Gray); }

            // 集中区小地块就近融合（减少集中区面积，以满足质检要求）【融合面积50,100,200,500】
            DissolveGKBJ(in_gdb + @"\规划用地", "GHDLBM", def_gdb + @"\tem_gk6");

            // 追加
            Arcpy.Append(def_gdb + @"\tem_gk6", output_fc_path);

            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

            // 属性映射——'GKBJLXMC'--> 'GKBJLXDM'
            GisTool.AttributeMapper(output_fc_path, "GKBJLXMC", "GKBJLXDM", def_folder + @"\管控边界表.xlsx\sheet1$");

            // 打开GDB数据库
            using Geodatabase gdb_2 = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse_2 = gdb_2.OpenDataset<FeatureClass>("GKBJ");
            // 逐行游标
            using (RowCursor rowCursor = featureClasse_2.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003020710";
                        row["MJ"] = Math.Round(double.Parse(row["SHAPE_Area"].ToString()), 2);
                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除中间要素
            List<string> list_delete = new List<string>() { "tem_gk1", "tem_gk2", "tem_gk3", "tem_gk4", "tem_gk5", "tem_gk6", "tem_gk7", "tem_bj", "tem_ghyd", "cal_table", "cal_table_2", "cal_table_3", "cal_tb_fin", "tem_sthx", "tem_jbnt" };
            foreach (var item in list_delete)
            {
                Arcpy.Delect(def_gdb + @"\" + item);
            }
            // 删除中间数据
            File.Delete(def_folder + @"\用地用海代码_管控分区.xlsx");
            File.Delete(def_folder + @"\管控边界表.xlsx");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成管控边界GKBJ至指定位置
        public static void CreateGKBJtoPath(string village_name, string outputPath)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\MBNGH\GKBJ";   // 空库要素的位置
            string output_fc_path = outputPath;   // 输出要素的位置
            string output_gdb_path = outputPath[..(outputPath.LastIndexOf(@".gdb") + 4)];  // 输出GDB的位置
            string output_fc = outputPath[(outputPath.LastIndexOf(@"\") + 1)..];  // 输出fc的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            // 复制Excel表
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_管控分区.xlsx", def_folder + @"\用地用海代码_管控分区.xlsx");
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.管控边界表.xlsx", def_folder + @"\管控边界表.xlsx");
            // 复制空库要素（定义正确的坐标系）
            Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

            Arcpy.Dissolve(in_gdb + @"\现状用地", def_gdb + @"\tem_bj");     // 生成边界
            Arcpy.CopyFeatures(in_gdb + @"\规划用地", def_gdb + @"\tem_ghyd");      // 复制规划用地

            //  裁剪生态保护红线
            if (GisTool.IsHaveFeaturClass(in_gdb, "生态保护红线"))
            {
                Arcpy.Clip(in_gdb + @"\生态保护红线", def_gdb + @"\tem_bj", def_gdb + @"\tem_sthx");
            }
            else
            {
                // 没有的话就随便放一个空要素
                Arcpy.Clip(empty_fc_path, def_gdb + @"\tem_bj", def_gdb + @"\tem_sthx");
            }

            //   裁剪永久基本农田
            if (GisTool.IsHaveFeaturClass(in_gdb, "永久基本农田"))
            {
                Arcpy.Clip(in_gdb + @"\永久基本农田", def_gdb + @"\tem_bj", def_gdb + @"\tem_jbnt");
            }
            else
            {
                // 没有的话就随便放一个空要素
                Arcpy.Clip(empty_fc_path, def_gdb + @"\tem_bj", def_gdb + @"\tem_jbnt");
            }

            // 添加GKBJLXMC字段
            Arcpy.AddField(def_gdb + @"\tem_ghyd", "GKBJLXMC", "TEXT");
            Arcpy.AddField(def_gdb + @"\tem_sthx", "GKBJLXMC", "TEXT");
            Arcpy.AddField(def_gdb + @"\tem_jbnt", "GKBJLXMC", "TEXT");

            // 赋值GKBJLXMC字段
            Arcpy.CalculateField(def_gdb + @"\tem_sthx", "GKBJLXMC", "\"生态保护红线\"");
            Arcpy.CalculateField(def_gdb + @"\tem_jbnt", "GKBJLXMC", "\"永久基本农田\"");
            // 删除无用且干扰字段
            Arcpy.DeleteField(def_gdb + @"\tem_jbnt", "XZQDM");

            // 属性映射_用地用海代码_管控分区
            GisTool.AttributeMapper(def_gdb + @"\tem_ghyd", "GHDLBM", "GKBJLXMC", def_folder + @"\用地用海代码_管控分区.xlsx\sheet1$");

            // 更新基本农田和生态红线
            Arcpy.Update(def_gdb + @"\tem_ghyd", def_gdb + @"\tem_sthx", def_gdb + @"\tem_gk1");
            Arcpy.Update(def_gdb + @"\tem_gk1", def_gdb + @"\tem_jbnt", def_gdb + @"\tem_gk2");

            // 计算边界类型【城镇空间、弹性发展区】
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(def_gdb)));
            // 获取要素类
            using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("tem_gk2");
            using (RowCursor rowCursor = featureClasse.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    var bjlx = row["SSBJLX"];
                    if (bjlx is not null)
                    {
                        if (bjlx.ToString() == "Z" || bjlx.ToString() == "z") { row["GKBJLXMC"] = "城镇空间"; }
                        else if (bjlx.ToString() == "T" || bjlx.ToString() == "t") { row["GKBJLXMC"] = "村庄弹性发展区"; }
                    }
                    row.Store();
                }
            }

            // 融合
            Arcpy.Dissolve(def_gdb + @"\tem_gk2", def_gdb + @"\tem_gk3", "GKBJLXMC");
            // 转成单部件
            Arcpy.MultipartToSinglepart(def_gdb + @"\tem_gk3", def_gdb + @"\tem_gk4");
            // 16平方以下的融合（非永农、生态）
            GisTool.FeatureClassEliminate(def_gdb + @"\tem_gk4", def_gdb + @"\tem_gk5", "SHAPE_Area < 16", "GKBJLXMC IN ('生态保护红线','永久基本农田')");
            // 1平方以下的融合
            GisTool.FeatureClassEliminate(def_gdb + @"\tem_gk5", def_gdb + @"\tem_gk6", "SHAPE_Area < 1", "");

            // 集中区小地块就近融合（减少集中区面积，以满足质检要求）【融合面积50,100,200,500】
            DissolveGKBJ(in_gdb + @"\规划用地", "GHDLBM", def_gdb + @"\tem_gk6");

            // 追加
            Arcpy.Append(def_gdb + @"\tem_gk6", output_fc_path);

            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            // 属性映射——'GKBJLXMC'--> 'GKBJLXDM'
            GisTool.AttributeMapper(output_fc_path, "GKBJLXMC", "GKBJLXDM", def_folder + @"\管控边界表.xlsx\sheet1$");

            // 打开GDB数据库
            using Geodatabase gdb_2 = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using FeatureClass featureClasse_2 = gdb_2.OpenDataset<FeatureClass>(output_fc);
            // 逐行游标
            using (RowCursor rowCursor = featureClasse_2.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var obj = row["OBJECTID"].ToString();
                        // 赋值
                        row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                        row["YSDM"] = "2003020710";
                        row["MJ"] = Math.Round(double.Parse(row["SHAPE_Area"].ToString()), 2);
                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除中间要素
            List<string> list_delete = new List<string>() { "tem_gk1", "tem_gk2", "tem_gk3", "tem_gk4", "tem_gk5", "tem_gk6", "tem_gk7", "tem_bj", "tem_ghyd", "cal_table", "cal_table_2", "cal_table_3", "cal_tb_fin", "tem_sthx", "tem_jbnt" };
            foreach (var item in list_delete)
            {
                Arcpy.Delect(def_gdb + @"\" + item);
            }
            // 删除中间数据
            File.Delete(def_folder + @"\用地用海代码_管控分区.xlsx");
            File.Delete(def_folder + @"\管控边界表.xlsx");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 集中区小地块就近融合（减少集中区面积，以满足质检要求）【融合面积50,100,200,500】
        public static void DissolveGKBJ(string yd, string bm_field, string dis_fc)
        {
            string gdb_path = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            CalculateYD(yd, bm_field, gdb_path + @"\cal_table", true);         // 规划用地分类汇总
            // 表筛选
            Arcpy.TableSelect(gdb_path + @"\cal_table", gdb_path + @"\cal_table_2", "YDLXMC IN ('居住用地', '公共管理与公共服务用地', '商业服务业用地', '工业用地', '仓储用地', '村庄内部道路用地', '交通场站用地', '其他交通设施用地', '公用设施用地', '绿地与开敞空间用地', '留白用地', '空闲地')");
            // 统计
            Arcpy.Statistics(gdb_path + @"\cal_table_2", gdb_path + @"\cal_table_3", "SUM_SHAPE_Area SUM", "");
            // 获取村庄建设用地面积
            string area = GisTool.GetCellFromPath(gdb_path + @"\cal_table_3", "SUM_SUM_Shape_Area", "OBJECTID = 1");
            double mark_area = double.Parse(area);
            // 递归融合
            ReDissolve(dis_fc, mark_area, 50);
        }

        // 递归融合
        public static void ReDissolve(string fc_path, double mark_area, double increase_area)
        {
            string gdb_path = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            // 汇总
            Arcpy.Statistics(fc_path, gdb_path + @"\cal_tb_fin", "Shape_Area SUM", "GKBJLXMC");
            // 获取村庄集中建设区面积
            string area = GisTool.GetCellFromPath(gdb_path + @"\cal_tb_fin", "SUM_Shape_Area", "GKBJLXMC = '村庄集中建设区'");
            double area_double = double.Parse(area);


            if (area_double > mark_area)
            {
                string sql_select = "GKBJLXMC = '村庄集中建设区' AND Shape_Area<" + increase_area.ToString();
                string sql_ex = "GKBJLXMC IN ('生态保护红线','永久基本农田')";
                GisTool.FeatureClassEliminate(fc_path, gdb_path + @"\tem_gk7", sql_select, sql_ex);
                Arcpy.CopyFeatures(gdb_path + @"\tem_gk7", fc_path);
                ReDissolve(fc_path, mark_area, increase_area + 50);
            }
        }

        // 现状、规划用地分类汇总
        public static void CalculateYD(string yd, string bm_field, string out_table, bool is_clear = false)
        {
            string folder_path = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            // 复制Excel表
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_村庄功能.xlsx", folder_path + @"\用地用海代码_村庄功能.xlsx");
            // 添加统计分类字段
            Arcpy.AddField(yd, "用地分类", "TEXT");

            // 获取Table
            using Table table = yd.TargetTable();
            // 逐行游标
            using RowCursor rowCursor = table.Search();
            // 统计现状用地的情况
            if (bm_field == "JQDLBM")
            {
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    var re = row["CZCSXM"];
                    if (re != null)
                    {
                        string tt = re.ToString();
                        if (tt == "202" || tt == "202A" || tt == "201" || tt == "201A")
                        {
                            row["用地分类"] = "城镇用地";
                        }
                        else
                        {
                            row["用地分类"] = row[bm_field];
                        }
                    }
                    else
                    {
                        row["用地分类"] = row[bm_field];
                    }
                    row.Store();
                }
            }
            // 统计规划用地的情况
            else if (bm_field == "GHDLBM")
            {
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    var re = row["SSBJLX"];
                    if (re != null)
                    {
                        string tt = re.ToString();
                        if (tt == "Z" || tt == "z")
                        {
                            row["用地分类"] = "城镇用地";
                        }
                        else
                        {
                            row["用地分类"] = row[bm_field];
                        }
                    }
                    else
                    {
                        row["用地分类"] = row[bm_field];
                    }
                    row.Store();
                }
            }

            // 添加村庄功能字段
            Arcpy.AddField(yd, "YDLXMC", "TEXT");
            // 村庄功能_属性映射
            GisTool.AttributeMapper(yd, "用地分类", "YDLXMC", folder_path + @"\用地用海代码_村庄功能.xlsx\sheet1$");
            // 统计
            Arcpy.Statistics(yd, out_table, "shape_area", "YDLXMC");
            // 去除括号中的内容
            if (is_clear)
            {
                // 获取表
                using Table tb = out_table.TargetTable();
                // 逐行游标
                using RowCursor rowCursor_2 = tb.Search();
                while (rowCursor_2.MoveNext())
                {
                    using Row row = rowCursor_2.Current;
                    var re = row["YDLXMC"];
                    if (re != null)
                    {
                        string tt = re.ToString();
                        int index = tt.IndexOf(@"（");
                        if (index != -1)
                        {
                            row["YDLXMC"] = tt[..index];
                        }
                    }
                    row.Store();
                }

                // 删除中间字段
                Arcpy.DeleteField(yd, "用地分类;YDLXMC");
                // 删除中间数据
                File.Delete(folder_path + @"\用地用海代码_村庄功能.xlsx");
            }
        }
        // 生成历史文化保护区LSWHBHQ
        public static void CreateLSWHBHQ(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb";  // 输出GDB的位置

            string empty_fc_path = def_folder + @"\村规空库\村庄规划数据库.gdb\MBNGH\LSWHBHQ";   // 空库要素的位置
            string output_fc_path = output_folder_path + @"\" + village_name + @"数据库\村庄规划数据库.gdb\MBNGH\LSWHBHQ";   // 输出要素的位置

            if (isAddMessage) { pw.AddMessage("创建LSWHBHQ要素", Brushes.Gray); }

            if (!GisTool.IsHaveFeaturClass(in_gdb, "文保"))
            {
                if (isAddMessage) { pw.AddMessage("\r" + "没有文保要素", Brushes.Red); }
            }

            else
            {
                // 复制空库
                if (!Directory.Exists(def_folder + @"\村规空库"))
                {
                    BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
                }
                // 复制空库要素
                Arcpy.CopyFeatures(empty_fc_path, output_fc_path);

                // 追加
                Arcpy.Append(in_gdb + @"\文保", output_fc_path);
                // 获取村庄行政区代码
                string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

                if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算字段值", Brushes.Gray); }

                // 打开GDB数据库
                using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
                // 获取要素类
                using FeatureClass featureClasse = gdb.OpenDataset<FeatureClass>("LSWHBHQ");
                // 逐行游标
                using (RowCursor rowCursor = featureClasse.Search(null, false))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            var obj = row["OBJECTID"].ToString();
                            var LSWHLX = row["LSWHLX"];
                            var JBDM = row["JBDM"];
                            // 赋值
                            row["BSM"] = village_id + new string('0', 8 - obj.Length) + obj;
                            row["YSDM"] = "2003020910";

                            if (LSWHLX is not null)
                            {
                                if (LSWHLX.ToString() == "历史文化核心保护范围") { row["LSWHLXDM"] = "01"; }
                                else if (LSWHLX.ToString() == "历史文化建设控制地带范围") { row["LSWHLXDM"] = "02"; }
                                else if (LSWHLX.ToString() == "其他") { row["LSWHLXDM"] = "03"; }
                            }

                            if (JBDM is not null)
                            {
                                if (JBDM.ToString().Contains("国家")) { row["JBDM"] = "10"; }
                                else if (JBDM.ToString().Contains('省')) { row["JBDM"] = "20"; }
                                else if (JBDM.ToString().Contains('市')) { row["JBDM"] = "30"; }
                                else if (JBDM.ToString().Contains('县')) { row["JBDM"] = "40"; }
                                else if (JBDM.ToString().Contains("其他")) { row["JBDM"] = "50"; }
                            }
                            // 保存
                            row.Store();
                        }
                    }
                }
                Directory.Delete(def_folder + @"\村规空库", true);
            }
        }

        // 生成规划指标表
        public static void CreateGHZBB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\规划表格.gdb";  // 输出GDB的位置

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地
            string yd_gh = in_gdb + @"\规划用地";    // 输入规划用地

            string empty_tb_path = def_folder + @"\村规空库\规划表格.gdb\GHZBB";   // 空库表的位置
            string output_tb_path = output_folder_path + @"\" + village_name + @"数据库\规划表格.gdb\GHZBB";   // 输出表的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建GHZBB表", Brushes.Gray); }

            // 复制空表
            Arcpy.CopyRows(empty_tb_path, output_tb_path);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "获取用地指标", Brushes.Gray); }

            // 获取KJGNJGTZB
            string tzb_path = "";
            // 现状、规划用地汇总
            CalculateYD(yd_xz, "JQDLBM", def_gdb + @"\tb_xz", true);
            CalculateYD(yd_gh, "GHDLBM", def_gdb + @"\tb_gh", true);
            // 修改汇总字段
            Arcpy.AlterField(def_gdb + @"\tb_xz", "SUM_shape_area", "现状面积", "现状面积");
            Arcpy.AlterField(def_gdb + @"\tb_gh", "SUM_shape_area", "规划面积", "规划面积");
            // 连接字段
            string tem_KJGNJGTZB = def_gdb + @"\KJGNJGTZB";
            Arcpy.CopyRows(def_folder + @"\村规空库\规划表格.gdb\KJGNJGTZB", tem_KJGNJGTZB);
            Arcpy.JoinField(tem_KJGNJGTZB, "YDLXMC", def_gdb + @"\tb_xz", "YDLXMC", new List<string>() { "现状面积" });
            Arcpy.JoinField(tem_KJGNJGTZB, "YDLXMC", def_gdb + @"\tb_gh", "YDLXMC", new List<string>() { "规划面积" });

            /// 更新计算字段值
            // 打开GDB数据库
            using Geodatabase gdb_tem = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(def_gdb)));
            // 获取要素类
            using Table table_tem = gdb_tem.OpenDataset<Table>("KJGNJGTZB");
            // 逐行游标
            using (RowCursor rowCursor = table_tem.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var xzmj = row["现状面积"];
                        var ghmj = row["规划面积"];

                        if (xzmj is not null) { row["JQXZNMJ"] = Math.Round(double.Parse(xzmj.ToString()) / 10000, 2); }          // 现状面积
                        else { row["JQXZNMJ"] = 0; }

                        if (ghmj is not null) { row["GHMBNMJ"] = Math.Round(double.Parse(ghmj.ToString()) / 10000, 2); }         // 规划面积
                        else { row["GHMBNMJ"] = 0; }

                        double xz_updata = double.Parse(row["JQXZNMJ"].ToString());      // 更新后现状面积
                        double gh_updata = double.Parse(row["GHMBNMJ"].ToString());      // 更新后规划面积
                        // 保存
                        row.Store();
                    }
                }
            }
            Arcpy.Delect(def_gdb + @"\tb_xz");
            Arcpy.Delect(def_gdb + @"\tb_gh");

            tzb_path = tem_KJGNJGTZB;

            // 获取指标
            double xz_gd = double.Parse(GisTool.GetCellFromPath(tzb_path, "JQXZNMJ", "YDLXMC = '耕地'"));
            double xz_ld = double.Parse(GisTool.GetCellFromPath(tzb_path, "JQXZNMJ", "YDLXMC = '林地'"));
            double xz_sd = double.Parse(GisTool.GetCellFromPath(tzb_path, "JQXZNMJ", "YDLXMC = '湿地'"));
            double xz_gyss = double.Parse(GisTool.GetCellFromPath(tzb_path, "JQXZNMJ", "YDLXMC = '公用设施用地'"));
            double xz_ggss = double.Parse(GisTool.GetCellFromPath(tzb_path, "JQXZNMJ", "YDLXMC = '公共管理与公共服务用地'"));
            double gh_gd = double.Parse(GisTool.GetCellFromPath(tzb_path, "GHMBNMJ", "YDLXMC = '耕地'"));
            double gh_ld = double.Parse(GisTool.GetCellFromPath(tzb_path, "GHMBNMJ", "YDLXMC = '林地'"));
            double gh_sd = double.Parse(GisTool.GetCellFromPath(tzb_path, "GHMBNMJ", "YDLXMC = '湿地'"));
            double gh_gyss = double.Parse(GisTool.GetCellFromPath(tzb_path, "GHMBNMJ", "YDLXMC = '公用设施用地'"));
            double gh_ggss = double.Parse(GisTool.GetCellFromPath(tzb_path, "GHMBNMJ", "YDLXMC = '公共管理与公共服务用地'"));

            // 统计【村庄建设用地,村域城乡建设用地,村域建设用地】
            string sql_cz = "YDLXMC IN ('居住用地', '公共管理与公共服务用地', '商业服务业用地', '工业用地', '仓储用地', '村庄内部道路用地', '交通场站用地', '其他交通设施用地', '公用设施用地', '绿地与开敞空间用地', '留白用地', '空闲地')";
            string sql_cs = "YDLXMC IN ('居住用地', '公共管理与公共服务用地', '商业服务业用地', '工业用地', '仓储用地', '村庄内部道路用地', '交通场站用地', '其他交通设施用地', '公用设施用地', '绿地与开敞空间用地', '留白用地', '空闲地', '城镇用地')";
            string sql_cy = "YDLXMC IN ('居住用地', '公共管理与公共服务用地', '商业服务业用地', '工业用地', '仓储用地', '村庄内部道路用地', '交通场站用地', '其他交通设施用地', '公用设施用地', '绿地与开敞空间用地', '留白用地', '空闲地', '城镇用地', '区域基础设施用地', '其他建设用地')";
            // 筛选
            Arcpy.TableSelect(tzb_path, def_gdb + @"\fc_cz", sql_cz);
            Arcpy.TableSelect(tzb_path, def_gdb + @"\fc_cs", sql_cs);
            Arcpy.TableSelect(tzb_path, def_gdb + @"\fc_cy", sql_cy);
            // 统计
            Arcpy.Statistics(def_gdb + @"\fc_cz", def_gdb + @"\tb_cz", "JQXZNMJ SUM;GHMBNMJ SUM", "");
            Arcpy.Statistics(def_gdb + @"\fc_cs", def_gdb + @"\tb_cs", "JQXZNMJ SUM;GHMBNMJ SUM", "");
            Arcpy.Statistics(def_gdb + @"\fc_cy", def_gdb + @"\tb_cy", "JQXZNMJ SUM;GHMBNMJ SUM", "");
            // 提取
            double xz_cz = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cz", "SUM_JQXZNMJ", ""));
            double xz_cs = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cs", "SUM_JQXZNMJ", ""));
            double xz_cy = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cy", "SUM_JQXZNMJ", ""));
            double gh_cz = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cz", "SUM_GHMBNMJ", ""));
            double gh_cs = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cs", "SUM_GHMBNMJ", ""));
            double gh_cy = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_cy", "SUM_GHMBNMJ", ""));

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "获取生态红线和基本农田面积", Brushes.Gray); }

            // 提取生态红线和基本农田的面积
            Arcpy.Dissolve(in_gdb + @"\现状用地", def_gdb + @"\tem_bj");     // 生成边界
            double sthx = 0;
            double jbnt = 0;
            // 计算生态保护红线
            if (GisTool.IsHaveFeaturClass(in_gdb, "生态保护红线"))
            {
                Arcpy.Clip(in_gdb + @"\生态保护红线", def_gdb + @"\tem_bj", def_gdb + @"\tem_sthx");
                Arcpy.Statistics(def_gdb + @"\tem_sthx", def_gdb + @"\tb_sthx", "shape_area SUM", "");
                string sthx_va = GisTool.GetCellFromPath(def_gdb + @"\tb_sthx", "SUM_shape_area", "");
                if (sthx_va != "") { sthx = Math.Round(double.Parse(sthx_va) / 10000, 2); }
            }
            // 计算永久基本农田
            if (GisTool.IsHaveFeaturClass(in_gdb, "永久基本农田"))
            {
                Arcpy.Clip(in_gdb + @"\永久基本农田", def_gdb + @"\tem_bj", def_gdb + @"\tem_jbnt");
                Arcpy.Statistics(def_gdb + @"\tem_jbnt", def_gdb + @"\tb_jbnt", "shape_area SUM", "");
                string jbnt_va = GisTool.GetCellFromPath(def_gdb + @"\tb_jbnt", "SUM_shape_area", "");
                if (jbnt_va != "") { jbnt = Math.Round(double.Parse(jbnt_va) / 10000, 2); }
            }

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "获取Excel表格指标", Brushes.Gray); }

            // 获取Excel表格指标
            string village_id = GisTool.GetCellFromPath(in_gdb + @"\现状用地", "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];   //村庄行政区代码

            Arcpy.CopyRows(excel_path + @"\sheet1$", def_gdb + @"\tem_exl");

            double xz_rk = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tem_exl", "值", "指标 = '现状人口'"));    // 现状人口
            double gh_rk = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tem_exl", "值", "指标 = '规划人口'"));    // 规划人口
            double xz_wh = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tem_exl", "值", "指标 = '现状自然和文化遗产'"));    // 现状自然和文化遗产
            double gh_wh = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tem_exl", "值", "指标 = '规划自然和文化遗产'"));    // 规划自然和文化遗产

            // 补充：重新计算【公共管理与公共服务用地规模（08+0704）】
            Arcpy.Statistics(in_gdb + @"\现状用地", def_gdb + @"\sta_xz", "shape_area SUM", "JQDLBM");
            Arcpy.Statistics(in_gdb + @"\规划用地", def_gdb + @"\sta_gh", "shape_area SUM", "GHDLBM");
            double xz_0704 = 0;
            double gh_0704 = 0;
            string xz_0704_va = GisTool.GetCellFromPath(def_gdb + @"\sta_xz", "SUM_shape_area", "JQDLBM = '0704'");
            string gh_0704_va = GisTool.GetCellFromPath(def_gdb + @"\sta_gh", "SUM_shape_area", "GHDLBM = '0704'");

            if (xz_0704_va != "")
            {
                xz_0704 = Math.Round((double.Parse(xz_0704_va) + xz_ggss) / 10000, 2);
            }
            else
            {
                xz_0704 = Math.Round(xz_ggss / 10000, 2);
            }
            if (gh_0704_va != "")
            {
                gh_0704 = Math.Round((double.Parse(gh_0704_va) + gh_ggss) / 10000, 2);
            }
            else
            {
                gh_0704 = Math.Round(gh_ggss / 10000, 2);
            }

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "字段赋值", Brushes.Gray); }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using Table table = gdb.OpenDataset<Table>("GHZBB");
            // 逐行游标
            using (RowCursor rowCursor = table.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        // 赋值
                        row["XZQDM"] = village_id;
                        row["XZQMC"] = village_name;

                        if (row["ZBMC"].ToString() == "村域建设用地总面积")
                        {
                            row["XZJQN"] = xz_cy;
                            row["GHMBN"] = gh_cy;
                        }
                        if (row["ZBMC"].ToString() == "村域城乡建设用地面积")
                        {
                            row["XZJQN"] = xz_cs;
                            row["GHMBN"] = gh_cs;
                        }
                        if (row["ZBMC"].ToString() == "村庄建设用地规模")
                        {
                            row["XZJQN"] = xz_cz;
                            row["GHMBN"] = gh_cz;
                        }
                        if (row["ZBMC"].ToString() == "公共管理与公共服务用地规模")
                        {
                            row["XZJQN"] = xz_ggss + xz_0704;
                            row["GHMBN"] = gh_ggss + gh_0704;
                        }
                        if (row["ZBMC"].ToString() == "公用设施用地规模")
                        {
                            row["XZJQN"] = xz_gyss;
                            row["GHMBN"] = gh_gyss;
                        }
                        if (row["ZBMC"].ToString() == "人均村庄建设用地面积")
                        {
                            row["XZJQN"] = Math.Round(xz_cz * 10000 / xz_rk, 2);
                            row["GHMBN"] = Math.Round(gh_cz * 10000 / gh_rk, 2);
                        }
                        if (row["ZBMC"].ToString() == "生态保护红线面积")
                        {
                            row["XZJQN"] = sthx;
                            row["GHMBN"] = sthx;
                        }
                        if (row["ZBMC"].ToString() == "永久基本农田保护面积")
                        {
                            row["XZJQN"] = jbnt;
                            row["GHMBN"] = jbnt;
                        }
                        if (row["ZBMC"].ToString() == "耕地保有量")
                        {
                            row["XZJQN"] = xz_gd;
                            row["GHMBN"] = gh_gd;
                        }
                        if (row["ZBMC"].ToString() == "林地保有量")
                        {
                            row["XZJQN"] = xz_ld;
                            row["GHMBN"] = gh_ld;
                        }
                        if (row["ZBMC"].ToString() == "湿地面积")
                        {
                            row["XZJQN"] = xz_sd;
                            row["GHMBN"] = gh_sd;
                        }
                        if (row["ZBMC"].ToString() == "自然和文化遗产")
                        {
                            row["XZJQN"] = xz_wh;
                            row["GHMBN"] = gh_wh;
                        }
                        // 保存
                        row.Store();
                    }
                }
            }

            // 删除中间要素
            List<string> list_delete = new List<string>() { "KJGNJGTZB", "tem_bj", "tem_sthx", "tem_jbnt", "tb_sthx", "tb_jbnt", "tem_exl", "sta_xz", "sta_gh", "fc_cs", "fc_cy", "fc_cz", "tb_cs", "tb_cy", "tb_cz" };
            foreach (var item in list_delete)
            {
                Arcpy.Delect(def_gdb + @"\" + item);
            }
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成空间功能结构调整表
        public static void CreateKJGNJGTZB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_gdb_path = output_folder_path + @"\" + village_name + @"数据库\规划表格.gdb";  // 输出GDB的位置

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地
            string yd_gh = in_gdb + @"\规划用地";    // 输入规划用地

            string empty_tb_path = def_folder + @"\村规空库\规划表格.gdb\KJGNJGTZB";   // 空库表的位置
            string output_tb_path = output_folder_path + @"\" + village_name + @"数据库\规划表格.gdb\KJGNJGTZB";   // 输出表的位置

            // 复制空库
            if (!Directory.Exists(def_folder + @"\村规空库"))
            {
                BaseTool.CopyResourceRar(@"CCTool.Data.Village.村规空库.rar", def_folder + @"\村规空库.rar");
            }

            if (isAddMessage) { pw.AddMessage("创建KJGNJGTZB表", Brushes.Gray); }

            // 复制空表
            Arcpy.CopyRows(empty_tb_path, output_tb_path);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "汇总用地指标", Brushes.Gray); }

            // 现状、规划用地汇总
            CalculateYD(yd_xz, "JQDLBM", def_gdb + @"\tb_xz", true);
            CalculateYD(yd_gh, "GHDLBM", def_gdb + @"\tb_gh", true);

            // 修改汇总字段
            Arcpy.AlterField(def_gdb + @"\tb_xz", "SUM_shape_area", "现状面积", "现状面积");
            Arcpy.AlterField(def_gdb + @"\tb_gh", "SUM_shape_area", "规划面积", "规划面积");

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "连接用地指标", Brushes.Gray); }

            // 连接字段
            Arcpy.JoinField(output_tb_path, "YDLXMC", def_gdb + @"\tb_xz", "YDLXMC", new List<string>() { "现状面积" });
            Arcpy.JoinField(output_tb_path, "YDLXMC", def_gdb + @"\tb_gh", "YDLXMC", new List<string>() { "规划面积" });

            // 获取总面积
            Arcpy.Statistics(yd_xz, def_gdb + @"\tb_all", "shape_area", "");
            double total_area = double.Parse(GisTool.GetCellFromPath(def_gdb + @"\tb_all", "SUM_shape_area", ""));

            // 获取村庄行政区代码
            string village_id = GisTool.GetCellFromPath(yd_xz, "ZLDWDM", "ZLDWDM IS NOT NULL")[..12];

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "计算其他字段值", Brushes.Gray); }

            /// 更新计算字段值
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(output_gdb_path)));
            // 获取要素类
            using Table table = gdb.OpenDataset<Table>("KJGNJGTZB");
            // 逐行游标
            using (RowCursor rowCursor = table.Search(null, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var xzmj = row["现状面积"];
                        var ghmj = row["规划面积"];

                        // 赋值
                        row["XZQDM"] = village_id;                      // 行政区代码
                        row["XZQMC"] = village_name;                // 行政区名称

                        if (xzmj is not null) { row["JQXZNMJ"] = Math.Round(double.Parse(xzmj.ToString()) / 10000, 2); }          // 现状面积
                        else { row["JQXZNMJ"] = 0; }

                        if (ghmj is not null) { row["GHMBNMJ"] = Math.Round(double.Parse(ghmj.ToString()) / 10000, 2); }         // 规划面积
                        else { row["GHMBNMJ"] = 0; }

                        double xz_updata = double.Parse(row["JQXZNMJ"].ToString());      // 更新后现状面积
                        double gh_updata = double.Parse(row["GHMBNMJ"].ToString());      // 更新后规划面积

                        row["JQXZNBZ"] = Math.Round(xz_updata / total_area * 1000000, 2);           // 现状比重
                        row["GHMBNBZ"] = Math.Round(gh_updata / total_area * 1000000, 2);      // 规划比重

                        row["GHQNMJZJ"] = gh_updata - xz_updata;            // 面积增减

                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除中间字段
            Arcpy.DeleteField(output_tb_path, "现状面积;规划面积");

            // 删除中间数据
            Arcpy.Delect(def_gdb + @"\tb_all");
            Arcpy.Delect(def_gdb + @"\tb_xz");
            Arcpy.Delect(def_gdb + @"\tb_gh");
            Directory.Delete(def_folder + @"\村规空库", true);
        }

        // 生成现状调整表【Excel】
        public static void CreateExcelXZTZB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string excel_GN = def_folder + @"\用地用海代码_村庄功能.xlsx";    // 村庄功能映射Excel
            string excel_TZB = output_excel_path + @"\现状指标表.xlsx";    // 输出现状指标表Excel

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地

            string field_xz = "JQDLBM";    // 现状编码字段
            string field_GN = "村庄功能";

            string output_table_xz = def_gdb + @"\output_table_xz";

            string statistics_fields = "Shape_Area SUM";

            if (isAddMessage) { pw.AddMessage("创建Excel表格：现状指标表", Brushes.Gray); }

            // 复制模板Excel和输出结果Excel
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_村庄功能.xlsx", excel_GN);
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】现状指标表.xlsx", excel_TZB);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "现状用地功能映射", Brushes.Gray); }
            // 现状用地功能映射
            GN_Mapper(yd_xz, field_xz, field_GN);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "统计面积", Brushes.Gray); }

            // 统计面积
            GisTool.MultiStatistics(yd_xz, output_table_xz, statistics_fields, new List<string>() { field_GN }, "总计", 1);

            // 删除中间字段
            Arcpy.DeleteField(yd_xz, field_GN);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "写入Excel", Brushes.Gray); }
            // 属性映射至Excel
            Dictionary<string, string> dict = GisTool.GetDictFromPath(output_table_xz, @"分组", "SUM_Shape_Area");

            OfficeTool.ExcelAttributeMapperDouble(excel_TZB + @"\sheet1$", 7, 4, dict, 3);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "Excel清理", Brushes.Gray); }
            // 删除0值行
            OfficeTool.ExcelDeleteNullRow(excel_TZB + @"\sheet1$", new List<int>() { 4 }, 3);
            // 删除参照列
            OfficeTool.ExcelDeleteCol(excel_TZB + @"\sheet1$", 7);
            // 删除中间数据
            Arcpy.Delect(new List<string>() { output_table_xz });

        }

        // 生成空间功能结构调整表【Excel】
        public static void CreateExcelKJGNJGTZB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string excel_GN = def_folder + @"\用地用海代码_村庄功能.xlsx";    // 村庄功能映射Excel
            string excel_TZB = output_excel_path + @"\空间功能结构调整表.xlsx";    // 输出空间功能结构调整表Excel

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地
            string yd_gh = in_gdb + @"\规划用地";    // 输入规划用地

            string field_xz = "JQDLBM";    // 现状编码字段
            string field_gh = "GHDLBM";   // 规划编码字段
            string field_GN = "村庄功能";

            string output_table_xz = def_gdb + @"\output_table_xz";
            string output_table_gh = def_gdb + @"\output_table_gh";
            string statistics_fields = "Shape_Area SUM";

            if (isAddMessage) { pw.AddMessage("创建Excel表格：空间功能结构调整表", Brushes.Gray); }

            // 复制模板Excel和输出结果Excel
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_村庄功能.xlsx", excel_GN);
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】空间功能结构调整表.xlsx", excel_TZB);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "现状、规划用地功能映射", Brushes.Gray); }
            // 现状、规划用地功能映射
            Arcpy.AddField(yd_xz, field_GN, "TEXT");
            Arcpy.AddField(yd_gh, field_GN, "TEXT");
            GN_Mapper(yd_xz, field_xz, field_GN);
            GN_Mapper(yd_gh, field_gh, field_GN);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "统计面积", Brushes.Gray); }
            // 统计面积
            GisTool.MultiStatistics(yd_xz, output_table_xz, statistics_fields, new List<string>() { field_GN }, "总计", 1);
            GisTool.MultiStatistics(yd_gh, output_table_gh, statistics_fields, new List<string>() { field_GN }, "总计", 1);
            // 删除中间字段
            Arcpy.DeleteField(yd_xz, field_GN);
            Arcpy.DeleteField(yd_gh, field_GN);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "写入Excel", Brushes.Gray); }
            // 属性映射至Excel
            Dictionary<string, string> dict_xz = GisTool.GetDictFromPath(output_table_xz, @"分组", "SUM_Shape_Area");
            Dictionary<string, string> dict_gh = GisTool.GetDictFromPath(output_table_gh, @"分组", "SUM_Shape_Area");
            OfficeTool.ExcelAttributeMapperDouble(excel_TZB + @"\sheet1$", 10, 4, dict_xz, 3);
            OfficeTool.ExcelAttributeMapperDouble(excel_TZB + @"\sheet1$", 10, 6, dict_gh, 3);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "Excel清理", Brushes.Gray); }
            // 删除0值行
            OfficeTool.ExcelDeleteNullRow(excel_TZB + @"\sheet1$", new List<int>() { 4, 6 }, 3);
            // 删除参照列
            OfficeTool.ExcelDeleteCol(excel_TZB + @"\sheet1$", 10);

            // 删除中间数据
            Arcpy.Delect(new List<string>() { output_table_xz, output_table_gh });
        }

        // 生成规划指标表【Excel】
        public static void CreateExcelGHZBB(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径

            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string excel_ZBB = output_excel_path + @"\规划指标一览表.xlsx";    // 输出规划指标一览表Excel
            string zbb = output_folder_path + @"\" + village_name + @"数据库\规划表格.gdb\GHZBB";   // GHZBB表的位置

            // 创建文件目录
            CreateTarget(village_name);
            if (isAddMessage) { pw.AddMessage( "生成规划指标一览表", Brushes.Gray); }
            // 生成GHZBB
            CreateGHZBB(village_name, pw, time_base);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "写入Excel", Brushes.Gray); }
            // 复制Excel表格
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】规划指标一览表.xlsx", excel_ZBB);
            // 属性映射至Excel
            Dictionary<string, string> dict_xz = GisTool.GetDictFromPath(zbb, "ZBMC", "XZJQN");
            Dictionary<string, string> dict_gh = GisTool.GetDictFromPath(zbb, "ZBMC", "GHMBN");
            OfficeTool.ExcelAttributeMapperDouble(excel_ZBB + @"\sheet1$", 1, 4, dict_xz, 2);
            OfficeTool.ExcelAttributeMapperDouble(excel_ZBB + @"\sheet1$", 1, 5, dict_gh, 2);
        }

        // 生成管控分区表【Excel】
        public static void CreateExcelGKFQ(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            // 获取参数
            string def_gdb = Project.Current.DefaultGeodatabasePath;   // 默认数据库位置
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径

            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string excel_GKFQ = output_excel_path + @"\国土空间管控表.xlsx";    // 输出国土空间管控表Excel
            string GKBJ = $@"{output_folder_path}\{village_name}数据库\村庄规划数据库.gdb\MBNGH\GKBJ";   // GKBJ的位置

            if (isAddMessage) { pw.AddMessage("生成GKBJ", Brushes.Gray); }

            // 创建文件目录
            CreateTarget(village_name, "MBNGH");
            // 生成GKBJ
            CreateGKBJ(village_name, pw, time_base);

            // 汇总指标
            GisTool.MultiStatistics(GKBJ, def_gdb + @"\s_gkbj", "SHAPE_Area SUM", new List<string>() { "GKBJLXMC" }, "合计", 1);

            if (isAddMessage) { pw.AddProcessMessage(0, time_base, "写入Excel", Brushes.Gray); }
            // 复制Excel表格
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】国土空间管控表.xlsx", excel_GKFQ);
            // 属性映射至Excel
            Dictionary<string, string> dict = GisTool.GetDictFromPath(def_gdb + @"\s_gkbj", "分组", "SUM_SHAPE_Area");
            OfficeTool.ExcelAttributeMapperDouble(excel_GKFQ + @"\sheet1$", 1, 2, dict, 3);

            // 删除中间数据
            Arcpy.Delect(def_gdb + @"\s_gkbj");
        }

        // 生成村域现状图【JPG】
        public static void CreateXZT(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            if (isAddMessage) { pw.AddMessage("创建现状指标表", Brushes.Gray); }
            // 创建现状指标表
            CreateJPGFolder(village_name);
            CreateExcelXZTZB(village_name, pw, time_base);

            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地

            // 获取excel单元格值的字典和列表
            Dictionary<string, string> dict = OfficeTool.GetDictFromExcel(excel_path + @"\sheet1$");
            // 获取村庄名称
            string village_fullName = dict["村庄名称"];
            // 获取规划期限
            string village_year = dict["规划期限"];

            // 复制制图包
            BaseTool.CopyResourceRar(@"CCTool.Data.Village.制图包.rar", def_folder + @"\制图包.rar");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导入现状图布局", Brushes.Gray); }

            // 导入现状图布局
            string pagePath = def_folder + @"\制图包\布局\村域现状图.pagx";
            IProjectItem pagx = ItemFactory.Instance.Create(pagePath) as IProjectItem;
            Project.Current.AddItem(pagx);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出现状表图片", Brushes.Gray); }
            // 导出现状表图片
            OfficeTool.ExcelImportToJPG(output_excel_path + @"\现状指标表.xlsx", output_excel_path + @"\现状指标表.jpg");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "生成地图要素", Brushes.Gray); }

            // 生成村界
            Arcpy.Dissolve(yd_xz, in_gdb + @"\村界");
            // 生成白底
            Arcpy.MinimumBoundingGeometry(in_gdb + @"\村界", in_gdb + @"\白底c");
            Arcpy.Buffer(in_gdb + @"\白底c", in_gdb + @"\白底", "2000 Meters");

            // 乡镇名
            List<string> village_names = CutVillageNames(village_fullName);
            string town_name = village_names[0] + village_names[1];
            // 生成乡镇界和乡镇界_周边
            string area_gdb = def_folder + @"\制图包\乡镇行政区划\乡镇.gdb";  // 乡镇边界库
            // 复制乡镇界
            if (GisTool.IsHaveFeaturClass(area_gdb, town_name))
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界");
            }
            else
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + "永泰县清凉镇", in_gdb + @"\乡镇界");
            }
            // 生成乡镇界周边
            Arcpy.Select(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界_周边", "CZMC <> \'" + village_name + "\'");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新数据", Brushes.Gray); }

            // 获取要素
            // 获取LayoutProjectItem
            LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("村域现状图"));
            Layout layout = layoutItem.GetLayout();            // 获取Layout                         
            MapFrame mf = layout.FindElement("现状图") as MapFrame;             // 获取主地图框
            Map map = mf.Map;                         // 获取主地图
            Layer mapBox = map.FindLayers("Mapbox影像")[0];     // 获取Mapbox影像图层
            Layer mapTD = map.FindLayers("天地图影像")[0];     // 获取天地图影像图层
            string txtPath = @"D:\ProSDKsettings\Settings.txt";
            int mapIndex = int.Parse(txtPath.GetAttFormTxtJson("basemapIndex"));

            MapFrame mf_index = layout.FindElement("导航图") as MapFrame;             // 获取索引图框
            Map map_index = mf_index.Map;                         // 获取索引图
            PictureElement windRose = layout.FindElement("风玫瑰") as PictureElement;          // 获取风玫瑰
            PictureElement picData = layout.FindElement("指标图") as PictureElement;          // 获取指标图

            // 更新地图坐标系
            map.SetSpatialReference(yd_xz.GetSpatialReference());
            map_index.SetSpatialReference(yd_xz.GetSpatialReference());
            // 更新地图源
            string oldGDBPath = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\制图包\lyr文件\示例库.gdb";
            map.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);
            map_index.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);

            // 更新图框名
            layout.SetName(village_fullName + "村庄规划（" + village_year + "）");

            // 创建图片【风玫瑰】
            Coordinate2D ll_wind = new Coordinate2D(818.1795, 616.3632);
            Coordinate2D ur_wind = new Coordinate2D(934.4086, 742.8888);
            Envelope env_wind = EnvelopeBuilderEx.CreateEnvelope(ll_wind, ur_wind);
            string picPath_wind = def_folder + $@"\制图包\风玫瑰\{village_names[0]}风玫瑰.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_wind.Extent, picPath_wind, "风玫瑰", true, new ElementInfo() { });

            // 创建图片【指标图】
            Coordinate2D ll_table = new Coordinate2D(773.0053, 67.8366);
            Coordinate2D ur_table = new Coordinate2D(1165.5944, 347.5667);
            Envelope env_table = EnvelopeBuilderEx.CreateEnvelope(ll_table, ur_table);
            string picPath_table = output_excel_path + @"\现状指标表.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_table.Extent, picPath_table, "现状指标表", true, new ElementInfo() { });

            // 打开地图影像
            if (mapIndex == 1)
            {
                mapTD.SetVisibility(true);
                mapBox.SetVisibility(false);
            }
            else
            {
                mapBox.SetVisibility(true);
                mapTD.SetVisibility(false);
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新视图范围", Brushes.Gray); }

            // 更新主地图范围
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(in_gdb)));
            // 获取要素类【村界】
            using FeatureClass featureClass = gdb.OpenDataset<FeatureClass>("村界");
            mf.SetCamera(featureClass.GetExtent());
            // 可以调节缩放
            Camera cam = mf.Camera;
            cam.Scale *= 1.15;
            mf.SetCamera(cam);

            // 获取要素类【乡镇界】
            if (GisTool.IsHaveFeaturClass(in_gdb, "乡镇界"))
            {
                using FeatureClass featureClass2 = gdb.OpenDataset<FeatureClass>("乡镇界");
                mf_index.SetCamera(featureClass2.GetExtent());
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出图片", Brushes.Gray); }

            // 图片属性
            JPEGFormat JPG = new JPEGFormat()
            {
                HasWorldFile = true,
                Resolution = DataStore.vgDPI,
                OutputFileName = output_excel_path + @"\现状用地图.jpg",
                ColorMode = JPEGColorMode.TwentyFourBitTrueColor,
            };
            // 导出图片
            layout.Export(JPG);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "移除布局，删除中间数据", Brushes.Gray); }

            // 移除布局
            Project.Current.RemoveItem(layoutItem);
            // 移除地图
            MapProjectItem mapt_dh = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("导航"));
            MapProjectItem map_xz = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("现状"));
            Project.Current.RemoveItem(mapt_dh);
            Project.Current.RemoveItem(map_xz);
            // 移除中间要素
            List<string> list_delect = new List<string>() { "村界", "白底", "白底c", "乡镇界", "乡镇界_周边" };
            foreach (var delect in list_delect)
            {
                Arcpy.Delect(in_gdb + @"\" + delect);
            }
            // 删除文件
            Directory.Delete(def_folder + @"\制图包", true);
        }

        // 生成村域规划图【JPG】
        public static void CreateGHT(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            if (isAddMessage) { pw.AddMessage("生成空间功能结构调整表", Brushes.Gray); }

            // 生成空间功能结构调整表
            CreateJPGFolder(village_name);
            CreateExcelKJGNJGTZB(village_name, pw, time_base);

            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地
            string yd_gh = in_gdb + @"\规划用地";    // 输入规划用地

            // 获取excel单元格值的字典和列表
            Dictionary<string, string> dict = OfficeTool.GetDictFromExcel(excel_path + @"\sheet1$");
            // 获取村庄名称
            string village_fullName = dict["村庄名称"];
            // 获取规划期限
            string village_year = dict["规划期限"];

            // 复制制图包
            BaseTool.CopyResourceRar(@"CCTool.Data.Village.制图包.rar", def_folder + @"\制图包.rar");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "生成地图要素", Brushes.Gray); }

            // 生成村界
            Arcpy.Dissolve(yd_xz, in_gdb + @"\村界");
            // 生成白底
            Arcpy.MinimumBoundingGeometry(in_gdb + @"\村界", in_gdb + @"\白底c");
            Arcpy.Buffer(in_gdb + @"\白底c", in_gdb + @"\白底", "2000 Meters");
            // 乡镇名
            List<string> village_names = CutVillageNames(village_fullName);
            string town_name = village_names[0] + village_names[1];
            // 生成乡镇界和乡镇界_周边
            string area_gdb = def_folder + @"\制图包\乡镇行政区划\乡镇.gdb";  // 乡镇边界库
            // 复制乡镇界
            // 复制乡镇界
            if (GisTool.IsHaveFeaturClass(area_gdb, town_name))
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界");
            }
            else
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + "永泰县清凉镇", in_gdb + @"\乡镇界");
            }
            // 生成乡镇界周边
            Arcpy.Select(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界_周边", "CZMC <> \'" + village_name + "\'");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "生成GKBJ", Brushes.Gray); }

            // 生成GKBJ
            if (!GisTool.IsHaveFeaturClass(in_gdb, "管控边界"))
            {
                CreateGKBJtoPath(village_name, in_gdb + @"\管控边界");
            }
            // 生成村庄建设边界
            if (!GisTool.IsHaveFeaturClass(in_gdb, "村庄建设边界"))
            {
                Arcpy.Select(in_gdb + @"\管控边界", in_gdb + @"\GKBJ_JS", "GKBJLXMC = '村庄集中建设区' Or GKBJLXMC = '村庄弹性发展区'");
                Arcpy.Dissolve(in_gdb + @"\GKBJ_JS", in_gdb + @"\村庄建设边界");
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导入规划图布局", Brushes.Gray); }

            // 导入规划图布局
            string pagePath = def_folder + @"\制图包\布局\村域规划图.pagx";
            IProjectItem pagx = ItemFactory.Instance.Create(pagePath) as IProjectItem;
            Project.Current.AddItem(pagx);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出空间功能结构调整表图片", Brushes.Gray); }
            // 导出空间功能结构调整表图片
            OfficeTool.ExcelImportToJPG(output_excel_path + @"\空间功能结构调整表.xlsx", output_excel_path + @"\空间功能结构调整表.jpg");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新数据", Brushes.Gray); }

            // 获取要素
            // 获取LayoutProjectItem
            LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("村域规划图"));
            Layout layout = layoutItem.GetLayout();            // 获取Layout                         
            MapFrame mf = layout.FindElement("规划图") as MapFrame;             // 获取主地图框
            Map map = mf.Map;                         // 获取主地图
            Layer mapBox = map.FindLayers("Mapbox影像")[0];     // 获取Mapbox影像图层
            Layer mapTD = map.FindLayers("天地图影像")[0];     // 获取天地图影像图层
            string txtPath = @"D:\ProSDKsettings\Settings.txt";
            int mapIndex = int.Parse(txtPath.GetAttFormTxtJson("basemapIndex"));

            MapFrame mf_index = layout.FindElement("导航图") as MapFrame;             // 获取索引图框
            Map map_index = mf_index.Map;                         // 获取索引图
            PictureElement windRose = layout.FindElement("风玫瑰") as PictureElement;          // 获取风玫瑰
            PictureElement picData = layout.FindElement("指标图") as PictureElement;          // 获取指标图

            // 更新地图坐标系
            map.SetSpatialReference(yd_xz.GetSpatialReference());
            map_index.SetSpatialReference(yd_xz.GetSpatialReference());
            // 更新地图源
            string oldGDBPath = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\制图包\lyr文件\示例库.gdb";
            map.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);
            map_index.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);

            // 更新图框名
            layout.SetName(village_fullName + "村庄规划（" + village_year + "）");

            // 创建图片【风玫瑰】
            Coordinate2D ll_wind = new Coordinate2D(818.1795, 616.3632);
            Coordinate2D ur_wind = new Coordinate2D(934.4086, 742.8888);
            Envelope env_wind = EnvelopeBuilderEx.CreateEnvelope(ll_wind, ur_wind);
            string picPath_wind = def_folder + $@"\制图包\风玫瑰\{village_names[0]}风玫瑰.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_wind.Extent, picPath_wind, "风玫瑰", true, new ElementInfo() { });

            // 创建图片【指标图】
            Coordinate2D ll_table = new Coordinate2D(773.1264, 63.8522);
            Coordinate2D ur_table = new Coordinate2D(1165.4729, 347.5667);
            Envelope env_table = EnvelopeBuilderEx.CreateEnvelope(ll_table, ur_table);
            string picPath_table = output_excel_path + @"\空间功能结构调整表.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_table.Extent, picPath_table, "空间功能结构调整表", true, new ElementInfo() { });

            // 打开地图影像
            if (mapIndex == 1)
            {
                mapTD.SetVisibility(true);
                mapBox.SetVisibility(false);
            }
            else
            {
                mapBox.SetVisibility(true);
                mapTD.SetVisibility(false);
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新视图范围", Brushes.Gray); }

            // 更新主地图范围
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(in_gdb)));
            // 获取要素类【村界】
            using FeatureClass featureClass = gdb.OpenDataset<FeatureClass>("村界");
            mf.SetCamera(featureClass.GetExtent());
            // 可以调节缩放
            Camera cam = mf.Camera;
            cam.Scale *= 1.15;
            mf.SetCamera(cam);

            // 获取要素类【乡镇界】
            if (GisTool.IsHaveFeaturClass(in_gdb, "乡镇界"))
            {
                using FeatureClass featureClass2 = gdb.OpenDataset<FeatureClass>("乡镇界");
                mf_index.SetCamera(featureClass2.GetExtent());
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出图片", Brushes.Gray); }

            // 图片属性
            JPEGFormat JPG = new JPEGFormat()
            {
                HasWorldFile = true,
                Resolution = DataStore.vgDPI,
                OutputFileName = output_excel_path + @"\规划用地图.jpg",
                ColorMode = JPEGColorMode.TwentyFourBitTrueColor,
            };
            // 导出图片
            layout.Export(JPG);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "移除布局，删除中间数据", Brushes.Gray); }

            // 移除布局
            Project.Current.RemoveItem(layoutItem);
            // 移除地图
            MapProjectItem mapt_dh = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("导航"));
            MapProjectItem map_gh = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("规划"));
            Project.Current.RemoveItem(mapt_dh);
            Project.Current.RemoveItem(map_gh);
            // 移除中间要素
            List<string> list_delect = new List<string>() { "村界", "白底", "白底c", "乡镇界", "乡镇界_周边", "村庄建设边界", "GKBJ_JS" };
            foreach (var delect in list_delect)
            {
                Arcpy.Delect(in_gdb + @"\" + delect);
            }
            // 删除管控边界
            if (isAddMessage) { Arcpy.Delect(in_gdb + @"\管控边界"); }
            // 删除文件
            Directory.Delete(def_folder + @"\制图包", true);

        }

        // 生成管制边界图【JPG】
        public static void CreateGKBJT(string village_name, ProcessWindow pw, DateTime time_base, bool isAddMessage = false)
        {
            if (isAddMessage) { pw.AddMessage("生成规划指标表", Brushes.Gray); }

            // 生成规划指标表
            CreateJPGFolder(village_name);
            CreateExcelGHZBB(village_name, pw, time_base);

            // 获取参数
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string def_gdb = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
            string input_folder_path = def_folder + @"\1-输入文件";        // 输入文件包路径
            string in_gdb = input_folder_path + @"\" + village_name + @".gdb";   // 输入gdb路径
            string excel_path = input_folder_path + @"\" + village_name + @".xlsx";   // 输入excel路径
            string output_folder_path = def_folder + @"\2-输出文件";        // 输出文件包路径
            string output_excel_path = output_folder_path + @"\" + village_name + @"表格+图集";  // 输出文件夹的位置

            string yd_xz = in_gdb + @"\现状用地";    // 输入现状用地
            string yd_gh = in_gdb + @"\规划用地";    // 输入规划用地

            // 获取excel单元格值的字典和列表
            Dictionary<string, string> dict = OfficeTool.GetDictFromExcel(excel_path + @"\sheet1$");
            // 获取村庄名称
            string village_fullName = dict["村庄名称"];
            // 获取规划期限
            string village_year = dict["规划期限"];

            // 复制制图包
            BaseTool.CopyResourceRar(@"CCTool.Data.Village.制图包.rar", def_folder + @"\制图包.rar");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "生成地图要素", Brushes.Gray); }

            // 生成村界
            Arcpy.Dissolve(yd_xz, in_gdb + @"\村界");
            // 生成白底
            Arcpy.MinimumBoundingGeometry(in_gdb + @"\村界", in_gdb + @"\白底c");
            Arcpy.Buffer(in_gdb + @"\白底c", in_gdb + @"\白底", "2000 Meters");
            // 乡镇名
            List<string> village_names = CutVillageNames(village_fullName);
            string town_name = village_names[0] + village_names[1];
            // 生成乡镇界和乡镇界_周边
            string area_gdb = def_folder + @"\制图包\乡镇行政区划\乡镇.gdb";  // 乡镇边界库
            // 复制乡镇界
            if (GisTool.IsHaveFeaturClass(area_gdb, town_name))
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界");
            }
            else
            {
                Arcpy.CopyFeatures(area_gdb + @"\" + "永泰县清凉镇", in_gdb + @"\乡镇界");
            }
            // 生成乡镇界周边
            Arcpy.Select(area_gdb + @"\" + town_name, in_gdb + @"\乡镇界_周边", "CZMC <> \'" + village_name + "\'");
            // 生成GKBJ
            CreateGKBJtoPath(village_name, in_gdb + @"\管控边界");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导入管制图布局", Brushes.Gray); }

            // 导入管制图布局
            string pagePath = def_folder + @"\制图包\布局\管制边界图.pagx";
            IProjectItem pagx = ItemFactory.Instance.Create(pagePath) as IProjectItem;
            Project.Current.AddItem(pagx);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出规划指标一览表图片", Brushes.Gray); }
            // 导出现状表图片
            OfficeTool.ExcelImportToJPG(output_excel_path + @"\规划指标一览表.xlsx", output_excel_path + @"\规划指标一览表.jpg");

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新数据", Brushes.Gray); }

            // 获取要素
            // 获取LayoutProjectItem
            LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("管制边界图"));
            Layout layout = layoutItem.GetLayout();            // 获取Layout                         
            MapFrame mf = layout.FindElement("管制图") as MapFrame;             // 获取主地图框
            Map map = mf.Map;                         // 获取主地图
            Layer mapBox = map.FindLayers("Mapbox影像")[0];     // 获取Mapbox影像图层
            Layer mapTD = map.FindLayers("天地图影像")[0];     // 获取天地图影像图层
            string txtPath = @"D:\ProSDKsettings\Settings.txt";
            int mapIndex = int.Parse(txtPath.GetAttFormTxtJson("basemapIndex"));

            MapFrame mf_index = layout.FindElement("导航图") as MapFrame;             // 获取索引图框
            Map map_index = mf_index.Map;                         // 获取索引图
            PictureElement windRose = layout.FindElement("风玫瑰") as PictureElement;          // 获取风玫瑰
            PictureElement picData = layout.FindElement("指标图") as PictureElement;          // 获取指标图

            // 更新地图坐标系
            map.SetSpatialReference(yd_xz.GetSpatialReference());
            map_index.SetSpatialReference(yd_xz.GetSpatialReference());
            // 更新地图源
            string oldGDBPath = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\制图包\lyr文件\示例库.gdb";
            map.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);
            map_index.FindAndReplaceWorkspacePath(oldGDBPath, in_gdb);

            // 更新图框名
            layout.SetName(village_fullName + "村庄规划（" + village_year + "）");

            // 创建图片【风玫瑰】
            Coordinate2D ll_wind = new Coordinate2D(818.1795, 616.3632);
            Coordinate2D ur_wind = new Coordinate2D(934.4086, 742.8888);
            Envelope env_wind = EnvelopeBuilderEx.CreateEnvelope(ll_wind, ur_wind);
            string picPath_wind = def_folder + $@"\制图包\风玫瑰\{village_names[0]}风玫瑰.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_wind.Extent, picPath_wind, "风玫瑰", true, new ElementInfo() { });

            // 创建图片【指标图】
            Coordinate2D ll_table = new Coordinate2D(773.1344, 152.8466);
            Coordinate2D ur_table = new Coordinate2D(1165.4653, 375.8342);
            Envelope env_table = EnvelopeBuilderEx.CreateEnvelope(ll_table, ur_table);
            string picPath_table = output_excel_path + @"\规划指标一览表.jpg";
            ElementFactory.Instance.CreatePictureGraphicElement(layout, env_table.Extent, picPath_table, "规划指标一览表", true, new ElementInfo() { });

            // 打开地图影像
            if (mapIndex == 1)
            {
                mapTD.SetVisibility(true);
                mapBox.SetVisibility(false);
            }
            else
            {
                mapBox.SetVisibility(true);
                mapTD.SetVisibility(false);
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "更新视图范围", Brushes.Gray); }

            // 更新主地图范围
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(in_gdb)));
            // 获取要素类【村界】
            using FeatureClass featureClass = gdb.OpenDataset<FeatureClass>("村界");
            mf.SetCamera(featureClass.GetExtent());
            // 可以调节缩放
            Camera cam = mf.Camera;
            cam.Scale *= 1.15;
            mf.SetCamera(cam);

            // 获取要素类【乡镇界】
            if (GisTool.IsHaveFeaturClass(in_gdb, "乡镇界"))
            {
                using FeatureClass featureClass2 = gdb.OpenDataset<FeatureClass>("乡镇界");
                mf_index.SetCamera(featureClass2.GetExtent());
            }

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "导出图片", Brushes.Gray); }

            // 图片属性
            JPEGFormat JPG = new JPEGFormat()
            {
                HasWorldFile = true,
                Resolution = DataStore.vgDPI,
                OutputFileName = output_excel_path + @"\管制边界图.jpg",
                ColorMode = JPEGColorMode.TwentyFourBitTrueColor,
            };
            // 导出图片
            layout.Export(JPG);

            if (isAddMessage) { pw.AddProcessMessage(10, time_base, "移除布局，删除中间数据", Brushes.Gray); }

            // 移除布局
            Project.Current.RemoveItem(layoutItem);
            // 移除地图
            MapProjectItem mapt_dh = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("导航"));
            MapProjectItem map_gh = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("管制"));
            Project.Current.RemoveItem(mapt_dh);
            Project.Current.RemoveItem(map_gh);
            // 移除中间要素
            List<string> list_delect = new List<string>() { "村界", "白底", "白底c", "乡镇界", "乡镇界_周边", "管控边界" };
            foreach (var delect in list_delect)
            {
                Arcpy.Delect(in_gdb + @"\" + delect);
            }
            // 删除文件
            Directory.Delete(def_folder + @"\制图包", true);
        }

        // 村庄功能映射
        public static string GN_Mapper(string yd, string field_bm, string field_GN)
        {
            string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
            string excel_GN = def_folder + @"\用地用海代码_村庄功能.xlsx";    // 村庄功能映射Excel
            // 复制模板Excel和输出结果Excel
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海代码_村庄功能.xlsx", excel_GN);

            // 汇总统计现状用地
            Arcpy.AddField(yd, field_GN, "TEXT");     // 添加一个参照字段
            // 属性映射
            GisTool.AttributeMapper(yd, field_bm, field_GN, excel_GN + @"\sheet1$");

            // 获取Table
            Table table = yd.TargetTable();

            // 逐行游标
            using (RowCursor rowCursor = table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        if (field_bm == "JQDLBM")
                        {
                            // 获取value
                            var value = row["CZCSXM"];
                            if (value is not null)
                            {
                                var va = value.ToString();
                                // 赋值
                                if (va == "202" || va == "202A" || va == "201" || va == "201A")
                                {
                                    row[field_GN] = "城镇用地";
                                }
                            }
                        }
                        else if (field_bm == "GHDLBM")
                        {
                            // 获取value
                            var value = row["SSBJLX"];
                            if (value is not null)
                            {
                                var va = value.ToString();
                                // 赋值
                                if (va == "Z" || va == "z")
                                {
                                    row[field_GN] = "城镇用地";
                                }
                            }
                        }
                        // 保存
                        row.Store();
                    }
                }
            }
            // 删除参照Excel表
            File.Delete(excel_GN);

            return yd;
        }
    }
}
