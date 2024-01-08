using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Polygon = ArcGIS.Core.Geometry.Polygon;

namespace CCTool.Scripts.DataPross.Excel
{
    /// <summary>
    /// Interaction logic for SetBoundaryPointsFromNW.xaml
    /// </summary>
    public partial class SetBoundaryPointsFromNW : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SetBoundaryPointsFromNW()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "面要素导出界址点要素";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string in_fc = combox_fc.Text;

                // 判断参数是否选择完全
                if (in_fc == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                Close();
                await QueuedTask.Run(() =>
                {
                    pw.AddMessage("获取目标FeatureLayer");
                    // 获取目标FeatureLayer
                    FeatureLayer featurelayer = in_fc.TargetFeatureLayer();
                    // 确保要素类的几何类型是多边形
                    if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                    {
                        // 如果不是多边形类型，则输出错误信息并退出函数
                        MessageBox.Show("该要素类不是多边形类型。");
                        return;
                    }
                    pw.AddProcessMessage(20, time_base, $"处理要素，按西北角起始，顺时针重排");
                    // 遍历面要素类中的所有要素
                    RowCursor cursor = featurelayer.Search();
                    while (cursor.MoveNext())
                    {
                        using var feature = cursor.Current as Feature;
                        // 获取要素的几何
                        ArcGIS.Core.Geometry.Polygon geometry = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;
                        if (geometry != null)
                        {
                            // 面要素的所有折点进行重排【按西北角起始，顺时针重排】
                            Polygon resultPolygon  = geometry.ReshotMapPointReturnPolygon();
                            // 重新设置要素并保存
                            feature.SetShape(resultPolygon);
                            feature.Store();
                        }
                    }
                });

                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
