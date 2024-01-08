using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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
using Brushes = System.Windows.Media.Brushes;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for StatisticsSD.xaml
    /// </summary>
    public partial class StatisticsSD : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StatisticsSD()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "统计三调【三大类】";

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

        // 执行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string bm_field = combox_field_BM.Text;
                string table_path = textTablePath.Text;
                // 默认数据库位置
                var gdb = Project.Current.DefaultGeodatabasePath;

                // 判断参数是否选择完全
                if (fc_path == "" || bm_field == "" || table_path == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                // 根据图层名找到当前图层
                var map = MapView.Active.Map;
                FeatureLayer initlayer = map.FindLayers(fc_path)[0] as FeatureLayer;

                Close();
                // 使用异步任务在后台进行编辑操作
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "汇总图斑面积");
                    // 汇总
                    Arcpy.Statistics(fc_path, gdb + @"\statistisc_all", bm_field + @" SUM", "DLBM");

                    pw.AddProcessMessage(10, time_base, "筛选三大类用地");
                    // 三大类用地SQL
                    string sql_nyd = "DLBM IN ('0303','0304','0306','0402','0101','0102','0103','0201','0201K','0202','0202K','0204','0301', '0301K', '0302', '0302K' ,'0305','0307','0401' ,'0402' ,'0403' ,'1006','1103','1104','1104A', '1104K' ,'1107', '1107A', '1202' ,'1203')";
                    string sql_jsyd = "DLBM IN ('0603','05H1','0508','0601','0602','0701','0702','08H1','08H2','0809','0810','0810A','09','1001', '1002','1003','1004','1005','1007','1008','1009', '1109', '1201')";
                    string sql_wlyd = "DLBM IN ( '1105','1106' ,'1108' ,'0404' ,'1101' ,'1102','1110','1204' ,'1205' ,'1206' ,'1207' )";
                    // 筛选三大类用地
                    Arcpy.TableSelect(gdb + @"\statistisc_all", gdb + @"\statistisc_nyd", sql_nyd);
                    Arcpy.TableSelect(gdb + @"\statistisc_all", gdb + @"\statistisc_jsyd", sql_jsyd);
                    Arcpy.TableSelect(gdb + @"\statistisc_all", gdb + @"\statistisc_wlyd", sql_wlyd);

                    pw.AddProcessMessage(20, time_base, "计算三大类用地");
                    // 分别汇总三大类用地
                    Arcpy.Statistics(gdb + @"\statistisc_nyd", gdb + @"\all_nyd", "SUM_" + bm_field + @" SUM", "");
                    Arcpy.Statistics(gdb + @"\statistisc_jsyd", gdb + @"\all_jsyd", "SUM_" + bm_field + @" SUM", "");
                    Arcpy.Statistics(gdb + @"\statistisc_wlyd", gdb + @"\all_wlyd", "SUM_" + bm_field + @" SUM", "");

                    pw.AddProcessMessage(10, time_base, "新建共有字段并赋值");
                    // 新建一个共有字段【地类名称】，并赋值
                    string filed_dl = "地类名称";
                    Arcpy.AddField(gdb + @"\all_nyd", filed_dl, "TEXT");
                    Arcpy.AddField(gdb + @"\all_jsyd", filed_dl, "TEXT");
                    Arcpy.AddField(gdb + @"\all_wlyd", filed_dl, "TEXT");
                    Arcpy.CalculateField(gdb + @"\all_nyd", filed_dl, "'" + "农用地" +"'");
                    Arcpy.CalculateField(gdb + @"\all_jsyd", filed_dl, "'" + "建设用地" + "'");
                    Arcpy.CalculateField(gdb + @"\all_wlyd", filed_dl, "'" + "未利用地" + "'");

                    pw.AddProcessMessage(10, time_base, "合并表格");
                    // 合并表格
                    List<string> files = new List<string>() { gdb + @"\all_nyd", gdb + @"\all_jsyd", gdb + @"\all_wlyd"};
                    Arcpy.Merge(files, gdb + @"\all_merge", true);
                    // 单位转换为公顷
                    Arcpy.CalculateField(gdb + @"\all_merge", "SUM_SUM_" + bm_field, "!SUM_SUM_" + bm_field + "!/10000");

                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel.三调三大地类统计表.xlsx", table_path);
                    
                    pw.AddProcessMessage(10, time_base, "获取映射属性表");
                    // 将映射属性表中获取字典Dictionary
                    Dictionary<string, string> dict = GisTool.GetDictFromPath("all_merge", filed_dl, "SUM_SUM_" + bm_field);
                    
                    pw.AddProcessMessage(20, time_base, "写入Excel表");
                    // 属性映射大类
                    OfficeTool.ExcelAttributeMapper(table_path + @"\Sheet1$", 2, 3, dict, 3);
                    // 删除中间数据
                    List<string> del_tables = new List<string>() { "statistisc_all", "statistisc_nyd", "statistisc_jsyd", "statistisc_wlyd", "all_nyd", "all_jsyd", "all_wlyd", "all_merge" };
                    foreach (var tb in del_tables)
                    {
                        Arcpy.Delect(gdb + @"\" + tb);
                    }
                });
                pw.AddProcessMessage(10, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
    }
}
