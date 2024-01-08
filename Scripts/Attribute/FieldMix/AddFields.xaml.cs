using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.Formula.Functions;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for AddFields.xaml
    /// </summary>
    public partial class AddFields : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AddFields()
        {
            InitializeComponent();
            //textFolderPath.Text = @"D:\【软件资料】\GIS相关\GisPro工具箱\4-测试数据\空 - 副本";
            //textExcelPath.Text = @"C:\Users\Administrator\Desktop\添加字段工具示例Excel.xlsx";
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "添加字段（批量）";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;
                // 获取工程默认文件夹位置
                var def_path = Project.Current.HomeFolderPath;
                // 获取指标
                string folder_path = textFolderPath.Text;
                string excel_path = textExcelPath.Text;
                string key_word = textKeyWord.Text;

                // 判断参数是否选择完全
                if (folder_path == "" || excel_path == "")
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
                    pw.AddMessage("获取字段属性结构表");

                    // 获取字段属性结构表
                    List<List<string>> list_field_attribute = new List<List<string>>();
                    // 获取名称、别名、类型、长度
                    List<string> list_mc = OfficeTool.GetListFromExcelAll(excel_path, 0, 2);
                    List<string> list_bm = OfficeTool.GetListFromExcelAll(excel_path, 1, 2);
                    List<string> list_fieldType = OfficeTool.GetListFromExcelAll(excel_path, 2, 2);
                    List<string> list_lenth = OfficeTool.GetListFromExcelAll(excel_path, 3, 2);
                    // 加入集合
                    for (int i = 0; i < list_mc.Count; i++)
                    {
                        list_field_attribute.Add(new List<string> { list_mc[i], list_bm[i], list_fieldType[i], list_lenth[i] });
                    }

                    pw.AddProcessMessage(10, time_base, "获取所有要素类及表文件");

                    // 合并路径列表
                    List<string> obj_all = new List<string>();
                    // 获取所有shp文件
                    List<string> shps = folder_path.GetAllFiles(".shp");
                    // 添加shp
                    obj_all.AddRange(shps);

                    // 获取所有gdb文件
                    List<string> gdbs = folder_path.GetAllGDBFilePaths();
                    // 获取所有要素类文件
                    if (gdbs is not null)
                    {
                        foreach (var gdb in gdbs)
                        {
                            List<string> fcs_and_tbs = gdb.GetFeatureClassAndTablePath();
                            // 添加要素类和表
                            obj_all.AddRange(fcs_and_tbs);
                        }
                    }

                    // 添加字段
                    foreach (var ob in obj_all)
                    {
                        string target_name = ob[(ob.LastIndexOf(@"\") + 1)..];
                        pw.AddProcessMessage(5, time_base, $"添加字段:{ob}");
                        // 如果不含关键字，直接添加字段
                        if (key_word != "")
                        {
                            foreach (var fa in list_field_attribute)
                            {
                                Arcpy.AddField(ob, fa[0], fa[2], fa[1], int.Parse(fa[3]));
                            }
                        }
                        else
                        {
                            // 如果含有关键字，筛选出含关键字的部分，再添加字段
                            if (target_name.Contains(key_word))
                            {
                                foreach (var fa in list_field_attribute)
                                {
                                    Arcpy.AddField(ob, fa[0], fa[2], fa[1], int.Parse(fa[3]));
                                }
                            }
                        }
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

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.OpenDialogExcel();
        }

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }
    }
}
