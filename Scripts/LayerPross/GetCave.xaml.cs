using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for GetCave.xaml
    /// </summary>
    public partial class GetCave : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GetCave()
        {
            InitializeComponent();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            // 获取默认数据库
            var gdb = Project.Current.DefaultGeodatabasePath;
            string output_extent = gdb + @"\处理结果_边界";
            string output_cave = gdb + @"\处理结果_空洞";
            // 获取输出模式
            bool extent = (bool)check_getExtent.IsChecked;
            bool cave = (bool)check_getCave.IsChecked;

            // 获取当前地图
            var map = MapView.Active.Map;
            // 获取图层
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            // 如果选择的不是面要素或是无选择，则返回
            if (ly.ShapeType != esriGeometryType.esriGeometryPolygon || ly == null)
            {
                MessageBox.Show("错误！请选择一个面要素！");
                return;
            }

            Close();

            try
            {
                await QueuedTask.Run(() =>
                {
                    if (extent)   // 获取边界
                    {
                        GisTool.GetCave(ly.Name, output_extent, "外边界");
                        MapCtlTool.AddFeatureLayerToMap(output_extent);
                    }
                    if (cave)   // 获取空洞
                    {
                        GisTool.GetCave(ly.Name, output_cave);
                        MapCtlTool.AddFeatureLayerToMap(output_cave);
                    }
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message+ee.StackTrace);
                return;
            }
        }

    }
}
