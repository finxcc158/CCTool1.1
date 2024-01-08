using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
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
using MessageBox = System.Windows.MessageBox;

namespace CCTool.Scripts.GHApp.YDYH
{
    /// <summary>
    /// Interaction logic for UpdataYDYH.xaml
    /// </summary>
    public partial class UpdataYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public UpdataYDYH()
        {
            InitializeComponent();
            // 版本
            combox_version.Items.Add("旧版");
            combox_version.Items.Add("新版");
            combox_version.SelectedIndex= 0;

            UpdataLabel(combox_YDYH, "用地用海合并字段_旧.xlsx");
        }

        // 更新代码名称
        public void UpdataLabel(ComboBox comboBox, string excelFile)
        {
            comboBox.Items.Clear();
            // 复制模板，并获取List
            string def_path = Project.Current.HomeFolderPath;
            string excel_map = def_path + @"\"+ excelFile;
            BaseTool.CopyResourceFile(@$"CCTool.Data.Excel.{excelFile}", excel_map);
            List<string> list = OfficeTool.GetListFromExcel(excel_map + @"\sheet1$", 0);
            // 加载用地类型
            foreach (string item in list)
            {
                comboBox.Items.Add(item);
                comboBox.SelectedIndex = 0;
            }
        }


        private void combox_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lyName = combox_fc.Text;
                string YDYH = combox_YDYH.Text;
                string ydbmField = combox_bm.Text;
                string ydmcField = combox_mc.Text;
                List<string> fieldNameList = new List<string>() { ydbmField, ydmcField };

                await QueuedTask.Run(() =>
                {

                    // 获取活动地图视图中选定的要素集合
                    var selectedSet = MapView.Active.Map.GetSelection();

                    // 将选定的要素集合转换为字典形式
                    var selectedList = selectedSet.ToDictionary();
                    // 创建一个新的 Inspector 对象以检索要素属性
                    var inspector = new Inspector();

                    // 遍历每个选定图层及其关联的对象 ID
                    foreach (var layer in selectedList)
                    {
                        // 获取图层和关联的对象 ID
                        FeatureLayer mapMember = layer.Key as FeatureLayer;

                        List<long> oids = layer.Value;
                        // 使用当前图层的第一个对象 ID 加载 Inspector
                        inspector.Load(mapMember, oids[0]);

                        // 遍历当前图层中的每个对象 ID
                        foreach (var oid in oids)
                        {
                            // 使用当前对象 ID 加载 Inspector
                            inspector.Load(mapMember, oid);
                            // 修改字段的值
                            string mc = YDYH.GetWord("中文");
                            string bm = YDYH.Replace(mc, "");
                            inspector[ydbmField] = bm;
                            inspector[ydmcField] = mc;

                            // 更新要素
                            inspector.Apply();
                        }

                        // 保存编辑
                        Project.Current.SaveEditsAsync();
                    }
                });

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_bm_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_bm);
        }

        private void combox_mc_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_fc.Text, combox_mc);
        }

        private void combox_version_Closed(object sender, EventArgs e)
        {
            if (combox_version.Text == "旧版")
            {
                UpdataLabel(combox_YDYH, "用地用海合并字段_旧.xlsx");
            }
            else
            {
                UpdataLabel(combox_YDYH, "用地用海合并字段_新.xlsx");
            }
            
        }
    }
}
