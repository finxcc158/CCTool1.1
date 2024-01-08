using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for AreaStatistics.xaml
    /// </summary>
    public partial class AreaStatistics : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AreaStatistics()
        {
            InitializeComponent();
            InitiArea();

            // 订阅地图选择更改事件
            MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
        }

        private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
        {
            // 当选择更改时，自动刷新面积计算
            InitiArea();
        }


        // 统计面积
        public async void InitiArea()
        {
            // 初始化变量以存储面要素的数量和各类面积指标
            int polygonCount = 0;
            double polygonArea = 0;
            double geoArea = 0;

            double hectares = 0;
            double geo_hectares = 0;
            double squKilometers = 0;
            double geo_squKilometers = 0;
            double areaMuValue = 0;
            double geo_areaMuValue = 0;
            // 立个Flag，面要素是否有坐标系
            bool has_geo = true;
            try
            {
                await QueuedTask.Run(() =>
                {
                    // 获取活动地图视图中选定的要素集合
                    var selectedSet = MapView.Active.Map.GetSelection();

                    // 将选定的要素集合转换为字典形式
                    var selectedList = selectedSet.ToDictionary();
                    // 创建一个新的 Inspector 对象以检索要素属性
                    var inspector = new Inspector();

                    // 遍历每个选定图层及其关联的对象 ID
                    foreach (var layer in selectedList)
                    {
                        // 获取图层和关联的对象 ID
                        MapMember mapMember = layer.Key;
                        List<long> oids = layer.Value;
                        // 使用当前图层的第一个对象 ID 加载 Inspector
                        inspector.Load(mapMember, oids[0]);
                        // 获取选定要素的几何类型
                        var geometryType = inspector.Shape.GeometryType;
                        // 检查几何类型是否为面要素
                        if (geometryType == GeometryType.Polygon)
                        {
                            // 遍历当前图层中的每个对象 ID
                            foreach (var oid in oids)
                            {
                                // 使用当前对象 ID 加载 Inspector
                                inspector.Load(mapMember, oid);
                                // 将要素转换为多边形
                                var polygon = inspector.Shape as Polygon;
                                // 计算并累加多边形的面积
                                polygonArea += Math.Abs(polygon.Area);

                                if (has_geo)       // 如果坐标系还都正确的情况下
                                {
                                    //获取面要素的坐标系
                                    var sr2 = polygon.SpatialReference;
                                    if (sr2.Name == "Unknown") { has_geo = false; }        // 如果出现不正确的坐标系，后面就不要计算了
                                    else { geoArea += Math.Abs(GeometryEngine.Instance.GeodesicArea(polygon)); }       // 否则，计算椭球面积
                                }
                                // 增加面要素的数量
                                polygonCount++;
                            }
                        }
                    }

                    // 定义一个自定义的面积单位 'mu'（常用于土地面积）
                    var areaMu = AreaUnit.CreateAreaUnit("mu", 10000.0 / 15.0);
                    // 将面积转换为公顷并四舍五入保留 4 位小数
                    hectares = Math.Round(AreaUnit.SquareMeters.ConvertTo(polygonArea, AreaUnit.Hectares), 4);
                    // 将面积转换为平方公里并四舍五入保留 4 位小数
                    squKilometers = Math.Round(AreaUnit.SquareMeters.ConvertTo(polygonArea, AreaUnit.SquareKilometers), 4);
                    // 将面积转换为 'mu' 并四舍五入保留 4 位小数
                    areaMuValue = Math.Round(AreaUnit.SquareMeters.ConvertTo(polygonArea, areaMu), 4);


                    if (has_geo)       // 如果都有正确的坐标系，就计算椭球面积
                    {
                        geo_hectares = Math.Round(AreaUnit.SquareMeters.ConvertTo(geoArea, AreaUnit.Hectares), 4);
                        geo_squKilometers = Math.Round(AreaUnit.SquareMeters.ConvertTo(geoArea, AreaUnit.SquareKilometers), 4);
                        geo_areaMuValue = Math.Round(AreaUnit.SquareMeters.ConvertTo(geoArea, areaMu), 4);
                    }
                });

                // 显示结果
                text_area_squ.Text = Math.Round(polygonArea, 2).ToString();
                text_area_ha.Text = hectares.ToString();
                text_area_km.Text = squKilometers.ToString();
                text_area_mu.Text = areaMuValue.ToString();
                // 默认先隐藏椭球面积的信息
                lb_1.Visibility = System.Windows.Visibility.Hidden;
                lb_2.Visibility = System.Windows.Visibility.Hidden;
                lb_3.Visibility = System.Windows.Visibility.Hidden;
                lb_4.Visibility = System.Windows.Visibility.Hidden;

                text_geoarea_ha.Visibility = System.Windows.Visibility.Hidden;
                text_geoarea_km.Visibility = System.Windows.Visibility.Hidden;
                text_geoarea_mu.Visibility = System.Windows.Visibility.Hidden;
                text_geoarea_squ.Visibility = System.Windows.Visibility.Hidden;


                lb_count.Content = "所选要素数量为：" + polygonCount.ToString();

                // 如果有椭球面积
                if (has_geo)
                {
                    text_geoarea_squ.Text = Math.Round(geoArea, 2).ToString();
                    text_geoarea_ha.Text = geo_hectares.ToString();
                    text_geoarea_km.Text = geo_squKilometers.ToString();
                    text_geoarea_mu.Text = geo_areaMuValue.ToString();
                    // 显示椭球面积的信息
                    lb_1.Visibility = System.Windows.Visibility.Visible;
                    lb_2.Visibility = System.Windows.Visibility.Visible;
                    lb_3.Visibility = System.Windows.Visibility.Visible;
                    lb_4.Visibility = System.Windows.Visibility.Visible;

                    text_geoarea_ha.Visibility = System.Windows.Visibility.Visible;
                    text_geoarea_km.Visibility = System.Windows.Visibility.Visible;
                    text_geoarea_mu.Visibility = System.Windows.Visibility.Visible;
                    text_geoarea_squ.Visibility = System.Windows.Visibility.Visible;
                    // 隐藏警告信息
                    lb_warning.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }


        // 当需要重新计算面积时，可以调用 Initialize 方法，它会调用计算面积方法并刷新窗口内容
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            InitiArea();
        }
    }
}
