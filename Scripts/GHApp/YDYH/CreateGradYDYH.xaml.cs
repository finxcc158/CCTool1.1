using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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
    /// Interaction logic for CreateGradYDYH.xaml
    /// </summary>
    public partial class CreateGradYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CreateGradYDYH()
        {
            InitializeComponent();
            Init();       // 初始化
        }

        // 初始化
        private void Init()
        {
            // combox_model框中添加3种模式，默认【大类】
            combox_model.Items.Add("大类");
            combox_model.Items.Add("中类");
            combox_model.Items.Add("小类");
            combox_model.SelectedIndex = 0;

            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "生成分级用地用海编码与名称";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        // 将图层字段加入到Combox列表中
        private void combox_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field);
        }

        // 执行
        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string fcPath = combox_fc.Text;
                string bmField = combox_field.Text;
                bool isMC = (bool)checkbox_isMC.IsChecked;

                string version = combox_version.Text;

                // 判断参数是否选择完全
                if (fcPath == "" || bmField == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 模式转换
                int model = 1;
                if (combox_model.Text == "中类") { model = 2; }
                else if (combox_model.Text == "小类") { model = 3; }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                Close();
                // 根据需求生成三级用地编码和名称
                await QueuedTask.Run(() =>
                {
                    string def_folder = Project.Current.HomeFolderPath;     // 工程默认文件夹位置
                    string excelName = "";
                    if (version == "旧版")
                    {
                        excelName = "用地用海_DM_to_MC";
                    }
                    else
                    {
                        excelName = "新版用地用海_DM_to_MC";
                    }

                    string output_excel = def_folder + @$"\{excelName}.xlsx";
                    BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelName}.xlsx", output_excel);

                    if (model >= 1)
                    {
                        pw.AddProcessMessage(10, "计算大类编码");
                        Arcpy.AddField(fcPath, "BM_1", "TEXT");     // 添加大类编码
                        Arcpy.CalculateField(fcPath, "BM_1", $"!{bmField}![:2]");   // 计算大类编码
                        if (isMC == true)
                        {
                            pw.AddProcessMessage(20, time_base, "计算大类名称");
                            Arcpy.AddField(fcPath, "MC_1", "TEXT");     // 添加大类名称
                            GisTool.AttributeMapper(fcPath, "BM_1", "MC_1", output_excel+@"\sheet1$");   // 编码转名称
                        }
                    }
                    if (model >= 2)
                    {
                        pw.AddProcessMessage(20, time_base, "计算中类编码");
                        Arcpy.AddField(fcPath, "BM_2", "TEXT");     // 添加中类编码
                        string code = "def ss(a):\r\n    if len(a)>2:\r\n        return a[:4]\r\n    else:\r\n        return \"\"";
                        Arcpy.CalculateField(fcPath, "BM_2", $"ss(!{bmField}!)", code);   // 计算中类编码

                        if (isMC == true)
                        {
                            pw.AddProcessMessage(20, time_base, "计算中类名称");
                            Arcpy.AddField(fcPath, "MC_2", "TEXT");     // 添加中类名称
                            GisTool.AttributeMapper(fcPath, "BM_2", "MC_2", output_excel + @"\sheet1$");   // 编码转名称
                        }
                    }
                    if (model >= 3)
                    {
                        pw.AddProcessMessage(20, time_base, "计算小类编码");
                        Arcpy.AddField(fcPath, "BM_3", "TEXT");     // 添加小类编码
                        string code = "def ss(a):\r\n    if len(a)>4:\r\n        return a[:6]\r\n    else:\r\n        return \"\"";
                        Arcpy.CalculateField(fcPath, "BM_3", $"ss(!{bmField}!)", code);   // 计算小类编码
                        if (isMC == true)
                        {
                            pw.AddProcessMessage(20, time_base, "计算小类名称");
                            Arcpy.AddField(fcPath, "MC_3", "TEXT");     // 添加小类名称
                            GisTool.AttributeMapper(fcPath, "BM_3", "MC_3", output_excel + @"\sheet1$");   // 编码转名称
                        }
                    }
                    // 删除中间数据
                    File.Delete(output_excel);
                    
                    pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);
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
