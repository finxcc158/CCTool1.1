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
using ArcGIS.Core.Internal.CIM;
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
using CCTool.Scripts.ToolManagers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CCTool.Scripts.UI.ProButton
{
    internal class TestButton2 : Button
    {

        // 定义一个进度框
        private ProcessWindow processwindow = null;

        protected override async void OnClick()
        {

            try
            {
                var prj = Project.Current;
                var map = MapView.Active;
                string defGDB = Project.Current.DefaultGeodatabasePath;
                string copyly = defGDB + @"\line_copy";

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "进度");
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行工具…………" + time_base + "\r", Brushes.Green);


                await QueuedTask.Run(() =>
                {
                    // 文件路径
                    string filePath = @"C:\Users\Administrator\Desktop\lab.xlsx\new$";

                    // 创建文件流
                    FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    // 打开工作簿
                    XSSFWorkbook wb = new XSSFWorkbook(fs);
                    // 获取第一个工作表
                    ISheet sheet = wb.GetSheetAt(0);
                    IRow row = sheet.GetRow(6);
                    sheet.RemoveRowBreak(3);
                    // 获取第3行（索引从0开始）
                    ICell cell = sheet.GetRow(0).GetCell(0);
                    cell.SetCellValue("base");

                    // 保存工作簿
                    using FileStream saveFile = new FileStream(filePath, FileMode.Create);
                    wb.Write(saveFile);

                    //OfficeTool.ExcelWriteCell(filePath, 6, 6, "更新");
                });

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
