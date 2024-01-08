using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections;
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
    /// Interaction logic for UpdateGDB.xaml
    /// </summary>
    public partial class UpdateGDB : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public UpdateGDB()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "要素类追加至空库(批量)";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string gdb_path = textGDBPath.Text;
                string gdb_empty_path = textEmptyGDBPath.Text;
                string folder_path = textFolderPath.Text;

                // 判断参数是否选择完全
                if (gdb_path == "" || gdb_empty_path == "" || folder_path == "")
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
                    pw.AddProcessMessage(10, "复制空库");
                    // 复制空库
                    BaseTool.CopyAllFiles(gdb_empty_path, folder_path + @"\result_all.gdb");

                    pw.AddProcessMessage(10, time_base, "创建dict，捕捉同名要素类和独立表");
                    // 获取要素类和表的完整路径
                    List<string> in_data_paths = gdb_path.GetFeatureClassAndTablePath();
                    string resultGDBPath = folder_path + @"\result_all.gdb";
                    List<string> empty_data_paths = resultGDBPath.GetFeatureClassAndTablePath();
                    // 创建dict，捕捉同名要素类和独立表
                    Dictionary<string, string> keyValuePairs= new Dictionary<string, string>();
                    foreach (var da in in_data_paths)
                    {
                        // 提取要素类或独立表名
                        string in_name = da[(da.LastIndexOf(@"\") + 1)..];
                        foreach (var em in empty_data_paths)
                        {
                            string em_name = em[(em.LastIndexOf(@"\") + 1)..];
                            if (in_name == em_name)
                            {
                                keyValuePairs.Add(da, em);
                                break;
                            }
                        }
                    }

                    // 执行追加工具
                    foreach (var pair in keyValuePairs)
                    {
                        string pair_name = pair.Key[(pair.Key.LastIndexOf(@"\") + 1)..];
                        pw.AddProcessMessage(10, time_base, "追加：" + pair_name);
                        Arcpy.Append(pair.Key, pair.Value);
                    }
                });

                pw.AddProcessMessage(70, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text = UITool.OpenDialogGDB();
        }

        private void openEmptyGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textEmptyGDBPath.Text = UITool.OpenDialogGDB();
        }

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }
    }
}
