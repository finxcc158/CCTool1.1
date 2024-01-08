using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CCTool.Scripts.Manager;
using System.IO;
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts
{
    /// <summary>
    /// Interaction logic for YDYHChanger.xaml
    /// </summary>
    public partial class YDYHChanger : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public YDYHChanger()
        {
            InitializeComponent();
            // combox_model框中添加2种转换模式，默认【代码转名称】
            combox_model.Items.Add("代码转名称");
            combox_model.Items.Add("名称转代码");
            combox_model.SelectedIndex = 0;

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "用地用海代码和名称转换";

        private void combox_field_before_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_before);
        }

        private void combox_field_after_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_after);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        // 执行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string field_before = combox_field_before.Text;
                string field_after = combox_field_after.Text;
                string model = combox_model.Text;
                string version = combox_version.Text;

                // 判断参数是否选择完全
                if (fc_path == "" || field_before == "" || field_after == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                this.Close();

                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, model);

                    string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
                    string excelName = "";
                    if (version=="旧版")
                    {
                        excelName = "用地用海_DM_to_MC";
                    }
                    else
                    {
                        excelName = "新版用地用海_DM_to_MC";
                    }
                    string output_excel = $@"{def_folder}\{excelName}.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", output_excel);

                    // 转换模式
                    bool reserve = false;
                    if (model == "名称转代码") { reserve = true; }

                    pw.AddProcessMessage(10, time_base, "开始转换...");
                    // 用地用海编码名称互转
                    GisTool.AttributeMapper(fc_path, field_before, field_after, output_excel + @"\sheet1$", reserve);

                    pw.AddProcessMessage(60, time_base, "删除中间数据");
                    // 删除中间数据
                    File.Delete(output_excel);

                });
                pw.AddProcessMessage(90, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
