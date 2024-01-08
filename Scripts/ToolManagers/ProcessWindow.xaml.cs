using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using ArcGIS.Desktop.Framework.Dialogs;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace CCTool.Scripts.Manager
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProcessWindow()
        {
            InitializeComponent();

        }

        // 变更进度条的进度【100%】
        public void AddProcess(int percent)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                pb.Value += percent;
            });
        }

        // 添加信息框文字
        public void AddMessage(string add_text, SolidColorBrush solidColorBrush = null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (solidColorBrush == null)
                {
                    solidColorBrush = Brushes.Black;
                }
                // 创建一个新的TextRange对象，范围为新添加的文字
                TextRange newRange = new TextRange(tb_message.Document.ContentEnd, tb_message.Document.ContentEnd)
                {
                    Text = add_text
                };
                // 设置新添加文字的颜色
                newRange.ApplyPropertyValue(TextElement.ForegroundProperty, solidColorBrush);
                // 设置新添加文字的样式
                newRange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            });
        }

        // 添加信息框文字_时间
        public void AddTime(DateTime time_base)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                DateTime time_now = DateTime.Now;
                TimeSpan time_span = time_now - time_base;
                string time_total = time_span.ToString()[..time_span.ToString().LastIndexOf(".")];
                string add_text = "………………用时" + time_total + "\r";

                // 创建一个新的TextRange对象，范围为新添加的文字
                TextRange newRange = new TextRange(tb_message.Document.ContentEnd, tb_message.Document.ContentEnd)
                {
                    Text = add_text
                };
                // 设置新添加文字的颜色为灰色
                newRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
                // 设置新添加文字的样式为斜体
                newRange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            });
        }

        // 综合显示进度【AddTime+AddProcess+AddMessage】
        public void AddProcessMessage(int percent, DateTime time_base, string add_text, SolidColorBrush solidColorBrush = null)
        {
            AddProcess(percent);
            AddTime(time_base);
            AddMessage(add_text, solidColorBrush);
        }

        // 综合显示进度【AddTime+AddMessage+AddProcess】
        public void AddProcessMessage(string add_text, int percent, DateTime time_base, SolidColorBrush solidColorBrush = null)
        {
            AddMessage(add_text, solidColorBrush);
            AddProcess(percent);
            AddTime(time_base);
        }

        // 综合显示进度【AddMessage+AddProcess】
        public void AddProcessMessage(int percent, string add_text, SolidColorBrush solidColorBrush = null)
        {
            AddProcess(percent);
            AddMessage(add_text, solidColorBrush);
        }
    }
}
