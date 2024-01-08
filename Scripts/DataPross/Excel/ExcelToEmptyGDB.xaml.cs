using ArcGIS.Desktop.Framework.Threading.Tasks;
using Aspose.Cells;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
    /// Interaction logic for ExcelToEmptyGDB.xaml
    /// </summary>
    public partial class ExcelToEmptyGDB : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ExcelToEmptyGDB()
        {
            InitializeComponent();
            Init();
        }

        // 初始化
        private void Init()
        {
            // combox_sr框中添加几种预制坐标系
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_34");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_35");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_36");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_37");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_38");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_39");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_40");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_Zone_41");

            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_102E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_105E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_108E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_111E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_114E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_117E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_120E");
            combox_sr.Items.Add("CGCS2000_3_Degree_GK_CM_123E");
            combox_sr.SelectedIndex = 5;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "属性结构描述表转空库(批量)";

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.OpenDialogExcel();
        }

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string excel_path = textExcelPath.Text;
                string gdb_path = textGDBPath.Text;
                string spatial_reference = combox_sr.Text;

                // 提取Excel文件名【即数据库名】
                string name_excel = excel_path[(excel_path.LastIndexOf(@"\") + 1)..excel_path.LastIndexOf(@".")];

                // 判断参数是否选择完全
                if (excel_path == "" || gdb_path == "" || spatial_reference == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();
                // 异步执行
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "创建空GDB数据库");
                    // 创建一个空的GDB数据库
                    Arcpy.CreateFileGDB(gdb_path, name_excel);

                    // 获取工作薄、工作表
                    string excelFile = excel_path.GetExcelPath();
                    // 打开工作薄
                    Workbook wb = OfficeTool.OpenWorkbook(excelFile);

                    // 针对每个工作表进行处理
                    for (int i = 0; i < wb.Worksheets.Count; i++)
                    {
                        // 获取工作表
                        Worksheet sheet = wb.Worksheets[i];
                        // 获取表名
                        string name_table = sheet.Cells[0, 0].StringValue;

                        pw.AddProcessMessage(10, time_base, "转表：" + name_table);
                        // 解析表名，获取要素名、要素别名
                        string name_feature = name_table[(name_table.LastIndexOf(@"属性表名：") + 5)..name_table.LastIndexOf(@"）")];
                        string name_alias = name_table[(name_table.LastIndexOf(" ") + 1)..name_table.LastIndexOf(@"属性结构")];
                        // 根据表名中是否包含要素类型来判断要创建的是要素还是表
                        if (name_table.EndsWith("】"))           //  要素类的情况
                        {
                            string fc_type = name_table[(name_table.LastIndexOf("【") + 1)..name_table.LastIndexOf("】")];
                            string fc_type_final = GetFeatureClassType(fc_type);
                            // 创建空要素
                            Arcpy.CreateFeatureclass(gdb_path + @"\" + name_excel + @".gdb", name_feature, fc_type_final, spatial_reference, name_alias);
                        }
                        else    //  表的情况
                        {
                            // 创建表
                            Arcpy.CreateTable(gdb_path + @"\" + name_excel + @".gdb", name_feature, name_alias);
                        }

                        // 获取总行数
                        int row_count = sheet.Cells.MaxDataRow;
                        // 创建字段
                        for (int j = 2; j <= row_count; j++)
                        {
                            // 获取字段属性
                            string mc = sheet.Cells[j, 1].StringValue;
                            string dm = sheet.Cells[j, 2].StringValue;
                            string length = sheet.Cells[j, 4].StringValue;
                            int length_value;
                            // 如果长度为空，则设为默认的255
                            if (length == "")
                            {
                                length_value = 255;
                            }
                            else
                            {
                                length_value = int.Parse(length);
                            }
                            if (mc != "" && dm != "")
                            {
                                string mc_value = mc.ToString().Replace("\n", "");               // 字段别名
                                string dm_value = dm.ToString().Replace("\n", "");              // 字段名
                                string field_type = GetFeildType(sheet.Cells[j, 3].StringValue);          // 字段类型
                                // 创建字段
                                Arcpy.AddField(gdb_path + @"\" + name_excel + @".gdb\" + name_feature, dm_value, field_type, mc_value, length_value);
                            }
                        }
                    }
                    wb.Dispose();

                    pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);
                });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        // 要素类型转换
        public static string GetFeatureClassType(string type)
        {
            string fc_type = "";
            if (type == "点") { fc_type = "POINT"; }
            else if (type == "线") { fc_type = "POLYLINE"; }
            else if (type == "面") { fc_type = "POLYGON"; }

            return fc_type;
        }

        // 字段类型转换
        public static string GetFeildType(string type)
        {
            string fd_type = type;
            if (type == "Float") { fd_type = "Double"; }
            else if (type == "Char" || type == "VarChar") { fd_type = "Text"; }
            else if (type == "Int") { fd_type = "Long"; }
            else if (type == "Date") { fd_type = "DATE"; }

            return fd_type;
        }
    }
}
