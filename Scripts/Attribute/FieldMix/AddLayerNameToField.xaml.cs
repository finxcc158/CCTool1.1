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

namespace CCTool.Scripts.Attribute.FieldMix
{
    /// <summary>
    /// Interaction logic for AddLayerNameToField.xaml
    /// </summary>
    public partial class AddLayerNameToField : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AddLayerNameToField()
        {
            InitializeComponent();
            UITool.AddFeatureLayersAndTablesToListbox(listbox_fc);
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "添加图层名称和路径到字段";

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                bool isAddName = (bool)checkBox_name.IsChecked;
                bool isAddPath = (bool)checkBox_path.IsChecked;
                string fieldName = txt_name.Text;
                string fieldPath = txt_path.Text;
                // 文本空值处理
                if (txt_name.Text == "") { fieldName = "图层名称"; }
                if (txt_path.Text == "") { fieldPath = "图层路径"; }

                // 判断参数是否选择完全
                if (isAddName == false && isAddPath ==false)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }
                if (listbox_fc.Items.Count==0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                pw.AddMessage("GO", Brushes.Green);
                Close();

                // 获取要素列表
                List<string> list_layer = listbox_fc.ItemsAsString();

                await QueuedTask.Run(() =>
                {
                    foreach (string layer in list_layer)
                    {
                        string layer_single = layer.GetLayerSingleName();
                        // 去除数字标记
                        if (layer_single.Contains("："))
                        {
                            layer_single = layer_single[..layer_single.IndexOf("：")];
                        }

                        pw.AddProcessMessage(5, time_base, $"处理要素或表：{layer_single}");
                        // 添加图层名称
                        if (isAddName)
                        {
                            pw.AddProcessMessage(5, time_base, $"添加字段：{fieldName}", Brushes.Gray);
                            // 添加字段
                            Arcpy.AddField(layer, fieldName, "TEXT");
                            // 计算字段
                            Arcpy.CalculateField(layer, fieldName, $"'{layer_single}'");
                        }
                        // 添加图层路径
                        if (isAddPath)
                        {
                            pw.AddProcessMessage(5, time_base, $"添加字段：{fieldPath}", Brushes.Gray);
                            // 获取路径
                            string path = layer.LayerSourcePath().Replace(@"\",@"\\");
                            // 添加字段
                            Arcpy.AddField(layer, fieldPath, "TEXT");
                            // 计算字段
                            Arcpy.CalculateField(layer, fieldPath, $"'{path}'");
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

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_fc.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listbox_fc.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = true;
            }
        }

        private void btn_unSelect_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_fc.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listbox_fc.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = false;
            }
        }
    }
}
