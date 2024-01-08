using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for ClipGDB.xaml
    /// </summary>
    public partial class ClipGDB : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ClipGDB()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "按范围分割数据库";

        private void openResultGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textResultGDB.Text = UITool.OpenDialogFolder();
        }

        private void openOriginalGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textOriginalGDB.Text = UITool.OpenDialogGDB();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string original_gdb = textOriginalGDB.Text;
                string clip_fc = comboxClipFeature.Text;
                string clip_field = comboxClipField.Text;
                string folder_resultGDB = textResultGDB.Text;

                // 工程默认文件夹位置
                string folder_path = Project.Current.HomeFolderPath;

                // 判断参数是否选择完全
                if (original_gdb == "" || clip_fc == "" || clip_field == "" || folder_resultGDB == "")
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
                    pw.AddProcessMessage(10, "解析范围要素");
                    // 创建临时数据库
                    Arcpy.CreateFileGDB(folder_path, "新数据库");
                    string new_gdb = $@"{folder_path}\新数据库.gdb";

                    // 分解范围要素
                    Arcpy.SplitByAttributes(clip_fc, new_gdb, clip_field);
                    // 收集分割后的地块
                    List<string> list_area = new_gdb.GetFeatureClassPathFromGDB();
                    // 分区处理
                    foreach (var area in list_area)
                    {
                        // 获取area_name
                        string area_name = area[(area.LastIndexOf(@"\")+1)..];

                        pw.AddProcessMessage(10, time_base, $"分割范围:{area_name}");

                        // 复制GDB
                        BaseTool.CopyAllFiles(original_gdb, folder_resultGDB + @$"\{area_name}.gdb");
                        // 裁剪要素类
                        List<string> list_fc = original_gdb.GetFeatureClassPathFromGDB();
                        foreach (var fc in list_fc)
                        {
                            pw.AddProcessMessage(2, time_base, $"{fc[(fc.LastIndexOf(@"\") + 1)..]}", Brushes.Gray);
                            Arcpy.Clip(fc, area, fc.Replace(original_gdb, folder_resultGDB + @$"\{area_name}.gdb"));
                        }
                        // 裁剪栅格
                        List<string>list_raster = original_gdb.GetRasterPath();
                        foreach (var raster in list_raster)
                        {
                            pw.AddProcessMessage(2, time_base, $"{raster[(raster.LastIndexOf(@"\") + 1)..]}", Brushes.Gray);
                            Arcpy.RasterClip(raster, raster.Replace(original_gdb, folder_resultGDB + @$"\{area_name}.gdb"), area);
                        }
                    }

                    // 删除中间数据
                    Directory.Delete(new_gdb, true);
                });
                pw.AddProcessMessage(70, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void comboxClipFeature_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(comboxClipFeature);
        }

        private void comboxClipField_DropOpen(object sender, EventArgs e)
        {
            string clip = comboxClipFeature.Text;
            UITool.AddTextFieldsToCombox(clip, comboxClipField);
        }

        private async void btn_check_Click(object sender, RoutedEventArgs e)
        {
            string original_gdb = textOriginalGDB.Text;
            string clip_fc = comboxClipFeature.Text;
            string clip_field = comboxClipField.Text;
            string folder_resultGDB = textResultGDB.Text;

            // 打开检查框
            ProcessWindow pw = UITool.OpenProcessWindow(processwindow, "数据检查");
            pw.AddMessage("开始执行数据检查" + "\r", Brushes.Green);

            await QueuedTask.Run(() =>
            {
                // 检查要素类型是否正确
                string result_fcType = CheckTool.CheckFeatureClassType(clip_fc, "Polygon");
                pw.AddMessage(result_fcType, Brushes.Red);

                if (result_fcType.Contains("】的要素类型不是【"))
                {
                    return;
                }

                // 检查是否包含空值
                string result_value_cs = CheckTool.CheckFieldValueEmpty(clip_fc, clip_field);
                pw.AddMessage(result_value_cs, Brushes.Red);

                pw.AddProcessMessage(100, "检查完成。", Brushes.Blue);
            });
        }
    }
}
