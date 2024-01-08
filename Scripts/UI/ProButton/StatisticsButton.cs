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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.UI.ProButton
{
    internal class StatisticsButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                // 获取活动地图视图中选定的要素集合
                var selectedSet = MapView.Active.Map.GetSelection();

                // 将选定的要素集合转换为字典形式
                var selectedList = selectedSet.ToDictionary();

                // 创建一个新的 Inspector 对象以检索要素属性
                var inspector = new Inspector();

                // 初始化变量以存储面要素的数量和总面积
                int polygonCount = 0;
                double polygonArea = 0;

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

                            // 增加面要素的数量
                            polygonCount++;
                        }
                    }
                }

                // 定义一个自定义的面积单位 'mu'（常用于土地面积）
                var areaMu = AreaUnit.CreateAreaUnit("mu", 10000.0 / 15.0);

                // 将面积转换为公顷并四舍五入保留 4 位小数
                double hectares = Math.Round(AreaUnit.SquareMeters.ConvertTo(polygonArea, AreaUnit.Hectares), 4);

                // 将面积转换为 'mu' 并四舍五入保留 4 位小数
                double areaMuValue = Math.Round(AreaUnit.SquareMeters.ConvertTo(polygonArea, areaMu), 4);

                // 显示包含总结信息的消息框
                MessageBox.Show("面要素数量：" + polygonCount + "\n" +
                    "总面积（亩）：" + areaMuValue + " 亩" + "\n" +
                    "       （公顷）：" + hectares + " 公顷" + "\n" +
                    "    （平方米）：" + Math.Round(polygonArea, 2) + " m²");
            });
        }
    }
}
