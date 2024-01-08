using ArcGIS.Desktop.Framework.Threading.Tasks;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for AttributeMapper.xaml
    /// </summary>
    public partial class AttributeMapper : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AttributeMapper()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "属性映射";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_fc);
        }


        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开Excel文件
            string path = UITool.OpenTable();
            // 将Excel文件的路径置入【textExcelPath】
            textExcelPath.Text = path;
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string map_tabel = textExcelPath.Text;
                string in_data = combox_fc.Text;
                string in_field = combox_field_before.Text;
                string map_field = combox_feild_after.Text;

                // 判断参数是否选择完全
                if (map_tabel == "" || in_data == "" || in_field == "" || map_field == "")
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
                    pw.AddProcessMessage(10, "处理数据");
                    pw.AddProcessMessage(10, time_base, "属性映射");
                    GisTool.AttributeMapper(in_data, in_field, map_field, map_tabel);
                    pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_be_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_before);
        }

        private void combox_af_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_feild_after);
        }
    }
}
