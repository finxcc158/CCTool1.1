using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace CCTool.Scripts
{
    /// <summary>
    /// Interaction logic for StatisticsYDYH.xaml
    /// </summary>
    public partial class StatisticsYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsYDYH()
        {
            InitializeComponent();
            Init();       // 初始化
        }
        // 初始化
        private void Init()
        {
            // combox_model框中添加3种转换模式，默认【大类】
            combox_model.Items.Add("大类");
            combox_model.Items.Add("中类");
            combox_model.Items.Add("小类");
            combox_model.SelectedIndex = 0;

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "用地用海指标汇总";

        // 点击打开按钮，选择输出的Excel文件位置
        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开Excel文件
            string path = UITool.SaveDialogExcel();
            // 将Excel文件的路径置入【textExcelPath】
            textExcelPath.Text = path;
        }

        // 添加要素图层的所有字符串字段到combox中
        private void combox_field_DropDown(object sender, EventArgs e)
        {
            // 将图层字段加入到Combox列表中
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_bmField);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        // 运行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取默认数据库
                var init_gdb = Project.Current.DefaultGeodatabasePath;
                // 获取参数
                string fc_path = combox_fc.Text;
                string field_bm = combox_bmField.Text;
                string output_table = init_gdb + @"\output_table";
                string excel_path = textExcelPath.Text;

                string version = combox_version.Text;

                string areaField = combox_areaField.Text;
                string statistics_fields = $"{areaField} SUM";
                string sta_result = $"SUM_{areaField}";

                List<string> bm1 = new List<string>() { "BM_1" };
                List<string> bm2 = new List<string>() { "BM_1", "BM_2" };
                List<string> bm3 = new List<string>() { "BM_1", "BM_2", "BM_3" };

                // 判断参数是否选择完全
                if (fc_path == "" || field_bm == "" || output_table == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                // 模式转换
                int model = 1;
                if (combox_model.Text == "中类") { model = 2; }
                else if (combox_model.Text == "小类") { model = 3; }

                Close();
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(20, "生成地类编码");
                    // 生成地类编码
                    GisTool.CreateYDYHBM(fc_path, field_bm, model, false);
                    // 汇总用地
                    if (model == 1)         // 大类
                    {
                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Excel.【模板】用地用海_大类.xlsx", excel_path);
                        pw.AddProcessMessage(20, time_base, "汇总大类指标");
                        // 汇总大类
                        GisTool.MultiStatistics(fc_path, output_table, statistics_fields, bm1, "合计", 1);
                        
                        pw.AddProcessMessage(20, time_base, "指标写入Excel");

                        // 保留2位小数
                        Arcpy.CalculateField(output_table, sta_result, $"round(!{sta_result}!,2)");
                        // 将映射属性表中获取字典Dictionary
                        Dictionary<string, string> dict = GisTool.GetDictFromPath(output_table, @"分组", sta_result);
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 1, 3, dict, 3);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_path + @"\用地用海$", 3, 3);
                        // 删除中间字段
                        Arcpy.DeleteField(fc_path, "BM_1");
                    }
                    else if (model == 2)       // 中类
                    {
                        string excelName = "";
                        if (version == "旧版")
                        {
                            excelName = "【模板】用地用海_中类";
                        }
                        else
                        {
                            excelName = "【新模板】用地用海_中类";
                        }

                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", excel_path);
                        pw.AddProcessMessage(20, time_base, "汇总大、中类指标");
                        // 汇总大、中类
                        GisTool.MultiStatistics(fc_path, output_table, statistics_fields, bm2, "合计", 1);
                        
                        pw.AddProcessMessage(20, time_base, "指标写入Excel");
                        // 保留2位小数
                        Arcpy.CalculateField(output_table, sta_result, $"round(!{sta_result}!,2)");
                        // 将映射属性表中获取字典Dictionary
                        Dictionary<string, string> dict = GisTool.GetDictFromPath(output_table, @"分组", sta_result);
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 7, 4, dict, 4);
                        // 属性映射中类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 8, 4, dict, 4);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_path + @"\用地用海$", 4, 4);
                        // 删除指定列
                        OfficeTool.ExcelDeleteCol(excel_path + @"\用地用海$", new List<int>() { 8, 7 });
                        // 删除中间字段
                        Arcpy.DeleteField(fc_path, "BM_2");
                    }
                    else if (model == 3)       // 小类
                    {
                        string excelName = "";
                        if (version == "旧版")
                        {
                            excelName = "【模板】用地用海_小类";
                        }
                        else
                        {
                            excelName = "【新模板】用地用海_小类";
                        }

                        // 复制嵌入资源中的Excel文件
                        BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", excel_path);
                        pw.AddProcessMessage(20, time_base, "汇总大、中、小类指标");
                        // 汇总大、中、小类
                        GisTool.MultiStatistics(fc_path, output_table, statistics_fields, bm3, "合计", 1);

                        pw.AddProcessMessage(20, time_base, "指标写入Excel");
                        // 保留2位小数
                        Arcpy.CalculateField(output_table, sta_result, $"round(!{sta_result}!,2)");
                        // 将映射属性表中获取字典Dictionary
                        Dictionary<string, string> dict = GisTool.GetDictFromPath(output_table, @"分组", sta_result);
                        // 属性映射大类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 7, 5, dict, 4);
                        // 属性映射中类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 8, 5, dict, 4);
                        // 属性映射小类
                        OfficeTool.ExcelAttributeMapper(excel_path + @"\用地用海$", 9, 5, dict, 4);
                        // 删除0值行
                        OfficeTool.ExcelDeleteNullRow(excel_path + @"\用地用海$", 5, 4);
                        // 删除指定列
                        OfficeTool.ExcelDeleteCol(excel_path + @"\用地用海$", new List<int>() { 9, 8, 7 });
                        // 删除中间字段
                        Arcpy.DeleteField(fc_path, "BM_3");
                    }

                    // 删除中间数据和中间字段
                    Arcpy.Delect(output_table);
                });
                pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
        }

        private void combox_areaField_DropDown(object sender, EventArgs e)
        {
            string fc_path = combox_fc.Text;
            UITool.AddAllFloatFieldsToCombox(fc_path, combox_areaField);
        }
    }
}
