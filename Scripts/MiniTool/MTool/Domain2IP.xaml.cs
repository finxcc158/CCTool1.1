using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
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

namespace CCTool.Scripts.MiniTool
{
    /// <summary>
    /// Interaction logic for Domain2IP.xaml
    /// </summary>
    public partial class Domain2IP : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public Domain2IP()
        {
            InitializeComponent();
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            string domain = txtDomain.Text;

            if (domain == "" || domain == null)
            {
                MessageBox.Show("请输入域名！");
            }

            IPHostEntry hostEntry = null;

            try
            {
                hostEntry = Dns.GetHostEntry(domain);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的域名！");
                return;
            }
            
            if (hostEntry == null || hostEntry.AddressList == null)
            {
                MessageBox.Show("请输入正确的域名！");
            }
            else
            {
                // 解析IP
                StringBuilder stringBuilder= new StringBuilder();
                foreach (IPAddress ipAdress in hostEntry.AddressList)
                {
                    stringBuilder.Append(ipAdress.ToString()+"\r\n");
                    txtResult.Text = stringBuilder.ToString();
                }
            }
        }
    }
}
