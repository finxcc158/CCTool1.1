using ArcGIS.Core.Data;
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
using System.Threading;

namespace CCTool.Scripts.MiniTool.GetInfo
{
    /// <summary>
    /// Interaction logic for InfoFields.xaml
    /// </summary>
    public partial class InfoFields : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public InfoFields()
        {
            InitializeComponent();
        }

        private void combox_layer_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayerAndTableToCombox(combox_layer);
            
        }

        private async void combox_layer_DropClosed(object sender, EventArgs e)
        {
            // 清空文本
            tb_message.Document.Blocks.Clear();

            string lyName = combox_layer.Text;
            bool isAllMessage = (bool)checkbox_isSimple.IsChecked;
            await QueuedTask.Run(() =>
            {
                var fields = GisTool.GetFieldsFromTarget(lyName, "allof");

                if (isAllMessage)   // 全部信息
                {
                    tb_message.AddMessage($"字段名称      字段别名      字段类型      字段长度\r", Brushes.Green);
                    foreach (Field field in fields)
                    {
                        tb_message.AddMessage($"{field.Name}      {field.AliasName}      {field.FieldType}      {field.Length}\r", Brushes.BlueViolet);
                    }
                }
                else    // 如果只要字段名
                {
                    foreach (Field field in fields)
                    {
                        tb_message.AddMessage($"{field.Name}\r", Brushes.BlueViolet);
                    }
                }
            });
        }

        private async void checkbox_isSimple_checked(object sender, RoutedEventArgs e)
        {
            // 清空文本
            tb_message.Document.Blocks.Clear();

            string lyName = combox_layer.Text;

            await QueuedTask.Run(() =>
            {
                var fields = GisTool.GetFieldsFromTarget(lyName, "allof");

                tb_message.AddMessage($"字段名称      字段别名      字段类型      字段长度\r", Brushes.Green);
                foreach (Field field in fields)
                {
                    tb_message.AddMessage($"{field.Name}      {field.AliasName}      {field.FieldType}      {field.Length}\r", Brushes.BlueViolet);
                }
            });
        }

        private async void checkbox_isSimple_unChecked(object sender, RoutedEventArgs e)
        {
            // 清空文本
            tb_message.Document.Blocks.Clear();

            string lyName = combox_layer.Text;

            await QueuedTask.Run(() =>
            {
                var fields = GisTool.GetFieldsFromTarget(lyName, "allof");

                foreach (Field field in fields)
                {
                    tb_message.AddMessage($"{field.Name}\r", Brushes.BlueViolet);
                }
            });
        }
    }
}
