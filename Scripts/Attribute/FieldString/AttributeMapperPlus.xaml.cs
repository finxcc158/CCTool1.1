using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Locate;
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
using Field = ArcGIS.Core.Data.Field;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for AttributeMapperPlus.xaml
    /// </summary>
    public partial class AttributeMapperPlus : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AttributeMapperPlus()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "属性映射(批量)";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string gdb_path = textGDBPath.Text;
                string map_tabel = textExcelPath.Text;
                string in_field = text_field_before.Text;
                string map_field = text_field_after.Text;

                // 判断参数是否选择完全
                if (map_tabel == "" || in_field == "" || map_field == "" || gdb_path == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                await QueuedTask.Run(() =>
                {
                    pw.AddMessage("获取所有要素类及表格");
                    List<string> list_table = gdb_path.GetFeatureClassAndTablePath();
                    foreach (var table in list_table)
                    {
                        pw.AddProcessMessage(10, time_base, $"处理:{table}", Brushes.Gray);
                        GisTool.AttributeMapper(table, in_field, map_field, map_tabel);
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

        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text = UITool.OpenDialogGDB();
        }

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开Excel文件
            string path = UITool.OpenTable();
            // 将Excel文件的路径置入【textExcelPath】
            textExcelPath.Text = path;
        }
    }
}
