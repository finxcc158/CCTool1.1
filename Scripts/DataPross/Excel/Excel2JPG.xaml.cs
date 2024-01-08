using ArcGIS.Desktop.Framework.Threading.Tasks;
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

namespace CCTool.Scripts.DataPross.Excel
{
    /// <summary>
    /// Interaction logic for Excel2JPG.xaml
    /// </summary>
    public partial class Excel2JPG : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public Excel2JPG()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "Excel表格转图片";

        private void openExcelButton_Click(object sender, RoutedEventArgs e)
        {
            textExcelPath.Text = UITool.OpenDialogExcel();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指标
                string excel_path = textExcelPath.Text;

                // 提取Excel文件名【即数据库名】
                string name_excel = excel_path[(excel_path.LastIndexOf(@"\") + 1)..excel_path.LastIndexOf(@".")];

                // 判断参数是否选择完全
                if (excel_path == "" )
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
                    OfficeTool.Excel2Pic(excel_path);

                    pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
                });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

    }
}
