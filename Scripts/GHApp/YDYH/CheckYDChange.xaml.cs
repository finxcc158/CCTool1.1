using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using System;
using ArcGIS.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts
{
    /// <summary>
    /// Interaction logic for CheckYDChange.xaml
    /// </summary>
    public partial class CheckYDChange : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CheckYDChange()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "现状规划用地变化检查";

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            // 获取指标
            string fc_xz_txt = combox_fc_xz.Text;
            string fc_gh_txt = combox_fc_gh.Text;
            string field_xz = combox_field_xz.Text;
            string field_gh = combox_field_gh.Text;
            string field_change = @"变化";

            // 判断参数是否选择完全
            if (fc_xz_txt == "" || fc_gh_txt == "" || field_xz == "" || field_gh == "")
            {
                MessageBox.Show("有必选参数为空！！！");
                return;
            }

            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                pw.AddProcessMessage(10, "获取外部参数");

                var map = MapView.Active.Map;

                // 输出检查结果要素
                string def_path = Project.Current.HomeFolderPath;
                string DefalutGDB = Project.Current.DefaultGeodatabasePath;
                string identityFeatureClass = DefalutGDB + @"\identityFeatureClass";
                string checkRezult = DefalutGDB + @"\checkRezult";

                Close();
                // 异步执行
                await QueuedTask.Run(() =>
                {
                    pw.AddProcessMessage(20, time_base, "标识要素");

                    // 复制输入要素
                    Arcpy.CopyFeatures(fc_xz_txt, DefalutGDB + @"\tem_xz");
                    Arcpy.CopyFeatures(fc_gh_txt, DefalutGDB + @"\tem_gh");
                    // 更改输入字段
                    Arcpy.AlterField(DefalutGDB + @"\tem_xz", field_xz, "现状_" + field_xz, "现状_" + field_xz);
                    Arcpy.AlterField(DefalutGDB + @"\tem_gh", field_gh, "规划_" + field_gh, "规划_" + field_gh);

                    // 标识
                    Arcpy.Identity(DefalutGDB + @"\tem_xz", DefalutGDB + @"\tem_gh", identityFeatureClass);

                    pw.AddProcessMessage(30, time_base, "添加字段");
                    // 添加字段
                    Arcpy.AddField(identityFeatureClass, field_change, "TEXT");

                    pw.AddProcessMessage(20, time_base, "计算字段，找出变化图斑");

                    // 计算字段，找出变化图斑
                    using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(DefalutGDB)));
                    using FeatureClass featureClass = gdb.OpenDataset<FeatureClass>("identityFeatureClass");
                    using (RowCursor rowCursor = featureClass.Search(null, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // 获取2个检查字段的值
                                var fd_xz = row["现状_" + field_xz];
                                var fd_gh = row["规划_" + field_gh];
                                if (fd_xz is not null && fd_gh is not null)
                                {
                                    if (fd_xz.ToString() != fd_gh.ToString())
                                    {
                                        // 赋值
                                        row[field_change] = @$"【{fd_xz}】-->【{fd_gh}】";
                                    }
                                    row.Store();
                                }
                            }
                        }
                    }
                    pw.AddProcessMessage(40, time_base, "提取变化图斑");
                    // 提取变化图斑
                    string sql = $"{field_change} IS NOT NULL";
                    Arcpy.Select(identityFeatureClass, checkRezult, sql, true);
                    // 删除过程要素
                    Arcpy.Delect(identityFeatureClass);
                    Arcpy.Delect(DefalutGDB + @"\tem_xz");
                    Arcpy.Delect(DefalutGDB + @"\tem_gh");
                    // 删除字段
                    Arcpy.DeleteField(checkRezult, new List<string>() { "现状_" + field_xz, "规划_" + field_gh, field_change }, "KEEP_FIELDS");
                });
                pw.AddProcessMessage(20, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            } 
        }

        private void combox_fc_xz_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc_xz);
        }

        private void combox_field_xz_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc_xz.Text, combox_field_xz);
        }

        private void combox_fc_gh_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc_gh);
        }

        private void combox_field_gh_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc_gh.Text, combox_field_gh);
        }
    }
}
