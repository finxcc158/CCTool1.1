using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Editing;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for CalculateFields.xaml
    /// </summary>
    public partial class CalculateFields : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CalculateFields()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "批量计算字段";

        // 定义一个字段列表，用于存储所有要素的字段
        List<string> field_list = new List<string>();

        // 获取所有字段
        private async void GetAllFields()
        {
            string gdb_path = textGDBPath.Text;
            field_list.Clear();

            await QueuedTask.Run(() =>
            {
                if (gdb_path != "")
                {
                    // 打开GDB
                    using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
                    // 获取所有要素类
                    IReadOnlyList<FeatureClassDefinition> featureClasses = gdb.GetDefinitions<FeatureClassDefinition>();
                    foreach (FeatureClassDefinition featureClass in featureClasses)
                    {
                        using (FeatureClass fc = gdb.OpenDataset<FeatureClass>(featureClass.GetName()))
                        {
                            // 获取字段列表
                            IReadOnlyList<ArcGIS.Core.Data.Field> fields = featureClass.GetFields();
                            foreach (var field in fields)
                            {
                                // 如果不重复，则加入列表
                                if (!field_list.Contains(field.Name))
                                {
                                    field_list.Add(field.Name);
                                }
                            }
                        }
                    }
                }
            });
        }

        // combox加入字段
        private void AddFields(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            foreach (var field in field_list)
            {
                comboBox.Items.Add(field);
            }
        }

        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text = UITool.OpenDialogGDB();
            if (textGDBPath.Text !="")
            {
                GetAllFields();   // 获取所有字段
            }
            else
            {
                field_list.Clear();
            }
        }

        private void DrowOpen_fd_01(object sender, EventArgs e)
        {
            AddFields(com_fd_01);
        }

        private void DrowOpen_fd_02(object sender, EventArgs e)
        {
            AddFields(com_fd_02);
        }

        private void DrowOpen_fd_03(object sender, EventArgs e)
        {
            AddFields(com_fd_03);
        }

        private void DrowOpen_fd_04(object sender, EventArgs e)
        {
            AddFields(com_fd_04);
        }

        private void DrowOpen_fd_05(object sender, EventArgs e)
        {
            AddFields(com_fd_05);
        }

        private void DrowOpen_fd_06(object sender, EventArgs e)
        {
            AddFields(com_fd_06);
        }

        private void AddDic(Dictionary<string, string>dic, string key, string value)
        {
            if (dic.ContainsKey(key))
            {
                dic.Add(key, value);
            }
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string gdb_path = textGDBPath.Text;
                string db_name = combox_db.Text;

                // 参数集合
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (!dic.ContainsKey(com_fd_01.Text)) { dic.Add(com_fd_01.Text, txt_01.Text); }
                if (!dic.ContainsKey(com_fd_02.Text)) { dic.Add(com_fd_02.Text, txt_02.Text); }
                if (!dic.ContainsKey(com_fd_03.Text)) { dic.Add(com_fd_03.Text, txt_03.Text); }
                if (!dic.ContainsKey(com_fd_04.Text)) { dic.Add(com_fd_04.Text, txt_04.Text); }
                if (!dic.ContainsKey(com_fd_05.Text)) { dic.Add(com_fd_05.Text, txt_05.Text); }
                if (!dic.ContainsKey(com_fd_06.Text)) { dic.Add(com_fd_06.Text, txt_06.Text); }


                // 判断参数是否选择完全
                if (gdb_path == "")
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
                    pw.AddMessage("获取所有要素类或表格");
                    // 获取所有要素类或表格
                    List<string> fcs = gdb_path.GetFeatureClassAndTablePath();
                    // 如果选择了数据集
                    if (db_name != "")     
                    {
                        List<string> tem= new List<string>();
                        foreach (var fc in fcs)
                        {
                            if (fc.Contains(@$"\{db_name}\"))
                            {
                                tem.Add(fc);
                            }
                        }
                        fcs = tem;
                    }

                    // 修改字段
                    foreach (var fc in fcs)
                    {
                        string fc_name = fc[(fc.LastIndexOf(@"\") + 1)..];  // 获取要素名

                        pw.AddProcessMessage(10, time_base, $"处理要素：{fc_name}");
                        // 获取要素的所有字段
                        List<string> fds = GisTool.GetFieldsNameFromTarget(fc);

                        foreach (var item in dic)
                        {
                            if (fds.Contains(item.Key))   // 如果存在字段，则计算
                            {
                                // 执行计算字段
                                Arcpy.CalculateField(fc, item.Key, item.Value);
                            }
                        }
                    }

                    
                });
                pw.AddProcessMessage(100, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private async void combox_db_DropOpen(object sender, EventArgs e)
        {
            combox_db.Items.Clear();
            
            string gdb = textGDBPath.Text;

            if (gdb == "")
            {
                return;
            }

            List<string> db_names = await QueuedTask.Run(() => 
            {
                return gdb.GetDataBaseName();
            });

            foreach (var item in db_names)
            {
                combox_db.Items.Add(item);
            }
        }
    }
}
