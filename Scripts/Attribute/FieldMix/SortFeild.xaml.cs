using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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
    /// Interaction logic for SortFeild.xaml
    /// </summary>
    public partial class SortFeild : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public SortFeild()
        {
            InitializeComponent();
        }

        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private void btn_up_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.SelectedItem == null)
            {
                MessageBox.Show("请选择一个字段！");
            }
            else
            {
                // 获取选定项的索引
                int selectedIndex = listbox_field.SelectedIndex;

                if (selectedIndex > 0)
                {
                    // 获取ListBox的Items集合
                    var items = listbox_field.Items;
                    // 从Items集合中移除选定的项
                    var selectedItem = listbox_field.SelectedItem;
                    items.Remove(selectedItem);
                    // 在前一个位置重新插入选定的项
                    items.Insert(selectedIndex - 1, selectedItem);
                    // 更新ListBox的选定项
                    listbox_field.SelectedItem = selectedItem;
                }
            }
        }

        private void btn_down_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.SelectedItem == null)
            {
                MessageBox.Show("请选择一个字段！");
            }
            else
            {
                // 获取选定项的索引
                int selectedIndex = listbox_field.SelectedIndex;

                if (selectedIndex<listbox_field.Items.Count-1)
                {
                    // 获取ListBox的Items集合
                    var items = listbox_field.Items;
                    // 从Items集合中移除选定的项
                    var selectedItem = listbox_field.SelectedItem;
                    items.Remove(selectedItem);
                    // 在前一个位置重新插入选定的项
                    items.Insert(selectedIndex + 1, selectedItem);
                    // 更新ListBox的选定项
                    listbox_field.SelectedItem = selectedItem;
                }
            }
        }

        private async void combox_fc_DropClose(object sender, EventArgs e)
        {
            // 打开SHP文件夹
            string ly = combox_fc.Text;
            if (ly!="")
            {
                var fields = await QueuedTask.Run(() =>
                {
                    return GisTool.GetFieldsFromTarget(ly);
                });
                // 清除listbox
                listbox_field.Items.Clear();
                // 生成SHP要素列表
                if (fields is not null)
                {
                    foreach (Field file in fields)
                    {
                        if (file.FieldType == FieldType.OID || file.FieldType == FieldType.Geometry)
                        {
                            continue;
                        }
                        // 将shp文件做成checkbox放入列表中
                        listbox_field.Items.Add(file.Name);
                    }
                }
            }
        }

        private void btn_top_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.SelectedItem == null)
            {
                MessageBox.Show("请选择一个字段！");
            }
            else
            {
                // 获取选定项的索引
                int selectedIndex = listbox_field.SelectedIndex;

                if (selectedIndex > 0)
                {
                    // 获取ListBox的Items集合
                    var items = listbox_field.Items;
                    // 从Items集合中移除选定的项
                    var selectedItem = listbox_field.SelectedItem;
                    items.Remove(selectedItem);
                    // 在前一个位置重新插入选定的项
                    items.Insert(0, selectedItem);
                    // 更新ListBox的选定项
                    listbox_field.SelectedItem = selectedItem;
                }
            }
        }

        private void btn_bottom_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_field.SelectedItem == null)
            {
                MessageBox.Show("请选择一个字段！");
            }
            else
            {
                // 获取选定项的索引
                int selectedIndex = listbox_field.SelectedIndex;

                if (selectedIndex < listbox_field.Items.Count - 1)
                {
                    // 获取ListBox的Items集合
                    var items = listbox_field.Items;
                    // 从Items集合中移除选定的项
                    var selectedItem = listbox_field.SelectedItem;
                    items.Remove(selectedItem);
                    // 在前一个位置重新插入选定的项
                    items.Insert(listbox_field.Items.Count, selectedItem);
                    // 更新ListBox的选定项
                    listbox_field.SelectedItem = selectedItem;
                }
            }
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前地图视图
            MapView mapView = MapView.Active;
            // 获取指标
            string ly = combox_fc.Text;
            var fields = listbox_field.Items;
            List<string> fieldOrder = new List<string>();
            foreach ( string field in fields )
            {
                listbox_field.Items.Add(field);
            }
            await QueuedTask.Run(() =>
            {
                // 根据图层名找到当前图层
                var map = MapView.Active.Map;
                FeatureLayer init_featurelayer = map.FindLayers(ly).OfType<FeatureLayer>().FirstOrDefault();

                if (init_featurelayer != null)
                {
                    // 获取要素类的字段定义
                    var fields = init_featurelayer.GetFeatureClass().GetDefinition().GetFields();


                    // 重新排列字段的位置
                    var orderedFields = fields.OrderBy(field => fieldOrder.IndexOf(field.Name));

                    // Create a new table definition with the updated field order
                    var fcDef = init_featurelayer.GetFeatureClass().GetDefinition();
                }

            });
            
        }

    }
}