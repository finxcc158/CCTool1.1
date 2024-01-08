using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
using ArcGIS.Desktop.Mapping;
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
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for MergerCAD.xaml
    /// </summary>
    public partial class MergerCAD : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MergerCAD()
        {
            InitializeComponent();
            combox_type.Items.Add("点");
            combox_type.Items.Add("线");
            combox_type.Items.Add("面");
            combox_type.Items.Add("文字");
            combox_type.SelectedIndex = 1;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "合并文件夹下的所有CAD文件";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取默认数据库
                var def_gdb = Project.Current.DefaultGeodatabasePath;
                // 获取指标
                string folder_path = textFolderPath.Text;
                string featureClass_path = textFeatureClassPath.Text;

                // 判断参数是否选择完全
                if (folder_path == "" || featureClass_path == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 获取输出要素类型
                var featureclass_type = combox_type.Text switch
                {
                    "点" => "Point",
                    "线" => "Polyline",
                    "面" => "Polygon",
                    "文字" => "Annotation",
                    _ => "",
                };

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                // 异步执行
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(10, "获取所有CAD文件");
                    // 获取所有CAD文件
                    var files = folder_path.GetAllFiles(".dwg");
                    // 初始化一个输出要素列表
                    List<string> list_out_fc = new List<string>();
                    // 分解文件夹目录，获取文件名和路径字段值
                    int num = 1;
                    foreach (var file in files)
                    {
                        // 获取CAD文件名
                        string cad_name = file.Substring(file.LastIndexOf(@"\") + 1).Replace(".dwg", "");
                        pw.AddProcessMessage(5, time_base, $"解析CAD文件：{cad_name}");

                        // 定义输出要素名称
                        string out_fc = $@"{def_gdb}\TransForm{num}_{featureclass_type}";
                        string target_fc = $@"{file}\{featureclass_type}";
                        // 复制要素
                        Arcpy.CopyFeatures(target_fc, out_fc);

                        // 加入列表
                        list_out_fc.Add(out_fc);
                        num++;
                    }
                    pw.AddProcessMessage(10, time_base, "合并要素");
                    // 合并要素
                    string mergeFC = (string)Arcpy.Merge(list_out_fc, featureClass_path);
                    // 删除中间要素
                    foreach (var out_fc in list_out_fc)
                    {
                        Arcpy.Delect(out_fc);
                    }
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

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private void openFeatureClassButton_Click(object sender, RoutedEventArgs e)
        {
            textFeatureClassPath.Text = UITool.SaveDialogFeatureClass();
        }
    }
}
