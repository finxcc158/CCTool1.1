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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    /// <summary>
    /// Interaction logic for AdjustmentsTool.xaml
    /// </summary>
    public partial class AdjustmentsTool : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AdjustmentsTool()
        {
            InitializeComponent();
            // 初始化combox
            combox_unit.Items.Add("平方米");
            combox_unit.Items.Add("公顷");
            combox_unit.Items.Add("平方公里");
            combox_unit.Items.Add("亩");
            combox_unit.SelectedIndex = 0;

            combox_areaType.Items.Add("投影面积");
            combox_areaType.Items.Add("图斑面积");
            combox_areaType.SelectedIndex = 1;

            combox_digit.Items.Add("1");
            combox_digit.Items.Add("2");
            combox_digit.Items.Add("3");
            combox_digit.Items.Add("4");
            combox_digit.Items.Add("5");
            combox_digit.Items.Add("6");
            combox_digit.SelectedIndex = 1;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "面积平差";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToCombox(combox_fc.Text, combox_field);
        }

        private void combox_land_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_land);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string fc_field = combox_field.Text;
                string land_path = combox_land.Text;
                string area_type = combox_areaType.Text[..2];
                string unit = combox_unit.Text;
                int digit = int.Parse(combox_digit.Text);

                bool isFieldOpen = (bool)check_fd.IsChecked;
                string areaField = combox_areaField.Text;

                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || fc_field == "" || land_path == "" || area_type == "" || unit == "" || combox_digit.Text == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);

                Close();
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, time_base, "平差计算", Brushes.Gray);
                    // 获取原始字段
                    List<string> fieldList = GisTool.GetFieldsNameFromTarget(fc_path);

                    string resultLayer = "";

                    if (isFieldOpen == true && areaField is not null && areaField!="")
                    {
                        resultLayer = GisTool.Adjustment(fc_path, land_path, gdb_path + @"\Adjustment", area_type, unit, digit, areaField);
                    }
                    else
                    {
                        // 平差计算
                        resultLayer = GisTool.Adjustment(fc_path, land_path, gdb_path + @"\Adjustment", area_type, unit, digit);
                    }

                    pw.AddProcessMessage(50, time_base, "结果赋值", Brushes.Gray);
                    // 字段赋值
                    Arcpy.CalculateField(resultLayer, fc_field, $"!{area_type}!");

                    pw.AddProcessMessage(20, time_base, "覆盖图层", Brushes.Gray);
                    // 返回覆盖图层
                    string fcFullPath = fc_path.LayerSourcePath();
                    Arcpy.CopyFeatures(resultLayer, fcFullPath, true);

                    pw.AddProcessMessage(20, time_base, "删除中间字段", Brushes.Gray);
                    // 删除中间字段
                    string fields = "";
                    foreach (var field in fieldList)
                    {
                        fields += field + ";";
                    }
                    string re = fields.Substring(0, fields.Length - 1);
                    Arcpy.DeleteField(fcFullPath, re, "KEEP_FIELDS");
                });
                pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_areaField_DropOpen(object sender, EventArgs e)
        {
            string fd = combox_fc.Text;
            if (fd is not null && fd!="")
            {
                UITool.AddFloatFieldsToCombox(fd, combox_areaField);
            }
        }

        private void check_fd_Uncheck(object sender, RoutedEventArgs e)
        {
            combox_areaField.IsEnabled= false;
        }

        private void check_fd_Check(object sender, RoutedEventArgs e)
        {
            combox_areaField.IsEnabled = true;
        }

    }
}
