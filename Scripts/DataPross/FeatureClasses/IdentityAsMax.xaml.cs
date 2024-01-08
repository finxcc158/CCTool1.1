using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing.Controls;
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

namespace CCTool.Scripts.DataPross.FeatureClasses
{
    /// <summary>
    /// Interaction logic for IdentityAsMax.xaml
    /// </summary>
    public partial class IdentityAsMax : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public IdentityAsMax()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "按最大面积标识";

        private void combox_origin_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_origin_fc);
        }

        private void combox_identity_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_identity_fc);
        }

        private void combox_identity_fc_Closed(object sender, EventArgs e)
        {
            string lyName = combox_identity_fc.Text;
            UITool.AddTextFieldsToListBox(listbox_field, lyName);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取数据
                string origin_fc = combox_origin_fc.Text;
                string identity_fc = combox_identity_fc.Text;
                string output_fc = textFeatureClassPath.Text;
                List<string> list_fields = UITool.GetCheckboxStringFromListBox(listbox_field);

                string defGDB = Project.Current.DefaultGeodatabasePath;

                // 判断参数是否选择完全
                if (origin_fc == "" || identity_fc == "" || output_fc == "" || listbox_field.Items.Count <= 0)
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
                    pw.AddMessage("复制要素");
                    string fcName = output_fc[(output_fc.LastIndexOf(@"\")+1)..];

                    // 复制要素
                    string identityfile = Arcpy.CopyFeatures(origin_fc, output_fc);

                    pw.AddProcessMessage(10, time_base, "标识要素");
                    // 标识要素
                    string identityfile_2 = Arcpy.Identity(identityfile, identity_fc, $@"{defGDB}\identityfile_2");

                    pw.AddProcessMessage(10, time_base, "排序");
                    // 排序
                    string cal_txt = $"FID_{fcName} ASCENDING;Shape_Area DESCENDING";
                    string sort_1= Arcpy.Sort(identityfile_2,$@"{defGDB}\sort_1", cal_txt, "UR");

                    pw.AddProcessMessage(10, time_base, "添加字段");
                    // 添加字段
                    Arcpy.AddField(sort_1, "筛选", "TEXT");

                    pw.AddProcessMessage(10, time_base, "计算字段");
                    // 计算字段
                    Arcpy.CalculateField(sort_1, "筛选", $"ss(!FID_{fcName}!)", "bs = \"\"\r\ndef ss(a):\r\n    global bs\r\n    if a!=bs:\r\n        bs=a\r\n        return \"留下\"\r\n    else:\r\n        return \"删除\"");

                    pw.AddProcessMessage(10, time_base, "选择");
                    // 选择
                    string sort_2 = Arcpy.Select(sort_1, $@"{defGDB}\sort_2", "筛选 = '留下'");

                    pw.AddProcessMessage(10, time_base, "连接字段");
                    // 连接字段
                    string objField = GisTool.GetIDFieldNameFromTarget(identityfile);
                    Arcpy.JoinField(identityfile, objField, sort_2, $"FID_{fcName}", list_fields,true);

                    pw.AddProcessMessage(10, time_base, "删除中间要素");
                    // 删除中间要素
                    List<string> list_fc = new List<string>() { "identityfile_2", "sort_1", "sort_2" };
                    foreach (var fc in list_fc)
                    {
                        Arcpy.Delect(defGDB + @"\" + fc);
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

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            UITool.SelectAllItems(listbox_field);
        }

        private void btn_unSelect_Click(object sender, RoutedEventArgs e)
        {
            UITool.UnSelectAllItems(listbox_field);
        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            textFeatureClassPath.Text = UITool.SaveDialogFeatureClass();
        }
    }
}
