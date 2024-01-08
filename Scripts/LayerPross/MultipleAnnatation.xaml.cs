using ArcGIS.Core.CIM;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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

namespace CCTool.Scripts.UI.ProWindow
{
    /// <summary>
    /// Interaction logic for MultipleAnnatation.xaml
    /// </summary>
    public partial class MultipleAnnatation : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MultipleAnnatation()
        {
            InitializeComponent();
            Init();
        }
 
        // 初始化
        private void Init()
        {
            List<TextBox> textBoxes = new List<TextBox>()
            {
                txt_DB,txt_DF,txt_LB,txt_RB,txt_LF,txt_RF,txt_UB,txt_UF
            };
            foreach (TextBox textBox in textBoxes)
            {
                textBox.Visibility = Visibility.Hidden;
            }
        }

        private void checkBox_add_Checked(object sender, RoutedEventArgs e)
        {
            List<TextBox> textBoxes = new List<TextBox>()
            {
                txt_DB,txt_DF,txt_LB,txt_RB,txt_LF,txt_RF,txt_UB,txt_UF
            };

            foreach (TextBox textBox in textBoxes)
            {
                textBox.Visibility = Visibility.Visible;
            }
        }

        private void checkBox_add_UnChecked(object sender, RoutedEventArgs e)
        {
            List<TextBox> textBoxes = new List<TextBox>()
            {
                txt_DB,txt_DF,txt_LB,txt_RB,txt_LF,txt_RF,txt_UB,txt_UF
            };

            foreach (TextBox textBox in textBoxes)
            {
                textBox.Visibility = Visibility.Hidden;
            }
        }

        private async void combox_Left_DropOpen(object sender, EventArgs e)
        {
            // 获取图层名称
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            string lyName = await QueuedTask.Run(() => {return ly.LayerSourcePath(); });

            // 添加字段
            UITool.AddFieldsToCombox(lyName, combox_Left);

        }

        private async void combox_Up_DropOpen(object sender, EventArgs e)
        {
            // 获取图层名称
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            string lyName = await QueuedTask.Run(() => { return ly.LayerSourcePath(); });
            // 添加字段
            UITool.AddFieldsToCombox(lyName, combox_Up);
        }

        private async void combox_Down_DropOpen(object sender, EventArgs e)
        {
            // 获取图层名称
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            string lyName = await QueuedTask.Run(() => { return ly.LayerSourcePath(); });
            // 添加字段
            UITool.AddFieldsToCombox(lyName, combox_Down);
        }

        private async void combox_Right_DropOpen(object sender, EventArgs e)
        {
            // 获取图层名称
            FeatureLayer ly = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;
            string lyName = await QueuedTask.Run(() => { return ly.LayerSourcePath(); });
            // 添加字段
            UITool.AddFieldsToCombox(lyName, combox_Right);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取参数
                string up = combox_Up.Text;
                string down = combox_Down.Text;
                string left = combox_Left.Text;
                string right = combox_Right.Text;

                // 避免填入空值
                List<string> par= new List<string>()
                {
                    up, down, left, right
                };

                for (int i = 0; i < par.Count; i++)
                {
                    if (par[i] == "")
                    {
                        par[i] = i.ToString();
                    }
                }

                // 避免同字段
                List<string> par2 = par.Distinct().ToList();
                string par_dis = "";
                foreach (var pa in par2)
                {
                    string str = $"[{pa}],";
                    par_dis += str;
                }
                string par_dis2 = par_dis[..(par_dis.Length-1)];

                string uf = "\""+ txt_UF.Text+ "\"";
                string ub = "\"" +txt_UB.Text + "\"";
                string df = "\"" + txt_DF.Text + "\"";
                string db = "\"" + txt_DB.Text + "\"";
                string lf = "\"" + txt_LF.Text + "\"";
                string lb = "\"" + txt_LB.Text + "\"";
                string rf = "\"" + txt_RF.Text + "\"";
                string rb = "\"" + txt_RB.Text + "\"";

                // 获取工程默认文件夹位置
                var def_path = Project.Current.HomeFolderPath;
                // 获取当前地图
                var map = MapView.Active.Map;
                // 获取图层
                FeatureLayer featureLayer = MapView.Active.GetSelectedLayers().FirstOrDefault() as FeatureLayer;

                await QueuedTask.Run(() =>
                {
                    // 获取图层定义
                    var lyrDefn = featureLayer.GetDefinition() as CIMFeatureLayer;
                    
                    // 获取标注
                    var listLabelClasses = lyrDefn.LabelClasses.ToList();
                    CIMLabelClass theLabelClass = listLabelClasses.FirstOrDefault();

                    // 设置标注语言为python
                    theLabelClass.ExpressionEngine = LabelExpressionEngine.Python;
                    
                    // 设置标注内容
                    string code = $"import re\r\ndef FindLabel ({par_dis2}):\r\n  a={uf}+[{par[0]}]+{ub}\r\n  b={df}+[{par[1]}]+{db}\r\n  c={lf}+[{par[2]}]+{lb}\r\n  d={rf}+[{par[3]}]+{rb}\r\n  xs=0.5722\r\n  va_up = re.findall(u'[\\u4e00-\\u9fa5]+',a)\r\n  result_up = ''\r\n  if len(va_up) > 0:\r\n      for i in range(0, len(va_up)):\r\n          result_up += va_up[i]\r\n  len_up=len(result_up)\r\n  len_up_other=len(a)-len_up\r\n  s_up=len_up+len_up_other*xs\r\n  va_down = re.findall(u'[\\u4e00-\\u9fa5]+',b)\r\n  result_down = ''\r\n  if len(va_down) > 0:\r\n      for i in range(0, len(va_down)):\r\n          result_down += va_down[i]\r\n  len_down=len(result_down)\r\n  len_down_other=len(b)-len_down\r\n  s_down=len_down+len_down_other*xs\r\n  va_left = re.findall(u'[\\u4e00-\\u9fa5]+',c)\r\n  result_left = ''\r\n  if len(va_left) > 0:\r\n      for i in range(0, len(va_left)):\r\n          result_left += va_left[i]\r\n  len_left=len(result_left)\r\n  len_left_other=len(c)-len_left\r\n  s_left=int(len_left+len_left_other*xs)\r\n  va_right = re.findall(u'[\\u4e00-\\u9fa5]+',d)\r\n  result_right = ''\r\n  if len(va_right) > 0:\r\n      for i in range(0, len(va_right)):\r\n          result_right += va_right[i]\r\n  len_right=len(result_right)\r\n  len_right_other=len(d)-len_right\r\n  s_right=int(len_right+len_right_other*xs)\r\n  if s_up>s_down:\r\n    s=int(s_up)\r\n  else:\r\n    s=int(s_down)\r\n  p=\"<ALIGN horizontal = 'center'>\" +\"<CLR alpha='0'>\" + \"\"+'\u2007'*s_left*2+\"\" + \"</CLR>\"+ \"\"+a+\"\"+\"<CLR alpha='0'>\" + \"\"+'\u2007'*s_right*2+\"\" + \"</CLR>\"+ \"</ALIGN>\"+'\\n'+\"<LIN leading = '-5' leading_type = 'extra'>\"+\"\"+c+\"\"+\"<CHR spacing = '-10'>\" + '—'*s + \"</CHR>\"+\"\"+d+\"\"+\"</LIN>\"+'\\n'+\"<LIN leading = '-3' leading_type = 'extra'>\"+ \"<ALIGN horizontal = 'center'>\" +\"<CLR alpha='0'>\" + \"\"+'\u2007'*s_left*2+\"\" + \"</CLR>\"+ \"\"+b+\"\"+\"<CLR alpha='0'>\" + \"\"+'\u2007'*s_right*2+\"\" + \"</CLR>\"+ \"</ALIGN>\"+\"</LIN>\"\r\n  return p";
                    
                    theLabelClass.Expression = code;
                    // 应用标注设置
                    lyrDefn.LabelClasses[0] = theLabelClass; // 假设只有一个标注类别

                    // 应用标注
                    featureLayer.SetDefinition(lyrDefn);

                    // 打开标注
                    if (!featureLayer.IsLabelVisible) { featureLayer.SetLabelVisibility(true); }
                });
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
            
                
        }
    }
}
