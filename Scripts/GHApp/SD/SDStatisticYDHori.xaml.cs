using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using CCTool.Scripts.UI.ProWindow;
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
using Row = ArcGIS.Core.Data.Row;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.GHApp.SD
{
    /// <summary>
    /// Interaction logic for SDStatisticYDHori.xaml
    /// </summary>
    public partial class SDStatisticYDHori : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SDStatisticYDHori()
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
        string tool_name = "1、土地利用现状分类面积汇总表(竖版)";

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
                string area_type = combox_area.Text[..2]; // 去掉“面积”，以防shp故障
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
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调规程.土地利用现状分类面积汇总表(竖版).xlsx", excel_path);
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

                        // 用地编码转换
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_1", folder_path + @"\三调用地自转换.xlsx\一级$");
                        GisTool.AttributeMapper(adj_fc, "DLMC", "mc_2", folder_path + @"\三调用地自转换.xlsx\二级$");

                        // 新建sheet
                        OfficeTool.ExcelCopySheet(excel_path, "sheet1", area_name);
                        string new_sheet_path = excel_path + @$"\{area_name}$";

                        // 处理地块
                        // 要处理的要素名称
                        string area_name_detail = adj_fc[(adj_fc.LastIndexOf(@"\") + 1)..];

                        pw.AddProcessMessage(10, time_base, $"{area_name_detail}__汇总用地指标", Brushes.Gray);

                        // 收集未扣除田坎的面积
                        Arcpy.Statistics(adj_fc, gdb_path + @"\前", area_type + " SUM", "");
                        double zmj_old = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\前", "SUM_" + area_type, "OBJECTID=1"));
                        // 计算扣除面积
                        Arcpy.CalculateField(adj_fc, area_type, $"round(!{area_type}!* (1-!KCXS!),{digit})");

                        // 收集扣除田坎的面积
                        Arcpy.Statistics(adj_fc, gdb_path + @"\后", area_type + " SUM", "");
                        double zmj_new = double.Parse(GisTool.GetCellFromPath(gdb_path + @"\后", "SUM_" + area_type, "OBJECTID=1"));
                        double kcmj = Math.Round(zmj_old - zmj_new, digit);

                        // 汇总大、中类
                        List<string> list_bm = new List<string>() { "mc_1", "mc_2" };
                        string statistic_sd = gdb_path + @"\statistic_sd";
                        GisTool.MultiStatistics(adj_fc, statistic_sd, area_type + " SUM", list_bm, "国土调查总面积", 4);

                        // 统计田坎面积
                        if (kcmj != 0)
                        {
                            // 插入【田坎】行
                            GisTool.UpdataRowToTable(statistic_sd, $"分组,田坎;SUM_{area_type},{kcmj}");
                            // 计算【其他土地、国土调查总面积】行
                            GisTool.IncreRowValueToTable(statistic_sd, $"分组,其他土地;SUM_{area_type},{kcmj}");
                            GisTool.IncreRowValueToTable(statistic_sd, $"分组,国土调查总面积;SUM_{area_type},{kcmj}");
                        }
                        // 再次确认小数位数
                        Arcpy.CalculateField(statistic_sd, $"SUM_{area_type}", $"round(!SUM_{area_type}!, {digit})");
                        // 将映射属性表中获取字典Dictionary
                        Dictionary<string, string> dict = GisTool.GetDictFromPath(gdb_path + @"\statistic_sd", @"分组", "SUM_" + area_type);

                        // 属性映射到Excel
                        OfficeTool.ExcelAttributeMapper(new_sheet_path, 7, 5, dict, 5);
                        pw.AddProcessMessage(20, time_base, "删除空列", Brushes.Gray);

                        // 更新面积标识
                        OfficeTool.ExcelWriteCell(new_sheet_path, 2, 1, $"面积单位：{unit}");

                        // 删除空行
                        OfficeTool.ExcelDeleteNullRow(new_sheet_path, 5, 4);
                        // 删除参照列
                        OfficeTool.ExcelDeleteCol(new_sheet_path, 7);
                    }

                    // 删除sheet1
                    OfficeTool.ExcelDeleteSheet(excel_path, "Sheet1");

                    // 删除中间数据
                    Arcpy.Delect(gdb_path + @"\statistic_sd");
                    Arcpy.Delect(gdb_path + @"\前");
                    Arcpy.Delect(gdb_path + @"\后");
                    File.Delete(folder_path + @"\三调用地自转换.xlsx");
                    Directory.Delete(new_gdb, true);
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
