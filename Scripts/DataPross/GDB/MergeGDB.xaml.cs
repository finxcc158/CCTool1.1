using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
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

namespace CCTool.Scripts.DataPross.GDB
{
    /// <summary>
    /// Interaction logic for MergeGDB.xaml
    /// </summary>
    public partial class MergeGDB : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MergeGDB()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "合并GDB数据库";

        private void openForldrButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string gdbFolder = textFolderPath.Text;
                string gdbName = text_gdbName.Text;

                // 判断参数是否选择完全
                if (gdbFolder == "" || gdbName == "")
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
                    pw.AddProcessMessage(10, "获取所有GDB文件");
                    // 获取所有GDB文件
                    List<string> gdbFiles = gdbFolder.GetAllGDBFilePaths();
                    pw.AddProcessMessage(10, time_base, "创建目标GDB");
                    // 创建合并GDB
                    string gdbPath = Arcpy.CreateFileGDB(gdbFolder, gdbName);
                    // 要素数据集列表
                    List<string> dataBaseNames = new List<string>();
                    // 要素类列表
                    List<string> featureClassNames = new List<string>();


                    foreach (string gdbFile in gdbFiles)
                    {
                        pw.AddProcessMessage(10, time_base, $"处理数据库：{gdbFile}");
                        // 获取FeatureClass
                        using (Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbFile))))
                        {
                            // 获取要素数据集
                            IReadOnlyList<FeatureDatasetDefinition> featureDatases = gdb.GetDefinitions<FeatureDatasetDefinition>();
                            // 新建要素数据集
                            if (featureDatases.Count > 0)
                            {
                                foreach (var featureDatase in featureDatases)
                                {
                                    string dbName = featureDatase.GetName();
                                    if (!dataBaseNames.Contains(dbName))   // 如果是新的，就创建
                                    {
                                        Arcpy.CreateFeatureDataset(gdbPath, dbName, featureDatase.GetSpatialReference());
                                    }
                                    dataBaseNames.Add(dbName);
                                }
                            }
                            
                            // 获取要素类
                            IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();
                            if (featureClasses.Count > 0)
                            {
                                foreach (var featureClass in featureClasses)
                                {
                                    string fcName = featureClass.GetName();
                                    FeatureClass fc = gdb.OpenDataset<FeatureClass>(fcName);
                                    // 获取要素类路径
                                    string fcPath = fc.GetPath().ToString().Replace("file:///", "").Replace("/", @"\");
                                    // 获取目标路径
                                    string targetPath = gdbPath + fcPath[(fcPath.IndexOf(".gdb")+4)..];

                                    if (!featureClassNames.Contains(fcName))   // 如果是新的，就复制要素类
                                    {
                                        Arcpy.CopyFeatures(fcPath, targetPath);
                                        featureClassNames.Add(fcName);
                                    }
                                    else   // 如果已经有要素了，就追加
                                    {
                                        Arcpy.Append(fcPath, targetPath);
                                    }
                                }
                            }
                        }
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
    }
}
