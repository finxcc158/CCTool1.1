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

namespace CCTool.Scripts.UI.ProButton
{
    internal class PolygonTopology : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "面要素拓扑检查";

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
                if (ly.ShapeType != esriGeometryType.esriGeometryPolygon || ly == null)
                {
                    MessageBox.Show("错误！请选择一个面要素！");
                    return;
                }
                
                string db_name = "Top2Check";    // 要素数据集名
                string fc_name = "top_fc";        // 要素名
                string top_name = "Topology";       // TOP名
                string db_path = gdb + "\\" + db_name;    // 要素数据集路径
                string fc_path = db_path + "\\" + fc_name;         // 要素路径
                string top_path = db_path + "\\" + top_name;         // TOP路径

                string err_fc = @"检查结果";
                string err_field = @"错误说明";

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
                    // 添加拓扑规则【重叠】
                    Arcpy.AddRuleToTopology(top_path, "Must Not Overlap (Area)", fc_path);

                    pw.AddProcessMessage(20, time_base, "生成重叠错误");

                    // 验证拓扑
                    Arcpy.ValidateTopology(top_path);
                    // 输出TOP错误
                    Arcpy.ExportTopologyErrors(top_path, gdb, "TopErr");

                    pw.AddProcessMessage(20, time_base, "生成空隙错误");

                    // 生成空隙
                    GisTool.GetCave(fc_path, gdb + @"\" + err_fc);
                    // 添加说明字段
                    Arcpy.AddField(gdb + @"\" + err_fc, err_field, "TEXT");
                    // 空隙错误说明赋值
                    Arcpy.CalculateField(gdb + @"\" + err_fc, err_field, "'存在空隙'");
                    // 合并错误
                    Arcpy.Append(gdb + @"\TopErr_poly", gdb + @"\" + err_fc);
                    // 加载错误面图层
                    MapCtlTool.AddFeatureLayerToMap(gdb + @"\" + err_fc);

                    pw.AddProcessMessage(20, time_base, "生成错误标记");

                    // 重叠错误说明赋值
                    FeatureLayer init_layer = map.FindLayers(err_fc)[0] as FeatureLayer;
                    using (ArcGIS.Core.Data.Table table = init_layer.GetTable())
                    {
                        using (RowCursor rowCursor = table.Search(null, false))
                        {
                            TableDefinition tableDefinition = table.GetDefinition();
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    // 获取value
                                    var va = row[err_field];
                                    // 赋值
                                    if (va is null)
                                    {
                                        row[err_field] = "存在重叠面";
                                    }
                                    row.Store();
                                }
                            }
                        }
                    }
                    // 删除多余字段
                    Arcpy.DeleteField(err_fc, "ORIG_FID");

                    pw.AddProcessMessage(20, time_base, "应用错误图层的显示符号");

                    // 复制图层符号
                    string copy_lyrx = def_path + @"\检查结果.lyrx";
                    BaseTool.CopyResourceFile(@"CCTool.Data.Layers." + @"检查结果.lyrx", copy_lyrx);
                    // 应用图层符号
                    Arcpy.ApplySymbologyFromLayer(err_fc, copy_lyrx);

                    // 删除中间要素
                    List<string> list_del = new List<string>() { "TopErr_point", "TopErr_line", "TopErr_poly" };
                    foreach (var fc in list_del)
                    {
                        Arcpy.Delect(gdb + @"\" + fc);
                    }
                    // 删除数据集和符号图层
                    Arcpy.Delect(db_path);
                    File.Delete(copy_lyrx);

                    pw.AddProcessMessage(10, time_base, "工具运行完成！！！", Brushes.Blue);
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
