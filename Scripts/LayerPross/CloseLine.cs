using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Brushes = System.Windows.Media.Brushes;
using Color = ArcGIS.Core.Internal.CIM.Color;

namespace CCTool.Scripts.UI.ProButton
{
    internal class CloseLine : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "线闭合";

        protected override async void OnClick()
        {
            try
            {
                string defGDBPath = Project.Current.DefaultGeodatabasePath;
                Map map = MapView.Active.Map;

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                await QueuedTask.Run(() => 
                {
                    // 获取当前选择的图层
                    FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                    // 如果选择的不是线要素或是无选择，则返回
                    if (featureLayer.ShapeType != esriGeometryType.esriGeometryPolyline || featureLayer == null)
                    {
                        MessageBox.Show("错误！请选择一个线要素！");
                        return;
                    }

                    pw.AddProcessMessage(20, "生成待处理的线要素");

                    // 复制要素【去掉Z值】
                    Arcpy.CopyFeatures(featureLayer.Name, @$"{defGDBPath}\{featureLayer.Name}_closed", true);
                    // 获取复制后的要素
                    using Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(defGDBPath)));
                    FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>($"{featureLayer.Name}_closed");

                    pw.AddProcessMessage(20, "执行线闭合");
                    // 获取要素图层的编辑操作
                    var editOperation = new EditOperation();
                    using (RowCursor rowCursor = featureClass.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Feature feature = rowCursor.Current as Feature)      // 获取要素
                            {
                                // 获取要素的几何对象
                                Polyline polyline = feature.GetShape() as Polyline;
                                // 获取线要素的起点和终点
                                MapPoint start_point = polyline.Points.First();
                                MapPoint end_point = polyline.Points.Last();

                                // 如果起始点和终止点的坐标不同，即线不闭合，则进行线闭合操作
                                if (!start_point.Coordinate2D.Equals(end_point.Coordinate2D))
                                {
                                    List<Coordinate2D> pts = new List<Coordinate2D>();
                                    // 将第一个部分闭合
                                    foreach (var part in polyline.Points)
                                    {
                                        pts.Add(part.Coordinate2D);
                                    }
                                    pts.Add(start_point.Coordinate2D);
                                    // 创建 PolylineBuilder 对象并闭合线要素
                                    var builder = new PolylineBuilder(pts);
                                    // 获取闭合后的几何
                                    var closedGeometry = builder.ToGeometry();

                                    // 设置要素的几何
                                    feature.SetShape(closedGeometry);
                                }
                                feature.Store();
                            }
                        }
                    }
                    // 执行编辑
                    editOperation.Execute();

                    pw.AddProcessMessage(60, time_base, "刷新地图视图");

                    // 刷新地图视图
                    MapView.Active.ZoomInFixed();

                    pw.AddProcessMessage(20, time_base, "工具运行完成！！！", Brushes.Blue);
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
