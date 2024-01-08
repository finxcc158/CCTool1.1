using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
    /// Interaction logic for FieldClear.xaml
    /// </summary>
    public partial class FieldClear : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public FieldClear()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "清洗字段值";

        private void combox_fc_DropOpen(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_fc);
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string input_fc = combox_fc.Text;
                // 符号系统选择
                string model = "";
                if (rb_string_clearSpace.IsChecked == true) { model = "string_clearSpace"; }
                else if (rb_string_clearNone.IsChecked == true) { model = "string_clearNone"; }
                else if (rb_num_none2zero.IsChecked == true) { model = "num_none2zero"; }
                else if (rb_num_zero2none.IsChecked == true) { model = "num_zero2none"; }

                // 判断参数是否选择完全
                if (input_fc == "")
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
                    pw.AddMessage($"获取符合要求的字段");
                    // 获取所有字符串、数字字段
                    List<Field> text_fields = GisTool.GetFieldsFromTarget(input_fc, "text");
                    List<Field> float_fields = GisTool.GetFieldsFromTarget(input_fc, "float");
                    List<Field> int_fields = GisTool.GetFieldsFromTarget(input_fc, "int");

                    float_fields.AddRange(int_fields);

                    if (model == "string_clearSpace")        // 字符串：清除空格
                    {
                        foreach (var field in text_fields)
                        {
                            pw.AddProcessMessage(10, time_base, $"清除字符串空格__{field.Name}", Brushes.Gray);
                            GisTool.ClearTextSpace(input_fc, field.Name);
                        }
                    }
                    else if (model == "string_clearNone")        // 字符串：清除空值
                    {
                        foreach (var field in text_fields)
                        {
                            pw.AddProcessMessage(10, time_base, $"清除字符串null值__{field.Name}", Brushes.Gray);
                            GisTool.ClearTextNull(input_fc, field.Name);
                        }
                    }
                    else if (model == "num_none2zero")        // 数值：空值转0
                    {
                        foreach (var field in float_fields)
                        {
                            pw.AddProcessMessage(10, time_base, $"清除数字型null值__{field.Name}", Brushes.Gray);
                            GisTool.ClearMathNull(input_fc, field.Name);
                        }
                    }
                    else if (model == "num_zero2none")        // 数值：0转空值
                    {
                        foreach (var field in float_fields)
                        {
                            pw.AddProcessMessage(10, time_base, $"0值转null值__{field.Name}", Brushes.Gray);
                            GisTool.Zero2Null(input_fc, field.Name);
                        }
                    }
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
