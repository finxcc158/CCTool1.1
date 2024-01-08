using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
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

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoLayer.xaml
    /// </summary>
    public partial class InfoLayer : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoLayer()
        {
            InitializeComponent();
            
        }

        private void form_loaded(object sender, RoutedEventArgs e)
        {
            // 更新地图标签
            string txt = lb.Content.ToString();
            Map map = MapView.Active.Map;
            string result = txt.Replace("Map", map.Name);

            lb.Content = result;

            // 获取所有要素图层
            List<string> featureLayers = map.AllFeatureLayers();
            // 获取所有独立表
            List<string> standaloneTables = map.AllStandaloneTables();

            // 更新要素图层信息
            if (featureLayers.Count>0)
            {
                tb_message.AddMessage($"要素图层\r", Brushes.Green);
                foreach (string layer in featureLayers)
                {
                    tb_message.AddMessage($"{layer}\r", Brushes.BlueViolet);
                }
            }
            // 更新独立表图层信息
            if (standaloneTables.Count > 0)
            {
                tb_message.AddMessage($"独立表图层\r", Brushes.Green);
                foreach (string layer in standaloneTables)
                {
                    tb_message.AddMessage($"{layer}\r", Brushes.BlueViolet);
                }
            }


        }

    }
}
