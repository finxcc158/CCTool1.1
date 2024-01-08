using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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
using CheckBox = System.Windows.Controls.CheckBox;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for DegreeChange.xaml
    /// </summary>
    public partial class DegreeChange : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public DegreeChange()
        {
            InitializeComponent();
            // combox_model框中添加2种转换模式，默认【度分秒转十进制度】
            combox_model.Items.Add("度分秒转十进制度");
            combox_model.Items.Add("十进制度转度分秒");
            combox_model.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "度分秒转十进制度";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string fc_path = combox_fc.Text;
                string model = combox_model.Text;

                // 判断参数是否选择完全
                if (fc_path == "" || model == "" || listBox_field.Items.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 字段列表
                List<string> list_field = new List<string>();
                foreach (CheckBox item in listBox_field.Items)
                {
                    if (item.IsChecked == true)
                    {
                        list_field.Add(item.Content.ToString());
                    }
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddProcessMessage(10, "获取参数");
                Close();

                await QueuedTask.Run(() =>
                {
                    foreach (var oldFieldName in list_field)
                    {
                        pw.AddProcessMessage(20, time_base, @$"处理字段： {oldFieldName}");

                        // 新字段名
                        string modelName = model[(model.IndexOf("转")+1)..];
                        string oldNameUpdata = oldFieldName;
                        if (oldFieldName.Contains("_转_"))
                        {
                            oldNameUpdata = oldFieldName[..oldFieldName.IndexOf("_转_")];
                        }
                        string newFieldName = $@"{oldNameUpdata}_转_{modelName}";
                        // 转换模式对应的字段类型
                        string FieldType = model switch
                        {
                            "度分秒转十进制度" => "Double",
                            "十进制度转度分秒" => "TEXT",
                            _ => "",
                        };

                        // 创建字段
                        Arcpy.AddField(fc_path, newFieldName, FieldType);

                        // 获取Table
                        var tb = fc_path.TargetTable();
                        // 字段计算
                        using (ArcGIS.Core.Data.Table table = tb)
                        {
                            using (RowCursor rowCursor = table.Search(null, false))
                            {
                                TableDefinition tableDefinition = table.GetDefinition();

                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        if (model == "度分秒转十进制度")      // 【度分秒转十进制度】模式
                                        {
                                            var value_text = row[oldFieldName];    // 获取输入的度分秒字段值
                                            if (value_text is null)       // 先排除空值的情况
                                            {
                                                row[newFieldName] = null;
                                            }
                                            else if (value_text.ToString() == "")       // 空字符的情况
                                            {
                                                row[newFieldName] = null;
                                            }
                                            else                // 【度分秒转十进制度】主流程
                                            {
                                                // 初始化度分秒符号的位置
                                                int index1 = -1;
                                                int index2 = -1;
                                                int index3 = -1;
                                                // 定义度分秒可能的符号
                                                List<string> list_degree = new List<string>() { "度", "°" };
                                                List<string> list_minutes = new List<string>() { "分", "′", "'" };
                                                List<string> list_seconds = new List<string>() { "秒", "″", "\"" };
                                                // 找到度分秒符号的位置
                                                foreach (var item in list_degree)
                                                {
                                                    if (value_text.ToString().IndexOf(item) != -1)
                                                    {
                                                        index1 = value_text.ToString().IndexOf(item);
                                                    }
                                                }
                                                foreach (var item in list_minutes)
                                                {
                                                    if (value_text.ToString().IndexOf(item) != -1)
                                                    {
                                                        index2 = value_text.ToString().IndexOf(item);
                                                    }
                                                }
                                                foreach (var item in list_seconds)
                                                {
                                                    if (value_text.ToString().IndexOf(item) != -1)
                                                    {
                                                        index3 = value_text.ToString().IndexOf(item);
                                                    }
                                                }
                                                // 计算度分秒数值
                                                double degree = double.Parse(value_text.ToString().Substring(0, index1));
                                                double minutes = double.Parse(value_text.ToString().Substring(index1 + 1, index2 - index1 - 1));
                                                double seconds = double.Parse(value_text.ToString().Substring(index2 + 1, index3 - index2 - 1));
                                                // 计算赋值
                                                row[newFieldName] = degree + minutes / 60 + seconds / 3600;
                                            }
                                        }
                                        else if (model == "十进制度转度分秒")      // 【十进制度转度分秒】模式
                                        {
                                            var value_float = row[oldFieldName];

                                            if (value_float is null)          // 先排除空值的情况
                                            {
                                                row[newFieldName] = null;
                                            }
                                            else if (double.Parse(value_float.ToString()) == 0)       // 0值的情况
                                            {
                                                row[newFieldName] = null;
                                            }
                                            else             // 【十进制度转度分秒】主流程
                                            {
                                                double value = double.Parse(value_float.ToString());
                                                // 计算度分秒的值
                                                int degree = (int)(value / 1);
                                                int minutes = (int)(value % 1 * 60 / 1);
                                                double seconds = (value % 1 * 60 - minutes) * 60;
                                                // 合并为字符串
                                                row[newFieldName] = degree.ToString() + "°" + minutes.ToString() + "′" + seconds.ToString("0.00") + "″";
                                            }
                                        }
                                        // 保存
                                        row.Store();
                                    }
                                }
                            }
                        }
                    }
                });
                pw.AddProcessMessage(80, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_model_Closed(object sender, EventArgs e)
        {
            string model = combox_model.Text;
            if (model=="")
            {
                combox_field.IsEditable = false;    
            }
            else if (model == "度分秒转十进制度")
            {
                combox_field.IsEditable = true;
                combox_field.Items.Clear();
                lb.Content = "选择度分秒字段 (文本型)：";
                listBox_field.Items.Clear();
            }
            else if (model == "十进制度转度分秒")
            {
                combox_field.IsEditable = true;
                combox_field.Items.Clear();
                lb.Content = "选择十进制度字段 (双精度型)：";
                listBox_field.Items.Clear();
            }
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            string model = combox_model.Text;

            if (model == "度分秒转十进制度")
            {
                UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field);
            }
            else if (model == "十进制度转度分秒")
            {
                UITool.AddFloatFieldsToCombox(combox_fc.Text, combox_field);
            }
        }

        private void combox_field_Closed(object sender, EventArgs e)
        {
            List<string> conList = new List<string>();
            foreach (CheckBox item in listBox_field.Items)
            {
                string con = item.Content.ToString();
                if (!conList.Contains(con))
                {
                    conList.Add(con);
                }
            }
            if (!conList.Contains(combox_field.Text) && combox_field.Text != "")
            {
                // 将txt文件做成checkbox放入列表中
                CheckBox cb = new CheckBox();
                cb.Content = combox_field.Text;
                cb.IsChecked = true;
                listBox_field.Items.Add(cb);
            }
        }
    }
}
