using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Brushes = System.Windows.Media.Brushes;
using Color = ArcGIS.Core.Internal.CIM.Color;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for ApplySymbologyYDYH.xaml
    /// </summary>
    public partial class ApplySymbologyYDYH : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ApplySymbologyYDYH()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "应用符号系统";

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string field = combox_field.Text;
                // 符号系统选择
                string symbol = "";
                if (rb_gk.IsChecked == true){symbol = "gk";}
                else if (rb_gk_new.IsChecked == true) { symbol = "gk_new"; }
                else if(rb_cg.IsChecked == true){ symbol = "cg"; }
                else if (rb_sd.IsChecked == true) { symbol = "sd"; }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();   //关闭窗口
                await QueuedTask.Run(() =>
                {
                    // 获取工程默认文件夹位置
                    var def_path = Project.Current.HomeFolderPath;
                    // 获取当前地图
                    var map = MapView.Active.Map;
                    // 获取图层
                    FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

                    // 如果选择的不是面要素或是无选择，则返回
                    if (ly.ShapeType != esriGeometryType.esriGeometryPolygon || ly == null)
                    {
                        pw.AddMessage("错误！请选择一个面要素！", Brushes.Red);
                        return;
                    }

                    if (symbol == "gk")
                    {
                        pw.AddProcessMessage(10, "复制【国空用地(旧版)】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.国空用地.lyrx", def_path + @"\国空用地.lyrx");
                        
                        pw.AddProcessMessage(40, time_base, "应用符号系统");
                        // 应用符号系统
                        Arcpy.ApplySymbologyFromLayer(ly, def_path + @"\国空用地.lyrx", "VALUE_FIELD YDYHFLMC " + field);
                        File.Delete(def_path + @"\国空用地.lyrx");
                    }

                    if (symbol == "gk_new")
                    {
                        pw.AddProcessMessage(10, "复制【国空用地(新版)】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.国空用地新版.lyrx", def_path + @"\国空用地新版.lyrx");

                        pw.AddProcessMessage(40, time_base, "应用符号系统");
                        // 应用符号系统
                        Arcpy.ApplySymbologyFromLayer(ly, def_path + @"\国空用地新版.lyrx", "VALUE_FIELD YDYHMC " + field);
                        File.Delete(def_path + @"\国空用地新版.lyrx");
                    }

                    else if (symbol == "cg")
                    {
                        pw.AddProcessMessage(10, "复制【村规用地】图层文件");
                        // 复制符号图层文件
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.村规用地.lyrx", def_path + @"\村规用地.lyrx");
                        pw.AddProcessMessage(40, time_base, "应用符号系统");
                        // 应用符号系统
                        Arcpy.ApplySymbologyFromLayer(ly, def_path + @"\村规用地.lyrx", "VALUE_FIELD YDYHFLMC " + field);
                        File.Delete(def_path + @"\村规用地.lyrx");
                    }
                    else if (symbol == "sd")
                    {
                        pw.AddProcessMessage(10, "复制【三调用地】图层文件");
                        // 复制符号图层文件  
                        BaseTool.CopyResourceFile(@"CCTool.Data.Layers.三调用地_混合.lyrx", def_path + @"\三调用地_混合.lyrx");
                        pw.AddProcessMessage(40, time_base, "应用符号系统");
                        // 生成三调图例字段
                        Arcpy.AddField(ly, "SDSymbol", "TEXT");
                        Arcpy.CalculateField(ly, "SDSymbol", "!DLBM!+!DLMC!");
                        // 应用符号系统
                        Arcpy.ApplySymbologyFromLayer(ly, def_path + @"\三调用地_混合.lyrx", "VALUE_FIELD SDSymbol SDSymbol");
                        File.Delete(def_path + @"\三调用地_混合.lyrx");
                    }

                    pw.AddProcessMessage(50, time_base, "工具运行完成！！！", Brushes.Blue);
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }


        private void combox_field_DropOpen(object sender, EventArgs e)
        {
            var map = MapView.Active.Map;
            // 获取图层
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

            UITool.AddTextFieldsToCombox(ly.Name, combox_field);
        }

        private void rb_sd_Checked(object sender, RoutedEventArgs e)
        {
            combox_field.IsEnabled = false;
        }

        private void rb_sd_Unchecked(object sender, RoutedEventArgs e)
        {
            combox_field.IsEnabled = true;
        }

    }
}
