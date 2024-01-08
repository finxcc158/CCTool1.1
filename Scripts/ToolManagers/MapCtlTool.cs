using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.ToolManagers
{
    public class MapCtlTool
    {
        // 图层（组）显示控制【选定显示，其它关闭】
        public static void ControlLayer(string lyName)
        {
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().ToList();
            // 获取输入的指定图层
            Layer initLayer = map.GetLayersAsFlattenedList().FirstOrDefault(item => item.Name.Equals(lyName));

            foreach (var ly in lys)
            {
                // 如果是指定的图层，或指定图层的子图层，就显示
                if (ly.Name == lyName || ly.Parent == initLayer)
                {
                    ly.SetVisibility(true);
                    ControlFatherLayer(ly, true);
                }
                else
                {
                    ly.SetVisibility(false);
                }
            }
        }

        // 设置图层的父图层显示状态
        public static void ControlFatherLayer(Layer ly, bool isVisible)
        {
            var fatherLayer = ly.Parent;
            if (fatherLayer is GroupLayer)
            {
                GroupLayer pLayer = (GroupLayer)fatherLayer;
                pLayer.SetVisibility(isVisible);
                ControlFatherLayer(pLayer, isVisible);
            }
        }

        // 图层显示控制
        public static void ControlLayerVisible(string lyName)
        {
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().ToList();
            // 获取输入的指定图层
            var initLayer = map.GetLayersAsFlattenedList().FirstOrDefault(item => item.Name.Equals(lyName));

            foreach (var ly in lys)
            {
                // 如果是指定的图层，或指定图层的子图层，就显示
                if (ly.Name == lyName || ly.Parent == initLayer)
                {
                    ly.SetVisibility(true);
                    ControlFatherLayer(ly, true);
                }
            }
        }

        // 获取当前所有图层显示信息
        public static Dictionary<string, bool> GetLayerVisible()
        {
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().ToList();
            foreach (var ly in lys)
            {
                if (ly.Parent is Map)
                {
                    dic.Add(ly.Name, ly.IsVisible);
                }
                else
                {
                    dic.Add($"{ly.Parent}+++{ly.Name}", ly.IsVisible);
                }
            }
            return dic;
        }

        // 设置当前所有图层显示信息
        public static void SetLayerVisible(Dictionary<string, bool> dic)
        {
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().ToList();
            foreach (var ly in lys)
            {
                if (ly.Parent is Map)
                {
                    ly.SetVisibility(dic[ly.Name]);
                }
                else
                {
                    string paName = $"{ly.Parent}+++{ly.Name}";
                    ly.SetVisibility(dic[paName]);
                }
            }
        }

        // 在当前地图中加载图层
        public static void AddFeatureLayerToMap(string LayerPath)
        {
            // 获取当前地图
            var map = MapView.Active.Map;
            // 加载图层
            Uri uri = new(LayerPath);
            LayerFactory.Instance.CreateLayer(uri, map);
        }

        // 从【SelectionSet】中获取最上层的【FeatureLayer】
        public static FeatureLayer GetFirstLayerFromSelectionSet(SelectionSet selectionSet)
        {
            FeatureLayer featureLayer = null;

            var mapView = MapView.Active;

            // 将选定的要素集合转换为字典形式
            var selectedList = selectionSet.ToDictionary();

            // 获取选定要素中位于最上层图层的要素
            int first = 1000;
            foreach (var layer in selectedList)
            {
                // 获取图层
                FeatureLayer featureLayer1 = layer.Key as FeatureLayer;
                int ss = mapView.Map.Layers.IndexOf(featureLayer1);
                if (ss < first)
                {
                    first = ss;    // 如果比较小，就更新first的值。
                }
            }

            // 获取所有图层
            var allLayers = MapView.Active.Map.GetLayersAsFlattenedList();

            // 获取最上层指定图层
            foreach (var layer in allLayers)
            {
                FeatureLayer ly = layer as FeatureLayer;
                int updata_ss = mapView.Map.Layers.IndexOf(ly);
                if (updata_ss == first)
                {
                    featureLayer = ly;
                }
            }

            // 返回值
            return featureLayer;
        }

        // 移除图层【按图层名】
        public static void RemoveLayer(string layer_name)
        {
            var map = MapView.Active.Map;
            Layer layer = map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == layer_name);
            map.RemoveLayer(layer);
        }
    }
}
