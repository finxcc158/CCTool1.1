using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts.UI.ProMapTool
{
	internal class KeepLayer : MapTool
	{
        public KeepLayer()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active) 
        {
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                if (mapView == null)
                    return true;

                // 获取所有图层
                var allLayers = MapView.Active.Map.GetLayersAsFlattenedList();
                // 图层全部隐藏
                foreach (var layer in allLayers)
                {
                    layer.SetVisibility(false);
                }

                // 获取选择的要素
                SelectionSet results = mapView.GetFeatures(geometry);

                if (results is not null)
                {
                    // 获取选定要素中位于最上层图层的要素
                    FeatureLayer featureLayer = MapCtlTool.GetFirstLayerFromSelectionSet(results);
                    // 显示图层
                    featureLayer.SetVisibility(true);
                }
                return true;
            });
        }
    }
}