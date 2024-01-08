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

namespace CCTool.Scripts.UI.ProButton
{
    internal class MapVisibleLayer : Button
    {
        protected override async void OnClick()
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    // 获取当前Map
                    Map map = MapView.Active.Map;
                    // 获取Map中的所有Layer
                    var layers = map.GetLayersAsFlattenedList();
                    // 显示所有图层
                    if (layers.Count > 0)
                    {
                        foreach (var layer in layers)
                        {
                            layer.SetVisibility(true);
                        }
                    }
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }
        }
    }
}
