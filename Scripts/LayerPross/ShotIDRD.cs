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
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CCTool.Scripts.UI.ProButton
{
    internal class ShotIDRD : Button
    {
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "从下至上从右至左排序";

        protected override async void OnClick()
        {
            try
            {
                var map = MapView.Active.Map;
                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;

                // 获取图层
                FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
                // 如果选择的不是面要素或是无选择，则返回
                if (ly == null)
                {
                    MessageBox.Show("错误！请选择一个要素类图层！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    // 获取图层要素的路径
                    string fc_path = ly.Name.LayerSourcePath();

                    pw.AddProcessMessage(10, "按空间位置排序【右下至左上】");
                    // 排序
                    Arcpy.Sort(ly, gdb + @"\sort_fc", "Shape ASCENDING", "LR");

                    pw.AddProcessMessage(50, time_base, "按空间位置排序【更新要素】");
                    // 更新要素
                    Arcpy.CopyFeatures(gdb + @"\sort_fc", fc_path, true);

                    pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
