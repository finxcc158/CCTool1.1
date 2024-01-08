using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    /// <summary>
    /// Interaction logic for ClearFeatureClassZM.xaml
    /// </summary>
    public partial class ClearFeatureClassZM : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ClearFeatureClassZM()
        {
            InitializeComponent();
            UITool.AddFeatureLayersToListbox(listbox_fc);
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "清除要素的ZM值";

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 判断参数是否选择完全
                if (listbox_fc.Items.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                // 获取要素列表
                List<string> list_fc = new List<string>();
                foreach (CheckBox item in listbox_fc.Items)
                {
                    if (item.IsChecked == true)
                    {
                        list_fc.Add(item.Content.ToString());
                    }
                }

                await QueuedTask.Run(() =>
                {
                    pw.AddMessage("获取要素列表");

                    foreach (var fc in list_fc)
                    {
                        // 输出路径
                        string outPath = "";
                        // 获取所选图层的所有字段
                        var fcPath = fc.LayerSourcePath();
                        // 如果是shp数据
                        if (fcPath.Contains(".shp"))
                        {
                            outPath = fcPath.Replace(".shp", "_ClearZM.shp");
                        }
                        // 如果是GDB数据
                        else if (fcPath.Contains(".gdb"))
                        {
                            outPath = fcPath + "_ClearZM";
                        }
                        // 其它情况不考虑
                        else
                        {
                            MessageBox.Show("不是shp或gdb数据，不符合要求！");
                        }

                        pw.AddProcessMessage(20, time_base, $"复制要素【{fc}】至: {outPath}");

                        // 复制要素
                        Arcpy.CopyFeatures(fcPath, outPath);
                    }
                });
                pw.AddProcessMessage(80, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
