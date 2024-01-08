using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CheckBox = System.Windows.Controls.CheckBox;
using Field = ArcGIS.Core.Data.Field;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for PHShp2Txt.xaml
    /// </summary>
    public partial class PHShp2Txt : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public PHShp2Txt()
        {
            InitializeComponent();
            Init();
        }

        // 初始化
        private void Init()
        {
            //txtFolder.Text = @"C:\Users\Administrator\Desktop\shp文件";
            //txtFolder2.Text = @"C:\Users\Administrator\Desktop\txt文件";
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "进出平衡@SHP转TXT";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取默认数据库
                var gdb = Project.Current.DefaultGeodatabasePath;
                // 获取工程默认文件夹位置
                var def_path = Project.Current.HomeFolderPath;
                // 获取指标
                string folder_path = txtFolder.Text;
                string folder_txt = txtFolder2.Text;
                var cb_shps = listbox_shp.Items;
                string field_mc = combox_mc.Text;
                string field_yt = combox_yt.Text;
                int digit_xy = int.Parse(textbox_xy.Text);

                // 判断参数是否选择完全
                if (folder_path == "" || field_mc == "" || field_yt == "" || folder_txt == "" || cb_shps.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                // 获取所有选中的shp
                List<string> list_shpPath = new List<string>();
                foreach (CheckBox shp in cb_shps)
                {
                    if (shp.IsChecked == true)
                    {
                        list_shpPath.Add(folder_path + shp.Content);
                    }
                }

                pw.AddMessage("获取参数", Brushes.Green);

                await QueuedTask.Run(() =>
                {
                    foreach (string fullPath in list_shpPath)
                    {
                        // 初始化写入txt的内容
                        string txt_all= "[地块坐标]" +"\r";

                        pw.AddProcessMessage(10, time_base, fullPath);
                        string shp_name = fullPath[(fullPath.LastIndexOf(@"\") + 1)..];  // 获取要素名
                        string shp_path = fullPath[..(fullPath.LastIndexOf(@"\"))];  // 获取shp名

                        // 打开shp
                        FileSystemConnectionPath fileConnection = new FileSystemConnectionPath(new Uri(shp_path), FileSystemDatastoreType.Shapefile);
                        using FileSystemDatastore shapefile = new FileSystemDatastore(fileConnection);
                        // 获取FeatureClass
                        FeatureClass featureClass = shapefile.OpenDataset<FeatureClass>(shp_name);

                        using (RowCursor rowCursor = featureClass.Search())
                        {
                            int featureIndex = 1;
                            while (rowCursor.MoveNext())
                            {
                                using (Feature feature = rowCursor.Current as Feature)
                                {
                                    // 获取地块名称，地块性质
                                    Row row = feature as Row;
                                    string ft_name = "";
                                    string ft_type = "";
                                    var areaName = row[field_mc];
                                    var areaType = row[field_yt];
                                    if (areaName != null) { ft_name = areaName.ToString(); }
                                    if (areaType != null) { ft_type = areaType.ToString(); }

                                    // 获取面要素的JSON文字
                                    Geometry polygon = feature.GetShape();
                                    string js = polygon.ToJson().ToString();

                                    // 解析JSON文字
                                    // 取坐标点文字
                                    string cod = js[(js.IndexOf("[[[") + 3)..js.IndexOf("]]]")];
                                    // 坐标点列表
                                    List<string> list_xy = cod.Split("]]").ToList();
                                    for (int i = 0; i < list_xy.Count; i++)
                                    {
                                        // 坐标行
                                        List<string> xy_detils = list_xy[i].Replace(",[[", "").Split("],[").ToList();

                                        // 加一行title
                                        int count = xy_detils.Count;    // 点的个数
                                        string title = $"{count},{ft_name},面,{ft_type},@ " + "\r";
                                        txt_all += title;

                                        for (int j = 0; j < xy_detils.Count; j++)
                                        {
                                            // 点序号
                                            int index = j + 1;
                                            if (index == xy_detils.Count) { index = 1; }
                                            // XY坐标点
                                            string x =Math.Round(double.Parse(xy_detils[j].Split(",")[0]), digit_xy).ToString();
                                            string y = Math.Round(double.Parse(xy_detils[j].Split(",")[1]), digit_xy).ToString();
                                            // 加入文本
                                            txt_all += $"J{index},{featureIndex},{x},{y}\r";
                                        }
                                    }

                                }
                                featureIndex++;
                            }
                        }
                        // 写入txt文件
                        string txtPath = @$"{folder_txt}\{shp_name.Replace(".shp", "")}.txt";
                        if (File.Exists(txtPath))
                        {
                            File.Delete(txtPath);
                        }
                        File.WriteAllText(txtPath, txt_all);
                    }
                });
                pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开SHP文件夹
            string folder = UITool.OpenDialogFolder();
            txtFolder.Text = folder;
            // 清除listbox
            listbox_shp.Items.Clear();
            // 生成SHP要素列表
            if (txtFolder.Text != "")
            {
                // 获取所有shp文件
                var files = folder.GetAllFiles(".shp");
                foreach ( var file in files)
                {
                    // 将shp文件做成checkbox放入列表中
                    CheckBox cb = new CheckBox();
                    cb.Content = file.Replace(folder, "");
                    cb.IsChecked= true;
                    listbox_shp.Items.Add(cb);
                }
            }
        }

        private async void combox_mc_Open(object sender, EventArgs e)
        {
            try
            {
                // 先清空
                combox_mc.Items.Clear();
                // 获取一个shp
                string folder = txtFolder.Text;
                var cb_shps = listbox_shp.Items;

                foreach ( CheckBox cb in cb_shps)
                {
                    // 获取所有字段
                    string shpPath = folder + cb.Content.ToString();
                    List<Field> list_field = await QueuedTask.Run(() =>
                    {
                        return GisTool.GetFieldsFromTarget(shpPath, "text");
                    });
                    // 将字段填入combox
                    foreach (Field field in list_field)
                    {
                        combox_mc.Items.Add(field.Name);
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private async void combox_yt_Open(object sender, EventArgs e)
        {
            try
            {
                // 先清空
                combox_yt.Items.Clear();
                // 获取一个shp
                string folder = txtFolder.Text;
                var cb_shps = listbox_shp.Items;

                foreach (CheckBox cb in cb_shps)
                {
                    // 获取所有字段
                    string shpPath = folder + cb.Content.ToString();
                    List<Field> list_field = await QueuedTask.Run(() =>
                    {
                        return GisTool.GetFieldsFromTarget(shpPath, "text");
                    });
                    // 将字段填入combox
                    foreach (Field field in list_field)
                    {
                        if (!combox_yt.Items.Contains(field.Name))
                        {
                            combox_yt.Items.Add(field.Name);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void openTXTFolderButton_Click(object sender, RoutedEventArgs e)
        {
            txtFolder2.Text = UITool.OpenDialogFolder();
        }
    }
}
