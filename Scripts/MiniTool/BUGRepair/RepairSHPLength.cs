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
using Microsoft.Win32;

namespace CCTool.Scripts.MiniTool.BUGRepair
{
    internal class RepairSHPLength : Button
    {
        protected override void OnClick()
        {
            // arcgis版本
            List<string> arcgisVerssions = new List<string>()
            {
                "ArcGISPro","Desktop10.2","Desktop10.3","Desktop10.4","Desktop10.5","Desktop10.6","Desktop10.7","Desktop10.8"
            };
            
            // 定义键名
            string valueName = "dbfDefault";
            string defaultValue = "936";

            foreach (string ver in arcgisVerssions)
            {
                // 定义注册表路径
                string registryPath = $@"Software\ESRI\{ver}\Common\CodePage";

                // 检查注册表中是否存在指定路径
                RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true);

                // 如果路径不存在，则创建路径
                key ??= Registry.CurrentUser.CreateSubKey(registryPath, true);

                // 设置或创建字符串值"dbfDefault"并设置其值为"936"
                key.SetValue(valueName, defaultValue, RegistryValueKind.String);

                // 关闭注册表键
                key.Close();
            }

            // 弹出提示框
            MessageBox.Show("SHP要素【字段名长度】修改为10(5个汉字)！\r所有版本的ArcGIS和ArcGIS Pro都适用。");
        }
    }
}
