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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CCTool.Scripts.UI.ProButton
{
    internal class VgCreateAll : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "村规_入库汇总";

        protected override async void OnClick()
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\rSTART!", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    // 获取村庄名称列表
                    List<string> village_names = VG.GetVillageNames();
                    // 处理每个村庄
                    foreach (var village_name in village_names)
                    {
                        pw.AddProcessMessage(10, time_base, "【" + village_name + "】\r");
                        // 创建文件目录
                        List<string> databases = new List<string>() {"JJYZQ", "JQXZ", "MBNGH", "" }; 
                        foreach (var database in databases)
                        {
                            VG.CreateTarget(village_name, database);
                        }
                        // 1、创建村级调查区
                        pw.AddMessage("1、创建村级调查区", Brushes.Gray);
                        VG.CreateCJDCQ(village_name, pw, time_base);
                        // 2、创建村级调查区界线
                        pw.AddProcessMessage(10, time_base, "2、创建村级调查区界线", Brushes.Gray);
                        VG.CreateCJDCQJX(village_name, pw, time_base);
                        // 3、创建基期地类图斑
                        pw.AddProcessMessage(10, time_base, "3、创建基期地类图斑", Brushes.Gray);
                        VG.CreateJQDLTB(village_name, pw, time_base);
                        // 4、创建现状公服设施点
                        pw.AddProcessMessage(10, time_base, "4、创建现状公服设施点", Brushes.Gray);
                        VG.CreateGGJCSSD(village_name, pw, time_base);
                        // 5、创建规划地类图斑
                        pw.AddProcessMessage(10, time_base, "5、创建规划地类图斑", Brushes.Gray);
                        VG.CreateGHDLTB(village_name, pw, time_base);
                        // 6、创建规划公服设施点
                        pw.AddProcessMessage(10, time_base, "6、创建规划公服设施点", Brushes.Gray);
                        VG.CreateGHGGJCSSD(village_name, pw, time_base);
                        // 7、创建管控边界
                        pw.AddProcessMessage(10, time_base, "7、创建管控边界", Brushes.Gray);
                        VG.CreateGKBJ(village_name, pw, time_base);
                        // 8、创建历史文化保护区
                        pw.AddProcessMessage(10, time_base, "8、创建历史文化保护区", Brushes.Gray);
                        VG.CreateLSWHBHQ(village_name, pw, time_base);
                        // 9、创建空间功能结构调整表
                        pw.AddProcessMessage(10, time_base, "9、创建空间功能结构调整表", Brushes.Gray);
                        VG.CreateKJGNJGTZB(village_name, pw, time_base);
                        // 10、创建规划指标表
                        pw.AddProcessMessage(10, time_base, "10、创建规划指标表", Brushes.Gray);
                        VG.CreateGHZBB(village_name, pw, time_base);
                    }
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
