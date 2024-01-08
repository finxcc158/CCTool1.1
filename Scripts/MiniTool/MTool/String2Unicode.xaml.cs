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

namespace CCTool.Scripts.MiniTool.MTool
{
    /// <summary>
    /// Interaction logic for String2Unicode.xaml
    /// </summary>
    public partial class String2Unicode : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public String2Unicode()
        {
            InitializeComponent();
        }

        private void txt_string_Changed(object sender, TextChangedEventArgs e)
        {
            // 获取汉字字符
            string targetString = txt_string.Text;
            
            if ((bool)ra_decimalism.IsChecked)   // 十进制
            {
                txt_unicode.Text = DecimalismChange(targetString);
            }
            else    // 十六进制
            {
                txt_unicode.Text = HexadecimalChange(targetString);
            }
            
        }

        private void hexadecimal_Checked(object sender, RoutedEventArgs e)
        {
            // 获取汉字字符
            string targetString = txt_string.Text;
            txt_unicode.Text = HexadecimalChange(targetString);
        }

        private void decimalism_Checked(object sender, RoutedEventArgs e)
        {
            // 获取汉字字符
            string targetString = txt_string.Text;
            txt_unicode.Text = DecimalismChange(targetString);
        }

        // 十六进制转换
        public string HexadecimalChange(string targetString)
        {
            try
            {
                // 获取bytes
                byte[] bytes = Encoding.Unicode.GetBytes(targetString);
                // 字符列表
                List<string> strList = new List<string>();

                string hexadecimalString = "";    // 十六进制
                // 获取字符列表
                for (int index = 0; index < bytes.Length; index += 2)
                {
                    strList.Add((string)bytes[index + 1].ToString("x").PadLeft(2, '0') + (string)bytes[index].ToString("x").PadLeft(2, '0'));
                }

                // 十六进制
                foreach (string str in strList)
                {
                    hexadecimalString += @$"\u{str}";
                }

                // 输出
                return hexadecimalString;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message+ee.StackTrace);
                return null;
            }
            
        }

        // 十进制转换
        public string DecimalismChange(string targetString)
        {
            try
            {
                // 获取bytes
                byte[] bytes = Encoding.Unicode.GetBytes(targetString);
                // 字符列表
                List<string> strList = new List<string>();

                string decimalismString = "";    // 十进制
                // 获取字符列表
                for (int index = 0; index < bytes.Length; index += 2)
                {
                    strList.Add((string)bytes[index + 1].ToString("x").PadLeft(2, '0') + (string)bytes[index].ToString("x").PadLeft(2, '0'));
                }

                // 十进制
                foreach (string str in strList)
                {
                    decimalismString += $"{Convert.ToInt32(str, 16)} ";
                }

                // 输出
                return decimalismString;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return null;
            }

        }
    }
}
