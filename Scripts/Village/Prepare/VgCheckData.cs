using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CCTool.Scripts.UI.ProButton
{
    internal class VgCheckData : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "村规_数据完整性检查";

        protected override async void OnClick()
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                // 获取参数
                string folder_path = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
                string gdb_path = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库
                string input_folder_name = "1-输入文件";    // 输入文件包名
                string input_folder_path = folder_path + @"\" + input_folder_name;        // 输入文件包路径

                await QueuedTask.Run(() =>
                {
                    // 检查是否存在文件夹【1-输入文件】
                    if (!Directory.Exists(input_folder_path))
                    {
                        pw.AddMessage("缺少文件包：【" + input_folder_name + @"】！" + "\r", Brushes.Red);
                    }
                    else
                    {
                        // 检查是否存在村规Excel文件
                        List<string> list_excel = Directory.GetFiles(input_folder_path, "*.xlsx").ToList();
                        if (list_excel.Count == 0)
                        {
                            pw.AddMessage("缺少村规Excel文件！" + "\r", Brushes.Red);
                        }
                        else
                        {
                            foreach (var excel in list_excel)
                            {
                                // 获取Excel文件名
                                string excel_name = excel[(excel.LastIndexOf(@"\") + 1)..].Replace(".xlsx", "");
                                pw.AddMessage("检查村规数据：  【" + excel_name + "】…………" + "\r");

                                pw.AddMessage("指标表" + "\r", Brushes.Gray);
                                // 检查Excel文件中的指标是否规范
                                CheckExcel(excel + @"\sheet1$", pw);

                                
                                pw.AddMessage("数据库" + "\r", Brushes.Gray);
                                // 获取gdb路径
                                string vg_gdb_path = input_folder_path + @"\" + excel_name + ".gdb";
                                // 检查是否有对应的GDB文件
                                if (!Directory.Exists(vg_gdb_path))
                                {
                                    pw.AddMessage("缺少村规GDB文件：" + excel_name + "！" + "\r", Brushes.Red);
                                }
                                else
                                {
                                    // 检查数据库
                                    CheckFeatureClass(vg_gdb_path, pw);
                                }
                            }
                        }
                    }
                });
                
                pw.AddMessage("工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
        }

        // 检查Excel指标
        public static void CheckExcel(string excel_path, ProcessWindow pw)
        {
            // 获取excel单元格值的字典和列表
            Dictionary<string, string> dict = OfficeTool.GetDictFromExcelAll(excel_path);
            List<string> list_key = new List<string>(dict.Keys);
            List<string> list_value = new List<string>(dict.Values);

            // 定义检查用的指标项列表
            List<string> list_check = new List<string>() { "村庄名称", "村庄类型", "规划期限", "现状人口", "规划人口", "现状自然和文化遗产", "规划自然和文化遗产"};
            
            for (int i = 0; i < list_check.Count; i++)
            {
                // 检查是否缺少指标项
                if (!list_key.Contains(list_check[i]))
                {
                    pw.AddMessage("缺少【" + list_check[i] + "】指标！" + "\r", Brushes.Red);
                }
                // 检查value是否规范
                else
                {
                    int index = list_key.IndexOf(list_check[i]);
                    // value空值的情况
                    if (list_value[index] == "" || list_value[index] is null)
                    {
                        pw.AddMessage("【" + list_key[index] + "】的指标值不能为空！" + "\r", Brushes.Red);
                    }
                    // value值不规范的情况
                    if (list_key[index] == "村庄名称")
                    {
                        if (list_value[index].Contains("市") || list_value[index].Contains("县") || list_value[index].Contains("实验区") || list_value[index].Contains("区"))
                        {
                            continue;
                        }
                        else
                        {
                            pw.AddMessage("村庄名称的命名不完整！！"  + "\r", Brushes.Red);
                        }
                    }
                }
            }
        }

        // 检查数据库
        public static void CheckFeatureClass(string vg_gdb_path, ProcessWindow pw)
        {
            // 待检查的要素类名称
            List<string> fc_names = new List<string>() { "现状用地", "规划用地", "现状公服", "规划公服", "永久基本农田", "生态保护红线" };
            // 已准备好的要素类名称
            List<string> input_fc_names = vg_gdb_path.GetFeatureClassNameFromGDB();

            // 检查缺失的要素类
            foreach (string fc_name in fc_names)
            {
                if (!input_fc_names.Contains(fc_name))
                {
                    if (fc_name == "永久基本农田" || fc_name == "生态保护红线")  // 生态红线和永农，来个弱提示
                    {
                        pw.AddMessage("缺少【" + fc_name + "】要素！ 请确认是否真的没有相应要素！" + "\r", Brushes.Orange);
                    }
                    else
                    {
                        pw.AddMessage("缺少【" + fc_name + "】要素！" + "\r", Brushes.Red);
                    }
                }
            }

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(vg_gdb_path)));
            // 获取所有要素类
            IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();
            // 获取三调坐标系
            var sd = gdb.GetDefinitions<FeatureClassDefinition>().FirstOrDefault(x =>x.GetName() == "现状用地");
            var sr = sd.GetSpatialReference();

            foreach (FeatureClassDefinition featureClass in featureClasses)
            {
                //要素名
                string fc_name = featureClass.GetName();
                // 获取字段列表
                IReadOnlyList<Field> fields = featureClass.GetFields();

                // 检查【现状用地】
                if (fc_name == "现状用地")         
                {
                    // 所需字段名列表
                    List<string> field_names = new List<string>() { "ZLDWDM", "JQDLBM", "JQDLMC", "CZCSXM"};
                    foreach (var field_name in field_names)
                    {
                        if (!fields.Any(f => f.Name == field_name))
                        {
                            pw.AddMessage("【" + fc_name + "】要素缺少字段：  " + field_name + "\r", Brushes.Red);
                        }
                    }
                }

                // 检查【规划用地】
                else if (fc_name == "规划用地")
                {
                    // 所需字段名列表
                    List<string> field_names = new List<string>() { "GHDLBM", "GHDLMC", "SSBJLX" };
                    foreach (var field_name in field_names)
                    {
                        if (!fields.Any(f => f.Name == field_name))
                        {
                            pw.AddMessage("【" + fc_name + "】要素缺少字段：  " + field_name + "\r", Brushes.Red);
                        }
                    }
                }

                // 检查【现状公服】
                else if (fc_name == "现状公服")
                {
                    // 所需字段名列表
                    List<string> field_names = new List<string>() { "SSLXMC" };
                    foreach (var field_name in field_names)
                    {
                        if (!fields.Any(f => f.Name == field_name))
                        {
                            pw.AddMessage("【" + fc_name + "】要素缺少字段：  " + field_name + "\r", Brushes.Red);
                        }
                    }
                }

                // 检查【规划公服】
                else if (fc_name == "规划公服")
                {
                    // 所需字段名列表
                    List<string> field_names = new List<string>() { "GHSSLXMC" };
                    foreach (var field_name in field_names)
                    {
                        if (!fields.Any(f => f.Name == field_name))
                        {
                            pw.AddMessage("【" + fc_name + "】要素缺少字段：  " + field_name + "\r", Brushes.Red);
                        }
                    }
                }

                // 检查【文保】
                else if (fc_name == "文保")
                {
                    // 所需字段名列表
                    List<string> field_names = new List<string>() { "LSWHLX", "JBDM" };
                    foreach (var field_name in field_names)
                    {
                        if (!fields.Any(f => f.Name == field_name))
                        {
                            pw.AddMessage("【" + fc_name + "】要素缺少字段：  " + field_name + "\r", Brushes.Red);
                        }
                    }
                }

                // 检查【生态保护红线】
                else if (fc_name == "生态保护红线")
                {
                    // 判断坐标系是否一致
                    if (!featureClass.GetSpatialReference().IsEqual(sr))
                    {
                        pw.AddMessage("【" + fc_name + "】的坐标系和三调不一致，请检查！  " + "\r", Brushes.Red);
                    }
                }
                // 检查【永久基本农田】
                else if (fc_name == "永久基本农田")
                {
                    // 判断坐标系是否一致
                    if (featureClass.GetSpatialReference().Name != sr.Name)
                    {
                        pw.AddMessage("【" + fc_name + "】的坐标系和三调不一致，请检查！  " + "\r", Brushes.Red);
                    }
                }
            }
        }
    }
}
