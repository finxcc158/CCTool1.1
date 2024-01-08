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
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using System.Windows.Forms;
using ArcGIS.Desktop.Mapping;
using Microsoft.Office.Core;
using ArcGIS.Core.Data.DDL;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;
using Row = ArcGIS.Core.Data.Row;
using ArcGIS.Desktop.Editing.Attributes;
using System.Security.Cryptography;
using ArcGIS.Desktop.Editing.Templates;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Net;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.IO.Path;
using ArcGIS.Desktop.Internal.Catalog.Wizards;
using ArcGIS.Desktop.Internal.Layouts.Utilities;
using System.Windows.Documents;
using ActiproSoftware.Windows;
using System.Windows;
using System.Runtime.InteropServices;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.GeoProcessing;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using ArcGIS.Core.Data.Exceptions;
using Table = ArcGIS.Core.Data.Table;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.Util;
using NPOI.SS.Formula.Functions;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Cells.Drawing;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.Util;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math.EC;
using ArcGIS.Core.Internal.CIM;
using Polyline = ArcGIS.Core.Geometry.Polyline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Org.BouncyCastle.Math.Primes;
using NPOI.HPSF;

namespace CCTool.Scripts.UI.ProButton
{
    internal class TestButton : Button
    {

        // 定义一个进度框
        private ProcessWindow processwindow = null;

        protected override void OnClick()
        {

            try
            {
                string out_path = @"c:\users\administrator\documents\arcgis\projects\test\test.gdb\demo_3_raste";  //输出的栅格路径，要替换
                var par = Geoprocessing.MakeValueArray("\"Extract_demo1\" + \"Extract_demo2\"", out_path);    // 表达式里的栅格图层要替换
                Geoprocessing.ExecuteToolAsync("sa.RasterCalculator", par);

                //var prj = Project.Current;
                //var map = MapView.Active;
                //string defGDB = Project.Current.DefaultGeodatabasePath;
                //string in_fc = "规划用地_CopyFeatures3";

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "进度");
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行工具…………" + time_base + "\r", Brushes.Green);


                //// 读取文件内容
                //string filePath = "C:\\ProSDKsettings\\Settings.txt";  // 替换为你的文件路径
                //string jsonContent = File.ReadAllText(filePath);

                //// 解析 JSON 数据
                //JObject jsonObject = JObject.Parse(jsonContent);

                //// 获取 "dpi" 属性的值
                //int dpiValue = (int)jsonObject["dpi"];
                //pw.AddMessage(dpiValue.ToString());

                //await QueuedTask.Run(() =>
                //{
                //    // 获取活动地图视图中选定的要素集合
                //    var selectedSet = MapView.Active.Map.GetSelection();

                //    // 将选定的要素集合转换为字典形式
                //    var selectedList = selectedSet.ToDictionary();
                //    // 创建一个新的 Inspector 对象以检索要素属性
                //    var inspector = new Inspector();

                //    // 遍历每个选定图层及其关联的对象 ID
                //    foreach (var layer in selectedList)
                //    {
                //        // 获取图层
                //        FeatureLayer featureLayer = layer.Key as FeatureLayer;
                //        // 获取图层和关联的对象 ID
                //        MapMember mapMember = layer.Key;
                //        List<long> oids = layer.Value;
                //        // 使用当前图层的第一个对象 ID 加载 Inspector
                //        inspector.Load(mapMember, oids[0]);
                //        // 获取选定要素的几何类型
                //        var geometryType = inspector.Shape.GeometryType;
                //        // 检查几何类型是否为面要素
                //        if (geometryType == GeometryType.Polygon)
                //        {
                //            // 遍历当前图层中的每个对象 ID
                //            foreach (var oid in oids)
                //            {
                //                // 使用当前对象 ID 加载 Inspector
                //                inspector.Load(mapMember, oid);
                //                // 将要素转换为多边形
                //                Polygon selectPolygon = inspector.Shape as Polygon;
                //                //selectPolygon = GeometryEngine.Instance.Buffer(polygon, 0.1) as Polygon;
                //            }
                //        }
                //    }

                //});

                pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }



    }
}

