using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
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
    /// Interaction logic for StatisticsSDDL.xaml
    /// </summary>
    public partial class StatisticsSDDL : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsSDDL()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "统计三调地类";

        private void combox_field_BM_DropDown(object sender, EventArgs e)
        {
            UITool.AddFloatFieldsToCombox(combox_fc.Text, combox_field_BM);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private void openTableButton_Click(object sender, RoutedEventArgs e)
        {
            textTablePath.Text = UITool.SaveDialogExcel();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string bm_field = combox_field_BM.Text;
                string excel_path = textTablePath.Text;
                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;     

                // 判断参数是否选择完全
                if (fc_path == "" || bm_field == "" || excel_path == "")
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
                    pw.AddProcessMessage(20, "复制Excel模板");

                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】三调现状分类汇总表.xlsx", excel_path);
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调用地自转换.xlsx", folder_path + @"\三调用地自转换.xlsx");

                    // 复制要素
                    Arcpy.CopyFeatures(fc_path, gdb_path + @"\fc_sd");

                    // 添加一个类一、二、三、四级字段
                    Arcpy.AddField(gdb_path + @"\fc_sd", "mc_1", "TEXT");
                    Arcpy.AddField(gdb_path + @"\fc_sd", "mc_2", "TEXT");
                    Arcpy.AddField(gdb_path + @"\fc_sd", "mc_3", "TEXT");
                    Arcpy.AddField(gdb_path + @"\fc_sd", "mc_4", "TEXT");
                    // 用地名称转大类
                    pw.AddProcessMessage(5, time_base, "一级类转换");
                    GisTool.AttributeMapper(gdb_path + @"\fc_sd", "DLMC", "mc_1", folder_path + @"\三调用地自转换.xlsx\一级$");
                    pw.AddProcessMessage(5, time_base, "二级类转换");
                    GisTool.AttributeMapper(gdb_path + @"\fc_sd", "DLMC", "mc_2", folder_path + @"\三调用地自转换.xlsx\二级$");
                    pw.AddProcessMessage(5, time_base, "三级类转换");
                    GisTool.AttributeMapper(gdb_path + @"\fc_sd", "DLMC", "mc_3", folder_path + @"\三调用地自转换.xlsx\三级$");
                    pw.AddProcessMessage(5, time_base, "四级类转换");
                    GisTool.AttributeMapper(gdb_path + @"\fc_sd", "DLMC", "mc_4", folder_path + @"\三调用地自转换.xlsx\四级$");

                    pw.AddProcessMessage(10, time_base, "汇总用地指标");

                    // 汇总大、中类
                    List<string> list_bm = new List<string>() {"mc_1", "mc_2", "mc_3", "mc_4" };
                    GisTool.MultiStatistics(gdb_path + @"\fc_sd", gdb_path + @"\statistic_sd", bm_field + " SUM", list_bm, "国土调查总面积", 1);

                    pw.AddProcessMessage(20, time_base, "指标写入Excel");

                    // 将映射属性表中获取字典Dictionary
                    Dictionary<string, string> dict = GisTool.GetDictFromPath(gdb_path + @"\statistic_sd", @"分组", "SUM_" + bm_field);
                    // 属性映射大类
                    OfficeTool.ExcelAttributeMapper(excel_path + @"\sheet1$", 6, 7, dict, 5);

                    pw.AddProcessMessage(20, time_base, "删除0值行");

                    // 删除0值行
                    OfficeTool.ExcelDeleteNullRow(excel_path + @"\sheet1$", 7, 4);

                    pw.AddProcessMessage(20, time_base, "删除空列");
                    // 删除空列
                    OfficeTool.ExcelDeleteNullCol(excel_path + @"\sheet1$", 5);
                    OfficeTool.ExcelDeleteNullCol(excel_path + @"\sheet1$", 4);

                    // 删除中间数据
                    Arcpy.Delect(gdb_path + @"\statistic_sd");
                    Arcpy.Delect(gdb_path + @"\fc_sd");
                    File.Delete(folder_path + @"\三调用地自转换.xlsx");
                    
                });
                pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
