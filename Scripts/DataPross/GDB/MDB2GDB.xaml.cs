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

namespace CCTool.Scripts.DataPross.GDB
{
    /// <summary>
    /// Interaction logic for MDB2GDB.xaml
    /// </summary>
    public partial class MDB2GDB : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MDB2GDB()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "MDB转GDB";

        private void openMDBButton_Click(object sender, RoutedEventArgs e)
        {
            textMDBPath.Text = UITool.OpenDialogMDB();
        }

        private void openGDBButton_Click(object sender, RoutedEventArgs e)
        {
            textGDBPath.Text= UITool.SaveDialogGDB();
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            // 打开进度框
            ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
            DateTime time_base = DateTime.Now;
            pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
            Close();

            pw.AddProcessMessage(10, "获取输入参数");

            // 获取输入参数
            //string mdbPath = textMDBPath.Text;
            //string gdbPath = textGDBPath.Text;

            string mdbPath = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\1-输入文件\4.规划数据库\村庄规划数据库.mdb";
            string gdbPath = @"C:\Users\Administrator\Documents\ArcGIS\Projects\Test\1-输入文件\备用\靛墩村.gdb";

            // 注册GDAL
            //OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            //OSGeo.GDAL.Gdal.AllRegister();
            //OSGeo.OGR.Ogr.RegisterAll();

            //Ogr.RegisterAll();// 注册所有的驱动
            //Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //Gdal.SetConfigOption("SHAPE_ENCODING", "");

            //必须的，不然会报错 “OSGeo.GDAL.GdalPINVOKE”的类型初始值设定项引发异常
            //GdalConfiguration.ConfigureGdal();
            //GdalConfiguration.ConfigureOgr();//矢量
            //GdalConfiguration.ConfigureOgr();

            //Gdal.AllRegister();//对GDAL进行注册，必须的

            //// 打开mdb
            //DataSource mdbDataSource = Ogr.Open(mdbPath, 0);
            //// 创建 GDB 数据源
            //DataSource gdbDataSource = Ogr.Open(gdbPath, 1); // 1 表示以写入模式创建数据源

            //// 遍历 MDB 数据源中的每个图层，并将其复制到 GDB 数据源中
            //for (int layerIndex = 0; layerIndex < mdbDataSource.GetLayerCount(); layerIndex++)
            //{
            //    Layer mdbLayer = mdbDataSource.GetLayerByIndex(layerIndex);
            //    string gdbLayerName = mdbLayer.GetName();

            //    pw.AddProcessMessage(10, time_base, gdbLayerName);

            //// 创建 GDB 中的图层
            //LayerOptions layerOptions = new LayerOptions($"LAUNDER={gdbLayerName}");
            //Layer gdbLayer = gdbDataSource.CreateLayer(gdbLayerName, mdbLayer.GetSpatialRef(), mdbLayer.GetGeomType(), layerOptions);

            //// 复制字段结构
            //LayerDefn mdbLayerDefn = mdbLayer.GetLayerDefn();
            //for (int fieldIndex = 0; fieldIndex < mdbLayerDefn.GetFieldCount(); fieldIndex++)
            //{
            //    FieldDefn fieldDefn = mdbLayerDefn.GetFieldDefn(fieldIndex);
            //    gdbLayer.CreateField(fieldDefn);
            //}

            //// 复制要素
            //Feature mdbFeature;
            //while ((mdbFeature = mdbLayer.GetNextFeature()) != null)
            //{
            //    Feature gdbFeature = gdbLayer.CreateFeature(mdbFeature);
            //    gdbFeature.Dispose(); // 释放资源
            //}

            //// 关闭数据源
            //mdbDataSource.Dispose();
            //gdbDataSource.Dispose();
        }
    }
}
