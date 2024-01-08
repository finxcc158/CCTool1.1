using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.OpenXmlFormats.Vml;
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

namespace CCTool.Scripts.GHApp.YDYH
{
    /// <summary>
    /// Interaction logic for YDYHOld2New.xaml
    /// </summary>
    public partial class YDYHOld2New : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public YDYHOld2New()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "用地用海旧转新";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private void combox_field_bm_old_DropDown(object sender, EventArgs e)
        {
            string fc = combox_fc.Text;
            UITool.AddTextFieldsToCombox(fc, combox_field_bm_old);
        }

        private void combox_field_bm_new_DropDown(object sender, EventArgs e)
        {
            string fc = combox_fc.Text;
            UITool.AddTextFieldsToCombox(fc, combox_field_bm_new);
        }

        private void combox_field_mc_new_DropDown(object sender, EventArgs e)
        {
            string fc = combox_fc.Text;
            UITool.AddTextFieldsToCombox(fc, combox_field_mc_new);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            // 获取指标
            string fc = combox_fc.Text;
            string oldBM = combox_field_bm_old.Text;
            string newBM = combox_field_bm_new.Text;
            string newMC = combox_field_mc_new.Text;

            // 判断参数是否选择完全
            if (fc == "" || oldBM == "" || newBM == "")
            {
                MessageBox.Show("有必选参数为空！！！");
                return;
            }

            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                pw.AddProcessMessage(10, "获取参数");

                Close();
                // 异步执行
                await QueuedTask.Run(() =>
                {
                    string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
                    string excelName = "旧用地用海编码_to_新用地用海编码";
                    string excelName2 = "新版用地用海_DM_to_MC";

                    pw.AddProcessMessage(30, time_base, "新旧编码属性映射");

                    string output_excel = $@"{def_folder}\{excelName}.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", output_excel);

                    // 新旧编码属性映射
                    GisTool.AttributeMapper(fc, oldBM, newBM, output_excel + @"\sheet1$");

                    pw.AddProcessMessage(30, time_base, "新编码名称属性映射");

                    string output_excel2 = $@"{def_folder}\{excelName2}.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName2}.xlsx", output_excel2);

                    // 新旧编码属性映射
                    GisTool.AttributeMapper(fc, newBM, newMC, output_excel2 + @"\sheet1$");

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
