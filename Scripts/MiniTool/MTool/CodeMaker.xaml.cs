using CCTool.Scripts.Manager;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace CCTool.Scripts.MiniTool.MTool
{
    /// <summary>
    /// Interaction logic for CodeMaker.xaml
    /// </summary>
    public partial class CodeMaker : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public CodeMaker()
        {
            InitializeComponent();
            // 初始化combox
            combox_color.Items.Add("黑色");
            combox_color.Items.Add("灰色");
            combox_color.Items.Add("蓝色");
            combox_color.Items.Add("绿色");
            combox_color.Items.Add("浅蓝");
            combox_color.Items.Add("棕色");
            combox_color.Items.Add("黄绿色");
            combox_color.Items.Add("浅绿");
            combox_color.SelectedIndex = 0;

        }

        private void openPicPathButton_Click(object sender, RoutedEventArgs e)
        {
            textPicPath.Text = UITool.OpenDialogPicture();
        }

        private void openResultButton_Click(object sender, RoutedEventArgs e)
        {
            resultPath.Text = UITool.SaveDialogPicture();
        }

        private void btn_go_Click(object sender, RoutedEventArgs e)
        {
            string insidePicture = textPicPath.Text;
            string resultPicture = resultPath.Text;

            string link = textLink.Text;

            // 颜色
            string comboxColor = combox_color.Text;
            Color co = comboxColor switch
            {
                "黑色" => Color.Black,
                "灰色" => Color.Gray,
                "蓝色" => Color.Blue,
                "绿色" => Color.Green,
                "浅蓝" => Color.LightBlue,
                "棕色" => Color.Brown,
                "黄绿色" => Color.YellowGreen,
                "浅绿" => Color.LightGreen,
                _ => Color.Black,
            };

            //  生成二维码
            QRCodeGenerator codeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = codeGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.H);
            QRCode qRCode = new QRCode(qRCodeData);
            // 加logo
            Bitmap logo = new Bitmap(insidePicture);
            Bitmap bitmap = qRCode.GetGraphic(15, co, Color.White, logo, 22,2,true);
            Image image = Image.FromHbitmap(bitmap.GetHbitmap());

            image.Save(resultPicture);

            MessageBox.Show("生成成功！");
            this.Close();
        }
    }
}
