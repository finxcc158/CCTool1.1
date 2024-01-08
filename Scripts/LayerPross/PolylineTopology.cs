using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts.LayerPross
{
	internal class PolylineTopology : Button
	{
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "线要素拓扑检查";

        protected override async void OnClick()
        {
            try
            {
                var map = MapView.Active.Map;
                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;
                // 获取工程默认文件夹位置
                var def_path = Project.Current.HomeFolderPath;
                // 获取图层
                FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                // 如果选择的不是面要素或是无选择，则返回
                if (ly.ShapeType != esriGeometryType.esriGeometryPolyline || ly == null)
                {
                    MessageBox.Show("错误！请选择一个线要素！");
                    return;
                }

                string db_name = "Top2Check";    // 要素数据集名
                string fc_name = "top_fc";        // 要素名
                string top_name = "Topology";       // TOP名
                string db_path = gdb + "\\" + db_name;    // 要素数据集路径
                string fc_path = db_path + "\\" + fc_name;         // 要素路径
                string top_path = db_path + "\\" + top_name;         // TOP路径

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "创建检查用的数据库");

                    //获取图层的坐标系
                    var sr = ly.GetSpatialReference();
                    //在数据库中创建要素数据集
                    Arcpy.CreateFeatureDataset(gdb, db_name, sr);
                    // 将所选要素复制到创建的要素数据集中
                    Arcpy.CopyFeatures(ly, fc_path);

                    pw.AddProcessMessage(10, time_base, "创建拓扑并设置规则");
                    // 新建拓扑
                    Arcpy.CreateTopology(db_path, top_name);
                    // 向拓扑中添加要素
                    Arcpy.AddFeatureClassToTopology(top_path, fc_path);
                    // 添加拓扑规则【悬挂点】
                    Arcpy.AddRuleToTopology(top_path, "Must Not Have Dangles (Line)", fc_path);
                    Arcpy.AddRuleToTopology(top_path, "Must Not Have Pseudo-Nodes (Line)", fc_path);

                    pw.AddProcessMessage(20, time_base, "验证拓扑");

                    // 验证拓扑
                    Arcpy.ValidateTopology(top_path);
                    // 输出TOP错误
                    Arcpy.ExportTopologyErrors(top_path, gdb, "TopErr");

                    pw.AddProcessMessage(20, time_base, "生成结果");

                    Arcpy.CopyFeatures(gdb + @"\TopErr_point", gdb + @"\拓扑检查结果_点", true);

                    // 删除中间要素
                    List<string> list_del = new List<string>() { "TopErr_point", "TopErr_line", "TopErr_poly" };
                    foreach (var fc in list_del)
                    {
                        Arcpy.Delect(gdb + @"\" + fc);
                    }
                    // 删除数据集和符号图层
                    Arcpy.Delect(db_path);

                    pw.AddProcessMessage(80, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
	}
}
