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
    internal class VgCheckGeometry : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "村规_要素几何检查";

        // 获取参数
        Map map = MapView.Active.Map;
        static string folder_path = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
        static string gdb_path = Project.Current.DefaultGeodatabasePath;    // 工程默认数据库

        protected override async void OnClick()
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                string input_folder_name = "1-输入文件";    // 输入文件包名
                string input_folder_path = folder_path + @"\" + input_folder_name;        // 输入文件包路径

                await QueuedTask.Run(() =>
                {
                    // 检查是否存在文件夹【1-输入文件】
                    if (Directory.Exists(input_folder_path))
                    {
                        // 检查是否存在村规Excel文件
                        List<string> list_excel = Directory.GetFiles(input_folder_path, "*.xlsx").ToList();
                        if (list_excel.Count > 0)
                        {
                            foreach (var excel in list_excel)
                            {
                                // 获取Excel文件名
                                string excel_name = excel[(excel.LastIndexOf(@"\") + 1)..].Replace(".xlsx", "");

                                pw.AddMessage("检查村规几何：  【" + excel_name + "】" + "\r");

                                // 获取gdb路径
                                string vg_gdb_path = input_folder_path + @"\" + excel_name + ".gdb";
                                // 检查是否有对应的GDB文件
                                if (Directory.Exists(vg_gdb_path))
                                {
                                    // 检查数据库中的要素的几何属性
                                    CheckFeatureClass(vg_gdb_path, pw, time_base);
                                }
                            }
                        }
                    }
                });

                pw.AddProcessMessage("工具运行完成！！！", 50, time_base, Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        // 检查数据库
        public static void CheckFeatureClass(string vg_gdb_path, ProcessWindow pw, DateTime time_base)
        {
            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(vg_gdb_path)));
            // 获取所有要素类
            IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();

            foreach (FeatureClassDefinition featureClass in featureClasses)
            {
                //要素名
                string fc_name = featureClass.GetName();
                using FeatureClass fc = gdb.OpenDataset<FeatureClass>(fc_name);
                // 获取要素类路径
                string fc_path = fc.GetPath().ToString().Replace("file:///", "");

                // 检查【现状用地】
                if (fc_name == "现状用地")
                {
                    pw.AddProcessMessage("检查要素：【现状用地】", 20, time_base, Brushes.Green);
                    // 检查要素类型
                    CheckGeometryType(fc_name, "Polygon", featureClass, pw);
                    // 检查几何错误
                    CheckGeometryErr(fc_name, fc_path, pw);
                    // 检查拓扑错误
                    CheckTopPolygon(fc_name, fc_path, pw);
                }

                // 检查【规划用地】
                else if (fc_name == "规划用地")
                {
                    pw.AddProcessMessage("检查要素：【规划用地】", 20, time_base, Brushes.Green);
                    // 检查要素类型
                    CheckGeometryType(fc_name, "Polygon", featureClass, pw);
                    // 检查几何错误
                    CheckGeometryErr(fc_name, fc_path, pw);
                    // 检查拓扑错误
                    CheckTopPolygon(fc_name, fc_path, pw);
                }

                // 检查【现状公服】
                else if (fc_name == "现状公服")
                {
                    pw.AddProcessMessage("检查要素：【现状公服】", 20, time_base, Brushes.Green);
                    // 检查要素类型
                    CheckGeometryType(fc_name, "Point", featureClass, pw);
                }

                // 检查【规划公服】
                else if (fc_name == "规划公服")
                {
                    pw.AddProcessMessage("检查要素：【规划公服】", 20, time_base, Brushes.Green);
                    // 检查要素类型
                    CheckGeometryType(fc_name, "Point", featureClass, pw);
                }

                // 检查【文保】
                else if (fc_name == "文保")
                {
                    pw.AddProcessMessage("检查要素：【文保】", 20, time_base, Brushes.Green);
                    // 检查要素类型
                    CheckGeometryType(fc_name, "Polygon", featureClass, pw);
                    // 检查几何错误
                    CheckGeometryErr(fc_name, fc_path, pw);
                    // 检查拓扑错误
                    CheckTopPolygon(fc_name, fc_path, pw);
                }
            }
        }

        // 检查要素类型
        public static void CheckGeometryType(string fc_name, string geo_type, FeatureClassDefinition featureClass, ProcessWindow pw)
        {
            pw.AddMessage("检查要素类型" + "\r", Brushes.Gray);
            if (featureClass.GetShapeType().ToString() != geo_type)
            {
                pw.AddMessage("【" + fc_name +"】必须是面要素！" + "\r", Brushes.Red);
            }
        }

        // 检查几何错误
        public static void CheckGeometryErr(string fc_name, string fc_path, ProcessWindow pw)
        {
            pw.AddMessage("检查几何错误" + "\r", Brushes.Gray);
            // 检查要素是否有几何错误
            string err_path = gdb_path + @"\cg";
            Arcpy.CheckGeometry(fc_path, err_path);
            int err_count = Arcpy.GetCount(err_path);
            if (err_count > 0)
            {
                pw.AddMessage($"【{fc_name}】存在几何错误！" + "\r", Brushes.Red);
            }
            // 删除中间数据
            Arcpy.Delect(err_path);
        }

        // 检查拓扑错误
        public static void CheckTopPolygon(string fc_name, string fc_path, ProcessWindow pw)
        {
            pw.AddMessage("检查拓扑错误" + "\r", Brushes.Gray);

            // 定义规则
            List<string> rules = new List<string>() { "Must Not Overlap (Area)" , "Must Not Have Gaps (Area)"};
            // 拓扑检查
            GisTool.TopologyCheck(fc_path, rules, gdb_path);
            
            // 获取错误数量
            int overlap_err = Arcpy.GetCount(gdb_path + @"\TopErr_poly");
            int gaps_err = Arcpy.GetCount(gdb_path + @"\TopErr_line");
            if (overlap_err > 0)
            {
                pw.AddMessage("【" + fc_name + "】存在重叠！" + "\r", Brushes.Red);
            }
            if (gaps_err > 1)
            {
                pw.AddMessage("【" + fc_name + "】存在空隙！" + "\r", Brushes.Red);
            }
        }

    }
}
