using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing.Controls;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using CCTool.Scripts.ToolManagers;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;

namespace CCTool.Scripts.DataPross.FeatureClasses
{


    /// <summary>
    /// Interaction logic for AttributeReader.xaml
    /// </summary>
    public partial class AttributeReader : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public AttributeReader()
        {
            InitializeComponent();
        }

        // 定义一个进度框
        private ProcessWindow processwindow = null;
        string tool_name = "属性读取";

        private void combox_origin_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_origin_fc);
        }

        private void combox_identity_fc_DropDown(object sender, EventArgs e)
        {
            UITool.AddFeatureLayersToCombox(combox_identity_fc);
        }

        private async void btn_go_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取数据
                string origin_fc = combox_origin_fc.Text;
                string identity_fc = combox_identity_fc.Text;

                string origin_field = combox_origin_field.Text;
                string identity_field = combox_identity_field.Text;

                string defGDB = Project.Current.DefaultGeodatabasePath;

                double prop = double.Parse(propTXT.Text)/100;

                // 判断参数是否选择完全
                if (origin_fc == "" || identity_fc == "" || origin_field == "" || identity_field == "")
                {
                    MessageBox.Show("有必选参数为空！！！");
                    return;
                }

                // 打开进度框
                ProcessWindow pw = UITool.OpenProcessWindow(processwindow, tool_name);
                DateTime time_base = DateTime.Now;
                pw.AddMessage("开始执行" + tool_name + "工具…………" + time_base + "\r", Brushes.Green);
                Close();

                await QueuedTask.Run(() =>
                {
                    // 获取原始图层和标识图层
                    FeatureLayer originFeatureLayer = origin_fc.TargetFeatureLayer();
                    FeatureLayer identityFeatureLayer = identity_fc.TargetFeatureLayer();
                    // 获取原始图层和标识图层的要素类
                    FeatureClass originFeatureClass = origin_fc.TargetFeatureClass();
                    FeatureClass identityFeatureClass = identity_fc.TargetFeatureClass();
                    // 确保两个图层使用相同的空间参考
                    if (!originFeatureLayer.GetSpatialReference().Equals(identityFeatureLayer.GetSpatialReference()))
                    {
                        MessageBox.Show("目标图层和源图层的空间参考不同！");
                        return;
                    }

                    // 获取目标图层和源图层的要素游标
                    using (RowCursor originCursor = originFeatureClass.Search())
                    {
                        // 遍历源图层的要素
                        while (originCursor.MoveNext())
                        {
                            using (Feature originFeature = (Feature)originCursor.Current)
                            {
                                double maxOverlapArea = 0;
                                // 初始化要标识的字段值
                                object fieldValue= new object();

                                Feature identityFeatureWithMaxOverlap = null;

                                // 获取源要素的几何
                                ArcGIS.Core.Geometry.Geometry originGeometry = originFeature.GetShape();

                                // 创建空间查询过滤器，以获取与源要素有重叠的目标要素
                                SpatialQueryFilter spatialFilter = new SpatialQueryFilter
                                {
                                    FilterGeometry = originGeometry,
                                    SpatialRelationship = SpatialRelationship.Intersects
                                };

                                // 在目标图层中查询与源要素重叠的要素
                                using (RowCursor identityCursor = identityFeatureClass.Search(spatialFilter))
                                {
                                    while (identityCursor.MoveNext())
                                    {
                                        using (Feature identityFeature = (Feature)identityCursor.Current)
                                        {
                                            // 获取目标要素的几何
                                            ArcGIS.Core.Geometry.Geometry identityGeometry = identityFeature.GetShape();

                                            // 计算源要素与目标要素的重叠面积
                                            ArcGIS.Core.Geometry.Geometry intersection = GeometryEngine.Instance.Intersection(originGeometry, identityGeometry);
                                            double overlapArea = (intersection as ArcGIS.Core.Geometry.Polygon).Area;
                                            double originArea = (originGeometry as ArcGIS.Core.Geometry.Polygon).Area;

                                            // 如果重叠面积大于当前最大重叠面积，则更新最大重叠面积和目标要素
                                            if (overlapArea > maxOverlapArea && overlapArea/ originArea > prop)
                                            {
                                                maxOverlapArea = overlapArea;
                                                // 重叠Feature
                                                identityFeatureWithMaxOverlap = identityFeature;
                                                // 字段值
                                                fieldValue = identityFeature[identity_field];
                                            }
                                        }
                                    }
                                }

                                // 如果找到与源要素有最大重叠的目标要素，则将其属性复制到源要素
                                if (identityFeatureWithMaxOverlap != null)
                                {
                                    // 复制属性
                                    object value = fieldValue;
                                    originFeature[origin_field] = value;

                                    // 更新源图层中的源要素
                                    originFeature.Store();
                                }
                                else    // 不符合要求的情况下
                                {
                                    // 清空值
                                    originFeature[origin_field] = null;
                                }
                            }
                        }
                    }

                });
                pw.AddProcessMessage(80, time_base, "工具运行完成！！！", Brushes.Blue);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                return;
            }
        }

        private void combox_origin_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_origin_fc.Text,combox_origin_field);
        }

        private void combox_identity_field_DropDown(object sender, EventArgs e)
        {
            UITool.AddTextFieldsToCombox(combox_identity_fc.Text, combox_identity_field);
        }
    }
}
