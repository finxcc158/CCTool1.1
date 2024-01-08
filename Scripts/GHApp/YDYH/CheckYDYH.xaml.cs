using ArcGIS.Core.CIM;
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

namespace CCTool.Scripts
{
    /// <summary>
    /// Interaction logic for CheckYDYH.xaml
    /// </summary>
    public partial class CheckYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CheckYDYH()
        {
            InitializeComponent();

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "检查用地用海字段";
        // 执行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string bm_field = combox_field_BM.Text;
                string mc_field = combox_field_MC.Text;
                string version = combox_version.Text;

                // 复制资源文件
                string folder = Project.Current.HomeFolderPath;

                string excelPath = "";
                if (version == "旧版")
                {
                    excelPath = "用地用海_DM_to_MC.xlsx";
                }
                else
                {
                    excelPath = "新版用地用海_DM_to_MC.xlsx";
                }

                string outputPath = System.IO.Path.Combine(folder, excelPath);

                // 判断参数是否选择完全
                if (fc_path == "" || bm_field == "" || mc_field == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                // 获取目标FeatureLayer
                FeatureLayer initlayer = fc_path.TargetFeatureLayer();
                
                Close();
                pw.AddProcessMessage(10, "获取参数");
                // 使用异步任务在后台进行编辑操作
                await QueuedTask.Run(() =>
                {
                    // 复制嵌入资源中的Excel文件
                    BaseTool.CopyResourceFile(@"CCTool.Data.Excel." + excelPath, outputPath);
                    pw.AddProcessMessage(10, time_base, "读取Excel文件，输出字典");
                    // 读取Excel文件，输出字典
                    Dictionary<string, string> dict = OfficeTool.GetDictFromExcel(outputPath + @"\Sheet1$");
                    pw.AddProcessMessage(10, time_base, "字段空值转换");
                    // 把字段值的空值先转成空字符串，避免后续计算不能通过
                    GisTool.ClearStringNull(fc_path, new List<string>() { bm_field, mc_field});

                    pw.AddProcessMessage(10, time_base, "添加字段【检查】");
                    // 添加一个检查字段
                    Arcpy.AddField(fc_path, "检查", "TEXT");

                    pw.AddProcessMessage(10, time_base, "检查字段");
                    // 打开要素图层的表格
                    var table = initlayer.GetTable();
                    // 定位到属性表的游标
                    using (var tableCursor = table.Search())
                    {
                        while (tableCursor.MoveNext())
                        {
                            // 获取当前记录的值
                            var row = tableCursor.Current;
                            var bm = row[bm_field];
                            var mc = row[mc_field];
                            // 定义错误信息文字
                            string err = "";
                            // 设置一个flag：是否符合规范
                            bool isOK = true;
                            // 检查编码字段
                            if (!dict.Keys.Contains(bm))
                            {
                                err += "BM错误;";
                                isOK = false;
                            }
                            // 检查名称字段
                            if (!dict.Values.Contains(mc))
                            {
                                err += "MC错误;";
                                isOK = false;
                            }
                            // 检查编码和名称是否一一对应
                            if (isOK)
                            {
                                if (dict[bm.ToString()] != mc.ToString())
                                {
                                    err += "BM和MC不匹配";
                                }
                            }
                            // 检查字段赋值
                            row["检查"] = err;
                            row.Store();
                        }
                    }
                    // 删除中间数据
                    File.Delete(outputPath);
                });
                pw.AddProcessMessage(70, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_field_BM_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_BM);
        }

        private void combox_field_MC_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_MC);
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }
    }
}
