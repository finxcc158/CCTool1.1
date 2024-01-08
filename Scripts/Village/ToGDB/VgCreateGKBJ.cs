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
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CCTool.Scripts.UI.ProButton
{
    internal class VgCreateGKBJ : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "7、创建管控边界";

        protected override async void OnClick()
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("执行" + tool_name + "工具…………" + time_base + "\rSTART!", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    // 获取村庄名称列表
                    List<string> village_names = VG.GetVillageNames();
                    // 处理每个村庄
                    foreach (var village_name in village_names)
                    {
                        pw.AddProcessMessage(10, time_base, "【" + village_name + "】\r");
                        // 创建文件目录
                        VG.CreateTarget(village_name, "MBNGH");
                        // 创建村级调查区
                        VG.CreateGKBJ(village_name, pw, time_base, true);
                    }
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
