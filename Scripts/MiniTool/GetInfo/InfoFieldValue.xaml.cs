using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Data;
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
using Table = ArcGIS.Core.Data.Table;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CCTool.Scripts.ToolManagers;
using ArcGIS.Desktop.Internal.Mapping.Symbology;

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoFieldValue.xaml
    /// </summary>
    public partial class InfoFieldValue : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoFieldValue()
        {
            InitializeComponent();
        }

        private async void combox_field_DropClosed(object sender, EventArgs e)
        {
            // 清空文本
            tb_message.Document.Blocks.Clear();
            
            string lyName = combox_layer.Text;
            string fieldName = combox_field.Text;

            Dictionary<string, long> fieldValues = await QueuedTask.Run(() =>
            {
                return lyName.GetFieldValuesDic(fieldName);
            });

            // 加入文本
            int index = 1;
            foreach (var fieldValue in fieldValues)
            {
                if (index < 1000)
                {
                    tb_message.AddMessage($"{fieldValue.Key}                      【{fieldValue.Value}】行\r", Brushes.BlueViolet);
                }
                else
                {
                    MessageBox.Show("超过1000个值，后续的不再显示！");
                    break;
                }
                index++;
            }
        }

        private void combox_field_DropDown(object sender, EventArgs e)
        {
            string lyName = combox_layer.Text;
            UITool.AddFieldsToCombox(lyName, combox_field);
        }

        private void combox_layer_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_layer);
        }
    }
}
