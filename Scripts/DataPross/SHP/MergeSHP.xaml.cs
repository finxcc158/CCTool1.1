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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for MergeSHP.xaml
    /// </summary>
    public partial class MergeSHP : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MergeSHP()
        {
            InitializeComponent();
        }
        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "合并文件夹下的所有SHP文件";

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
                string featureClass_path = textFeatureClassPath.Text;
                string shp_path = def_path + @"\ShpFiles";

                // 判断参数是否选择完全
                if (folder_path == "" || featureClass_path == "")
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
                    // 复制shp文件夹用作后续处理
                    if (Directory.Exists(shp_path))
                    {
                        Directory.Delete(shp_path, true);
                    }
                    BaseTool.CopyAllFiles(folder_path, shp_path);
                    pw.AddProcessMessage(10, "获取所有shp文件");
                    // 获取所有shp文件
                    var files = shp_path.GetAllFiles(".shp");
                    // 获取路径标签
                    string tab = folder_path.Substring(folder_path.LastIndexOf(@"\") + 1);
                    pw.AddProcessMessage(10, time_base, "解析shp文件，添加标记字段");
                    // 分解文件夹目录，获取文件名和路径字段值
                    foreach (string file in files)
                    {
                        // 获取名称和路径
                        string short_path = file.Replace(def_path + @"\", "");
                        int index1 = short_path.LastIndexOf(@"\");      // 最后一个【"\"】的位置
                        int index2 = short_path.LastIndexOf(@".shp");  // 最后一个【".shp"】的位置
                        string name = short_path.Substring(index1+ 1, index2 - index1 - 1);
                        string path = file.Replace( $@"\{name}.shp", "").Replace(@"\", @"/");
                        // 添加2个标记字段
                        Arcpy.AddField(file, "shpName", "TEXT");
                        Arcpy.AddField(file, "shpPath", "TEXT");
                        Arcpy.CalculateField(file, "shpName", "\"" + name + "\"");
                        Arcpy.CalculateField(file, "shpPath", "\"" + path + "\"");
                    }
                    pw.AddProcessMessage(40, time_base, "合并要素");
                    // 合并要素
                    Arcpy.Merge(files, featureClass_path);
                    // 删除复制的shp文件夹
                    Directory.Delete(shp_path, true);
                    // 将要素类添加到当前地图
                    var map = MapView.Active.Map;
                    LayerFactory.Instance.CreateLayer(new Uri(featureClass_path), map);

                });
                pw.AddProcessMessage(40, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            textFeatureClassPath.Text = UITool.SaveDialogFeatureClass();
        }

        private void openSHPButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }
    }
}
