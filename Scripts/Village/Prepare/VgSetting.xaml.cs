using ApeFree.DataStore;
using ApeFree.DataStore.Local;
using ArcGIS.Desktop.Internal.Core;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for VgSetting.xaml
    /// </summary>
    public partial class VgSetting : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        // 设置一个store
        private LocalStore<Settings> store;

        public VgSetting()
        {
            InitializeComponent();

            combox_pic.Items.Add("现状用地图");
            combox_pic.Items.Add("规划用地图");
            combox_pic.Items.Add("管制边界图");

            combox_basemap.Items.Add("MapBox");
            combox_basemap.Items.Add("天地图");

            // 创建数据存储文件
            string savePath = @"D:\ProSDKSettings\Settings.txt";
            store = StoreFactory.Factory.CreateLocalStore<Settings>(new ApeFree.DataStore.Local.LocalStoreAccessSettings(savePath));
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("请输入一个整数型数字！！");
                return;
            }
            
        }
        // 窗体加载
        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                // 加载store
                store.Load();
                // 参数获取
                textDPI.Text = store.Value.dpi.ToString();
                // 地图
                combox_basemap.SelectedIndex = store.Value.basemapIndex;
                
                listBox.Items.Clear();
                foreach (var item in store.Value.listPic)
                {
                    listBox.Items.Add(item.ToString());
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }
        // 窗体关闭
        private void Form_Closed(object sender, EventArgs e)
        {
            try
            {
                // 保存参数
                store.Value.dpi = int.Parse(textDPI.Text);
                store.Value.listPic = listBox.Items.Cast<string>().ToList();
                store.Value.basemapIndex= combox_basemap.SelectedIndex;
                // 保存store
                store.Save();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_pic_DropClosed(object sender, EventArgs e)
        {
            listBox.Items.Add(combox_pic.Text);
        }
    }

    public class Settings
    {
        /// 参数设置
        // 导出图纸的DPI
        public int dpi { get; set; } = 300;
        // 地图
        public int basemapIndex { get; set; } = 0;
        // 导出图纸列表
        public List<string> listPic { get; set; } = new List<string>() { } ;
        // 工具框尺寸
        public Size formSize { get; set; } = new Size(400, 200);
        // 工具框位置
        public Point formLocaltion { get; set; } = new Point(200, 200);
    }
}
