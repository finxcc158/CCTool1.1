using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for SD2YDYH.xaml
    /// </summary>
    public partial class SD2YDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SD2YDYH()
        {
            InitializeComponent();
            combox_field_before.Items.Add("DLMC");
            combox_field_before.SelectedIndex= 0;

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "三调名称转用地用海名称";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string in_data = combox_fc.Text;
                string in_field = combox_field_before.Text;
                string map_field = combox_feild_after.Text;
                string version = combox_version.Text;

                // 判断参数是否选择完全
                if (in_data == "" || in_field == "" || map_field == "")
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
                    // 获取工程默认文件夹位置
                    var def_path = Project.Current.HomeFolderPath;
                    // 复制映射表
                    pw.AddMessage("复制映射表");
                    string excelName = "";
                    if (version == "旧版")
                    {
                        excelName = "三调用地名称_to_用地用海用地名称";
                    }
                    else
                    {
                        excelName = "三调用地名称_to_用地用海用地名称_新版";
                    }
                    string map_excel = $@"{def_path}\{excelName}.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", map_excel);

                    // 属性映射
                    pw.AddProcessMessage(10, time_base, "属性映射");
                    GisTool.AttributeMapper(in_data, in_field, map_field, map_excel + @"\sheet1$");

                    // 删除中间数据
                    pw.AddProcessMessage(50, time_base, "删除中间数据");
                    File.Delete(map_excel);

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
            try
            {
                UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_before);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
        }

        private void combox_af_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_feild_after);

        }
    }
}
