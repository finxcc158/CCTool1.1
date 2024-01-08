using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.Manager;
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
using ArcGIS.Desktop.Framework.Dialogs;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using CCTool.Scripts.ToolManagers;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for GetWord.xaml
    /// </summary>
    public partial class GetWord : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GetWord()
        {
            InitializeComponent();
            Init();
        }

        // 初始化
        private void Init()
        {
            // combox_model框中添加3种模式，默认【中文】
            combox_model.Items.Add("中文");
            combox_model.Items.Add("英文");
            combox_model.Items.Add("数字");
            combox_model.Items.Add("特殊符号");
            combox_model.SelectedIndex = 0;
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "提取特定文字";

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_fc);
        }

        private void combox_field_in_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_in);
        }

        private void combox_field_out_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_field_out);
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取数据
                string layer_path = combox_fc.Text;
                string field_in = combox_field_in.Text;
                string field_out = combox_field_out.Text;
                string model = combox_model.Text;

                // 判断参数是否选择完全
                if (layer_path == "" || field_in == "" || field_out == "")
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
                    pw.AddProcessMessage(10, "获取属性表");
                    // 获取属性表
                    ArcGIS.Core.Data.Table tb = layer_path.TargetTable();
                    pw.AddProcessMessage(10, time_base, "提取" + model);
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
                                    // 获取输入字段的值
                                    var value_in = row[field_in].ToString();
                                    // 切片
                                    row[field_out] = value_in.GetWord(model);
                                    // 保存
                                    row.Store();
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
    }
}
