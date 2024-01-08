using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Net.WebRequestMethods;
using Geometry = ArcGIS.Core.Geometry.Geometry;

namespace CCTool.Scripts.UI.ProMapTool
{
    internal class TestMapTool : MapTool
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;

        public TestMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            // 打开进度框
            ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "进度");
            DateTime time_base = DateTime.Now;
            pw.AddMessage("开始执行工具…………" + time_base + "\r", Brushes.Green);

            await QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                if (mapView == null)
                    return true;

                // 获取选择的要素
                var results = mapView.GetFeatures(geometry);

                if (results is not null)
                {
                    mapView.FlashFeature(results);

                    // 将选定的要素集合转换为字典形式
                    var selectedList = results.ToDictionary();

                    // 获取选定要素中位于最上层图层的要素
                    int first = 1000;
                    foreach (var layer in selectedList)
                    {
                        // 获取图层
                        FeatureLayer featureLayer = layer.Key as FeatureLayer;
                        int ss = mapView.Map.Layers.IndexOf(featureLayer);
                        if (ss < first)
                        {
                            first = ss;    // 如果比较小，就更新first的值。
                        }
                    }
                    // 隐藏最上层的图层
                    foreach (var layer in selectedList)
                    {
                        // 获取图层
                        FeatureLayer featureLayer = layer.Key as FeatureLayer;
                        int updata_ss = mapView.Map.Layers.IndexOf(featureLayer);
                        if (updata_ss == first)
                        {
                            featureLayer.SetVisibility(false);
                        }
                    }
                }
                return true;
            });

            pw.AddProcessMessage(100, time_base, "工具执行完成！！", Brushes.Blue);
            return true;
        }
    }
}
