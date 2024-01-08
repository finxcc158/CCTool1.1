using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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

namespace CCTool.Scripts.UI.SD
{
    /// <summary>
    /// Interaction logic for SDStatistic3.xaml
    /// </summary>
    public partial class SDStatistic3 : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SDStatistic3()
        {
            InitializeComponent();
            // 初始化combox
            combox_unit.Items.Add("平方米");
            combox_unit.Items.Add("公顷");
            combox_unit.Items.Add("平方公里");
            combox_unit.Items.Add("亩");
            combox_unit.SelectedIndex = 0;

            combox_area.Items.Add("投影面积");
            combox_area.Items.Add("图斑面积");
            combox_area.SelectedIndex = 0;

            combox_digit.Items.Add("1");
            combox_digit.Items.Add("2");
            combox_digit.Items.Add("3");
            combox_digit.Items.Add("4");
            combox_digit.Items.Add("5");
            combox_digit.Items.Add("6");
            combox_digit.SelectedIndex = 1;

            combox_field_area.IsEnabled = false;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "3、土地利用现状三大类面积汇总表";

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
                string area_type = combox_area.Text[..2];
                string unit = combox_unit.Text;
                int digit = int.Parse(combox_digit.Text);
                bool isAdj = (bool)checkBox_adj.IsChecked;

                string fc_area_path = combox_fc_area.Text;
                string name_field = combox_field_area.Text;

                string excel_path = textTablePath.Text;
                // 默认数据库位置
                var gdb_path = Project.Current.DefaultGeodatabasePath;
                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (fc_path == "" || excel_path == "")
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
                    // 获取图层名
                    string single_layer_path = fc_path.GetLayerSingleName();

                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调规程.土地利用现状三大类面积汇总表.xlsx", excel_path);
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调用地自转换.xlsx", folder_path + @"\三调用地自转换.xlsx");

                    // 放到新建的数据库中
                    Arcpy.CreateFileGDB(folder_path, "新数据库");
                    string new_gdb = $@"{folder_path}\新数据库.gdb";

                    if (fc_area_path == "")  // 如果没有分地块统计
                    {
                        Arcpy.Dissolve(fc_path, $@"{new_gdb}\{single_layer_path}");
                    }

                    else   // 如果分地块统计
                    {
                        // 按属性分割
                        Arcpy.SplitByAttributes(fc_area_path, new_gdb, name_field);
                    }

                    // 收集分割后的地块
                    List<string> list_area = new_gdb.GetFeatureClassAndTablePath();
                    // 新建的gdb列表
                    List<string> gdb_list = new List<string>();

                    foreach (var area in list_area)
                    {
                        // 裁剪新用地
                        string area_name = area[(area.LastIndexOf(@"\") + 1)..];
                        string adj_fc = $@"{new_gdb}\{area_name}_结果";

                        pw.AddProcessMessage(20, time_base, $"处理表格：{area_name}");

                        if (!isAdj)          // 如果不平差计算
                        {
                            GisTool.AdjustmentNot(fc_path, area, adj_fc, area_type, unit, digit);
                        }
                        else          // 平差计算
                        {
                            pw.AddProcessMessage(10, time_base, $"平差计算", Brushes.Gray);
                            GisTool.Adjustment(fc_path, area, adj_fc, area_type, unit, digit);
                        }

                        pw.AddProcessMessage(10, time_base, "用地名称转为统计分类字段", Brushes.Gray);
                        // 添加一个类一、二级字段
                        Arcpy.AddField(adj_fc, "mc_1", "TEXT");
                        Arcpy.AddField(adj_fc, "mc_2", "TEXT");
                        Arcpy.AddField(adj_fc, "mc_3", "TEXT");

                        // 用地编码转换
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_1", folder_path + @"\三调用地自转换.xlsx\三大类一级$");
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_2", folder_path + @"\三调用地自转换.xlsx\二级$");
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_3", folder_path + @"\三调用地自转换.xlsx\三大类$");

                        // 将新用地按【坐落地代码名称】分割放入新的数据库，并收集，方便下一步处理
                        Arcpy.CreateFileGDB(folder_path, $"{area_name}_裁剪结果");
                        gdb_list.Add(@$"{folder_path}\{area_name}_裁剪结果.gdb");
                        // 再分割
                        Arcpy.SplitByAttributes(adj_fc, $@"{folder_path}\{area_name}_裁剪结果.gdb", "ZLDWDM;ZLDWMC");
                        // 收集分割后的地块
                        string resultGDBPath = $@"{folder_path}\{area_name}_裁剪结果.gdb";
                        List<string> list_area_detail = resultGDBPath.GetFeatureClassAndTablePath();

                        // 新建sheet
                        OfficeTool.ExcelCopySheet(excel_path, "sheet1", area_name);
                        string new_sheet_path = excel_path + @$"\{area_name}$";

                        // 处理每个行政单位
                        int start_row = 7;
                        List<int> row_list = new List<int>();
                        foreach (var area_detail in list_area_detail)
                        {
                            // 要处理的要素名称
                            string area_name_detail = area_detail[(area_detail.LastIndexOf(@"\") + 1)..];
                            // 获取行政代码和行政名称
                            string ad_name = area_name_detail[(area_name_detail.LastIndexOf(@"_") + 1)..];
                            string ad_code = area_name_detail[1..area_name_detail.LastIndexOf(@"_")];

                            pw.AddProcessMessage(10, time_base, $"{ad_name}__汇总用地指标", Brushes.Gray);

                            // 收集未扣除田坎的面积
                            Arcpy.Statistics(area_detail, gdb_path + @"\前", area_type + " SUM", "");
                            double zmj_old = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\前", "SUM_" + area_type, "OBJECTID=1"));
                            // 计算扣除面积
                            Arcpy.CalculateField(area_detail, area_type, $"round(!{area_type}!* (1-!KCXS!),{digit})");

                            // 收集扣除田坎的面积
                            Arcpy.Statistics(area_detail, gdb_path + @"\后", area_type + " SUM", "");
                            double zmj_new = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\后", "SUM_" + area_type, "OBJECTID=1"));
                            double kcmj = Math.Round(zmj_old - zmj_new, digit);

                            // 复制行
                            OfficeTool.ExcelCopyRow(new_sheet_path, 6, start_row);
                            // 记录写入行数的列表
                            row_list.Add(start_row);

                            // 汇总大、中类
                            List<string> list_bm = new List<string>() { "mc_1", "mc_2", "mc_3" };
                            string statistic_sd = gdb_path + @"\statistic_sd";
                            GisTool.MultiStatistics(area_detail, statistic_sd, area_type + " SUM", list_bm, "国土调查总面积", 4);

                            // 统计田坎面积
                            if (kcmj != 0)
                            {
                                // 插入【田坎】行
                                GisTool.UpdataRowToTable(statistic_sd, $"分组,田坎;SUM_{area_type},{kcmj}");
                                // 计算【其他土地、国土调查总面积】行
                                GisTool.IncreRowValueToTable(statistic_sd, $"分组,农用地其他土地;SUM_{area_type},{kcmj}");
                                GisTool.IncreRowValueToTable(statistic_sd, $"分组,农用地;SUM_{area_type},{kcmj}");
                                GisTool.IncreRowValueToTable(statistic_sd, $"分组,国土调查总面积;SUM_{area_type},{kcmj}");
                            }
                            // 再次确认小数位数
                            Arcpy.CalculateField(statistic_sd, $"SUM_{area_type}", $"round(!SUM_{area_type}!, {digit})");
                            // 将映射属性表中获取字典Dictionary
                            Dictionary<string, string> dict = GisTool.GetDictFromPath(gdb_path + @"\statistic_sd", @"分组", "SUM_" + area_type);
                            // 加入行政代码和名称
                            dict.Add("名称", ad_name);
                            dict.Add("代码", ad_code);

                            // 属性映射到Excel
                            OfficeTool.ExcelAttributeMapperCol(new_sheet_path, 1, start_row, dict);

                            // 进入下一个行政单位
                            start_row++;
                        }
                        pw.AddProcessMessage(20, time_base, "删除空列", Brushes.Gray);

                        // 更新面积标识
                        OfficeTool.ExcelWriteCell(new_sheet_path, 2, 0, $"面积单位：{unit}");

                        // 删除空列
                        OfficeTool.ExcelDeleteNullCol(new_sheet_path, row_list, 2);
                        // 删除行
                        OfficeTool.ExcelDeleteRow(new_sheet_path, new List<int>() { 6, 1 });
                    }

                    // 删除sheet1
                    OfficeTool.ExcelDeleteSheet(excel_path, "Sheet1");

                    // 删除中间数据
                    Arcpy.Delect(gdb_path + @"\statistic_sd");
                    Arcpy.Delect(gdb_path + @"\前");
                    Arcpy.Delect(gdb_path + @"\后");
                    File.Delete(folder_path + @"\三调用地自转换.xlsx");
                    Directory.Delete(new_gdb, true);
                    foreach (var item in gdb_list)
                    {
                        Directory.Delete(item, true);
                    }
                });
                pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_fc_area_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc_area);
        }

        private void combox_fc_area_Closed(object sender, EventArgs e)
        {
            string fc_area = combox_fc_area.Text;
            if (fc_area == "")
            {
                combox_field_area.IsEnabled = false;
            }
            else
            {
                combox_field_area.IsEnabled = true;
            }
        }

        private void combox_field_area_DropDown(object sender, EventArgs e)
        {
            string area_fc = combox_fc_area.Text;
            UITool.AddTextFieldsToCombox(area_fc, combox_field_area);
        }
        // 数据检查
        private async void btn_check_Click(object sender, RoutedEventArgs e)
        {

            string lyName = combox_fc.Text;
            string lyArea = combox_fc_area.Text;
            string areaName = combox_field_area.Text;

            // 打开检查框
            ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "数据检查");
            pw.AddMessage("开始执行数据检查" + "\r", Brushes.Green);
            try
            {
                await QueuedTask.Run(() =>
                {
                    // 检查要素类型是否正确
                    string result_fcType = CheckTool.CheckFeatureClassType(lyName, "Polygon");
                    pw.AddMessage(result_fcType, Brushes.Red);

                    if (lyArea != "")
                    {
                        // 检查分地块要素类型是否正确
                        string result_areaType = CheckTool.CheckFeatureClassType(lyArea, "Polygon");
                        pw.AddMessage(result_areaType, Brushes.Red);
                        // 检查分地块字段是否包含空值
                        string result_value_areaName = CheckTool.CheckFieldValueEmpty(lyArea, areaName);
                        pw.AddMessage(result_value_areaName, Brushes.Red);
                    }

                    if (result_fcType.Contains("】的要素类型不是【"))
                    {
                        return;
                    }

                    // 检查字段值是否合规
                    string result_value = CheckTool.CheckFieldValue(lyName, "DLMC", DataStore.yd_sd);
                    pw.AddMessage(result_value, Brushes.Red);

                    // 检查是否包含空值
                    string result_value_cs = CheckTool.CheckFieldValueEmpty(lyName, "KCXS", "DLMC IN ('旱地', '水田', '水浇地')");
                    pw.AddMessage(result_value_cs, Brushes.Red);

                    // 检查是否包含空值  【ZLDWDM,ZLDWMC】
                    string result_value_zldw = CheckTool.CheckFieldValueEmpty(lyName, new List<string>() { "ZLDWDM", "ZLDWMC" });
                    pw.AddMessage(result_value_zldw, Brushes.Red);

                    pw.AddProcessMessage(100, "检查完成。", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }
        }
    }
}
