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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.UI.ProMapTool
{
    internal class FeatureCounter : MapTool
    {
        public FeatureCounter()
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
            var mv = MapView.Active;
            var identifyResult = await QueuedTask.Run(() =>
            {
                var sb = new StringBuilder();

                // 获取选择的要素
                var features = mv.GetFeatures(geometry);

                // 获取所有的要素图层
                var lyrs = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                foreach (var lyr in lyrs)
                {
                    // 所选要素在图层中的数量
                    var fCnt = features.ToDictionary().ContainsKey(lyr) ? features[lyr].Count : 0;
                    // 如果有要素被选中，就标记出来
                    if (fCnt>0)
                    {
                        sb.AppendLine($@"【{lyr.Name}】中有({fCnt})个要素被选中。");
                    }
                }
                return sb.ToString();
            });
            MessageBox.Show(identifyResult);
            return true;
        }
    }
}
