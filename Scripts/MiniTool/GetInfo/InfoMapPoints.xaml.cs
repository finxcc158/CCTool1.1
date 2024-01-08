using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
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
using System.Threading;
using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using MathNet.Numerics.LinearAlgebra.Factorization;
using ArcGIS.Core.Internal.CIM;
using CCTool.Scripts.UI.ProMapTool;

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoMapPoints.xaml
    /// </summary>
    public partial class InfoMapPoints : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoMapPoints()
        {
            InitializeComponent();
        }

        private void combox_layer_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_layer);

        }

        private async void combox_layer_DropClosed(object sender, EventArgs e)
        {
            // 清空文本
            tb_message.Document.Blocks.Clear();

            string lyName = combox_layer.Text;
            await QueuedTask.Run(() =>
            {
                // 获取目标FeatureLayer
                FeatureLayer featurelayer = lyName.TargetFeatureLayer();
                // 确保要素类的几何类型是多边形
                if (featurelayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                {
                    // 如果不是多边形类型，则输出错误信息并退出函数
                    MessageBox.Show("该要素类不是多边形类型。");
                    return;
                }

                tb_message.AddMessage($"共计【{featurelayer.GetFeatureCount()}】个图斑\r", Brushes.Black);

                // 遍历面要素类中的所有要素
                using (var cursor = featurelayer.Search())
                {
                    int featureCount = 1;
                    while (cursor.MoveNext())
                    {
                        using (var feature = cursor.Current as Feature)
                        {

                            // 获取要素的几何
                            ArcGIS.Core.Geometry.Polygon polygon = feature.GetShape() as ArcGIS.Core.Geometry.Polygon;

                            // 获取面要素的所有点（内外环）
                            List<List<MapPoint>> mapPoints = polygon.MapPointsFromPolygon();
                            // 获取每个环的最西北点
                            List<List<double>> NWPoints = polygon.NWPointsFromPolygon();
                            tb_message.AddMessage($"***********************************************\r", Brushes.Black);
                            tb_message.AddMessage($"第【{featureCount}】个图斑\r", Brushes.Gray);

                            // 每个环进行处理
                            for (int j = 0; j < mapPoints.Count; j++)
                            {
                                
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

                                // 判断是否是西北角
                                string isNW = "";
                                if (targetIndex == 0)
                                {
                                    isNW = "初始点在西北角";
                                }
                                else
                                {
                                    isNW = "初始点不在西北角";
                                }

                                // 判断顺逆时针，如果有问题就调整反向
                                string clock = "";
                                bool isClockwise = vertices.IsColckwise();
                                if (!isClockwise)
                                {
                                    clock = "逆时针";
                                }
                                if (isClockwise)
                                {
                                    clock = "顺时针";
                                }

                                // 判断内外环
                                string isYH = "";
                                if (j == 0)
                                {
                                    isYH = "外环";
                                }
                                else
                                {
                                    isYH = $"内环{j}";
                                }

                                // 在末尾加起始点
                                vertices.Add(vertices[0]);
                                if (j>0)
                                {
                                    tb_message.AddMessage($"---------------------------------------------\r", Brushes.Black);
                                }
                                tb_message.AddMessage($"{isYH}      {clock}     {isNW}\r", Brushes.Green);

                                // 获取点信息
                                for (int i = 0; i < vertices.Count; i++)
                                {
                                    tb_message.AddMessage($"{i+1},      {vertices[i].X},     {vertices[i].Y}\r", Brushes.BlueViolet);
                                }
                            }

                        }
                        featureCount++;
                    }
                }
            });
        }

    }
}
