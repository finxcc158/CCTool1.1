using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using SharpCompress.Common;
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
using System.Xml;
using static System.Net.WebRequestMethods;

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for ExportLayout.xaml
    /// </summary>
    public partial class ExportLayout : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ExportLayout()
        {
            InitializeComponent();
        }


        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "导出布局（批量）";

        private void openFolderButton_Click(object sender, RoutedEventArgs e)
        {
            textFolderPath.Text = UITool.OpenDialogFolder();
        }

        private async void btn_go_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 参数获取
                string filePath = textFolderPath.Text;
                int dpi = int.Parse(text_dpi.Text);
                string pic_type = "";
                if (rb_jpg.IsChecked == true)
                {
                    pic_type = "jpg";
                }
                else if (rb_png.IsChecked == true)
                {
                    pic_type = "png";
                }
                else if (rb_pdf_series.IsChecked == true)
                {
                    pic_type = "pdf_ss";
                }

                // 获取布局列表
                List<string> layoutList = UITool.GetStringFromListbox(listBox_layout);

                // 判断参数是否选择完全
                if (filePath == "" || text_dpi.Text == "" || layoutList.Count == 0)
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                pw.AddProcessMessage(10, "获取当前工程中的所有Layout");

                // 获取当前工程中的所有LayoutProjectItem
                IEnumerable<LayoutProjectItem> layoutProjects = Project.Current.GetItems<LayoutProjectItem>();

                await QueuedTask.Run(() =>
                {
                    foreach (LayoutProjectItem layoutProject in layoutProjects)
                    {
                        // 获取layout 
                        Layout layout = layoutProject.GetLayout();

                        // JPG图片属性
                        JPEGFormat JPG = new JPEGFormat()
                        {
                            HasWorldFile = true,
                            Resolution = dpi,               // 分辨率
                            OutputFileName = filePath + @"\" + layout.Name + @".jpg",      // 输出路径
                        };
                        // PNG图片属性
                        PNGFormat PNG = new PNGFormat()
                        {
                            HasWorldFile = true,
                            HasTransparentBackground = true,   // 透明底
                            Resolution = dpi,               // 分辨率
                            OutputFileName = filePath + @"\" + layout.Name + @".png",      // 输出路径
                        };
                        // PDF图片属性
                        PDFFormat PDF = new PDFFormat()
                        {
                            OutputFileName = filePath + @"\" + layout.Name + @".pdf",      // 输出路径
                            Resolution = dpi,               // 分辨率
                            DoCompressVectorGraphics = true,   // 是否压缩矢量图形
                            DoEmbedFonts = true,            // 是否执行嵌入字体         
                            HasGeoRefInfo = true,             // 是否具有地理参考信息
                            ImageCompression = ImageCompression.Adaptive,   // 图形压缩.自适应
                            ImageQuality = ImageQuality.Best,           // 图形质量
                            LayersAndAttributes = LayersAndAttributes.LayersAndAttributes   // 图层  属性
                        };

                        // 如果在所选的布局中，则打印
                        if (layoutList.Contains(layout.Name))
                        {
                            pw.AddProcessMessage(20, time_base, "导出布局：" + layout.Name);

                            // 设置地图系列的导出方式
                            var mapSeriesExportOptions = new MapSeriesExportOptions()
                            {
                                // 导出内容，包括【All, Current, SelectedIndexFeatures】
                                ExportPages = ExportPages.All,
                                // 导出单个PDF，也可以按名称导出多个PDF
                                ExportFileOptions = ExportFileOptions.ExportAsSinglePDF,
                                // 分组
                                DoOrderPagesByGrouping = true,
                            };

                            // 导出JPG
                            if (pic_type == "jpg")
                            {
                                layout.Export(JPG);
                            }
                            // 导出PNG
                            if (pic_type == "png")
                            {
                                layout.Export(PNG);
                            }
                            // 导出PDF地图系列
                            if (pic_type == "pdf_ss")
                            {
                                layout.Export(PDF, mapSeriesExportOptions);
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

        private void listBox_layout_Load(object sender, RoutedEventArgs e)
        {
            // 在列表框中加入Layouts
            UITool.AddLayoutsToListbox(listBox_layout);
        }
    }
}
