using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
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
    /// Interaction logic for CopyFields.xaml
    /// </summary>

    // 创建一个字段类
    public class FieldDef
    {
        public string fldName { get; set; }
        public string fldAlias { get; set; }
        public string fldType { get; set; }
        public int fldLength { get; set; }
    }


    public partial class CopyFields : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CopyFields()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "复制字段";

        private void combox_fc_before_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_fc_before);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);

                pw.AddProcessMessage(10, "获取相关参数");
                // 参数获取
                string fc_before = combox_fc_before.Text;
                var fc_after = listbox_targetFeature.Items;
                var fileds = listbox_field.Items;

                // 判断参数是否选择完全
                if (fc_before == "" || listbox_targetFeature.Items.Count == 0 || listbox_field.Items.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }
                Close();

                // 获取参数listbox
                List<string> fieldNames = UITool.GetCheckboxStringFromListBox(listbox_field);
                List<string> targetFeatureClasses = UITool.GetCheckboxStringFromListBox(listbox_targetFeature);


                await QueuedTask.Run(() =>
                {
                    List<FieldDef> fieldDefs = new List<FieldDef>();

                    pw.AddProcessMessage(20, time_base, @"获取字段属性");
                    // 获取字段属性
                    foreach (string fieldName in fieldNames)
                    {
                        FieldDef fd = new FieldDef();
                        FeatureLayer featureLayer = fc_before.TargetFeatureLayer();
                        var inspector = new Inspector();
                        inspector.LoadSchema(featureLayer);
                        // 获取属性
                        foreach (var att in inspector)
                        {
                            // 如果符合字段名
                            if (att.FieldName == fieldName)
                            {
                                fd.fldName = att.FieldName;
                                fd.fldAlias = att.FieldAlias;
                                fd.fldType = att.FieldType.ToString();
                                fd.fldLength = att.Length;
                            }
                        }
                        // 加入字段集合
                        fieldDefs.Add(fd);
                    }

                    // 复制字段
                    foreach (string targetFeatureClass in targetFeatureClasses)
                    {
                        foreach (var fd in fieldDefs)
                        {
                            pw.AddProcessMessage(10, time_base, $"【{targetFeatureClass}】__ 复制字段：{fd.fldName}");
                            Arcpy.AddField(targetFeatureClass, fd.fldName, fd.fldType, fd.fldAlias, fd.fldLength);
                        }
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

        private void combox_fc_before_DropClose(object sender, EventArgs e)
        {
            try
            {
                string fcPath = combox_fc_before.Text;

                // 生成字段列表
                if (combox_fc_before.Text != "")
                {
                    UITool.AddFieldsToListBox(listbox_field,fcPath);
                }

                // 将剩余要素图层放在目标图层中
                UITool.AddFeatureLayersAndTablesToListbox(listbox_targetFeature, new List<string>() { fcPath });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }

        }

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listbox_field.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = true;
            }
        }

        private void btn_unSelect_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_field = listbox_field.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = false;
            }
        }

        private void btn_select_fc_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_targetFeature.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_targetFeature = listbox_targetFeature.Items;
            foreach (CheckBox item in list_targetFeature)
            {
                item.IsChecked = true;
            }
        }

        private void btn_unSelect_fc_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_targetFeature.Items.Count == 0)
            {
                MessageBox.Show("列表内没有字段！");
                return;
            }

            var list_targetFeature = listbox_targetFeature.Items;
            foreach (CheckBox item in list_targetFeature)
            {
                item.IsChecked = false;
            }
        }
    }
}
