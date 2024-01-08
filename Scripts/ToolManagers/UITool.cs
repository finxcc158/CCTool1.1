using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing.Controls;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.ToolManagers;
using Microsoft.Win32;
using NPOI.OpenXmlFormats.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CCTool.Scripts.Manager
{
    public class UITool
    {
        // 将当前地图的所有要素图层加入到Combox中
        public static void AddFeatureLayersToCombox(ComboBox comboBox)
        {
            try
            {
                // 清空combox_field
                comboBox.Items.Clear();
                // 获取当前地图
                Map map = MapView.Active.Map;
                // 获取所有要素图层
                List<string> featureLayers = map.AllFeatureLayers();
                foreach (string featureLayer in featureLayers)
                {
                    comboBox.Items.Add(featureLayer);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message + ee.StackTrace);
                throw;
            }
            
        }

        // 将当前地图的所有独立表加入到Combox中
        public static void AddAllStandeTableToCombox(ComboBox comboBox)
        {
            // 清空combox_field
            comboBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> standaloneTables = map.AllStandaloneTables();
            foreach (var standaloneTable in standaloneTables)
            {
                // combox框中添加所有独立表
                comboBox.Items.Add(standaloneTable);
            }
        }

        // 将当前地图的所有要素图层和独立表加入到Combox中
        public static void AddFeatureLayerAndTableToCombox(ComboBox comboBox)
        {
            // 添加所有要素图层
            AddFeatureLayersToCombox(comboBox);
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> standaloneTables = map.AllStandaloneTables();
            foreach (var standaloneTable in standaloneTables)
            {
                // combox框中添加所有独立表
                comboBox.Items.Add(standaloneTable);
            }
        }

        // 将所有布局加入到ListBox中【带复选框】
        public static async void AddLayoutsToListbox(ListBox listBox)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取LayoutProjectItem
            var layoutProjectItem = Project.Current.GetItems<LayoutProjectItem>();

            foreach (LayoutProjectItem layoutItem in layoutProjectItem)
            {
                Layout layout = await QueuedTask.Run(() =>
                {
                    // 获取Layout
                    return layoutItem.GetLayout();
                });

                // 添加CheckBox
                CheckBox cb = new()
                {
                    Content = layout.Name,
                    IsChecked = false
                };
                listBox.Items.Add(cb);
            }
        }

        // 获取ListBox中所有选中的CheckBox【string格式】
        public static List<string> GetStringFromListbox(ListBox listBox)
        {
            // 获取要素
            var items = listBox.Items;
            // 获取所有选中的要素名
            List<string> listName = new List<string>();
            foreach (CheckBox item in items)
            {
                if (item.IsChecked == true)
                {
                    listName.Add(item.Content.ToString());
                }
            }
            // 返回
            return listName;
        }

        // 将所有图层加入到ListBox中【带复选框】
        public static void AddFeatureLayersToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            List<string> featureLayers = map.AllFeatureLayers();

            foreach (var featureLayer in featureLayers)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(featureLayer))
                    {
                        CheckBox cb = new()
                        {
                            Content = featureLayer,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = featureLayer,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将所有独立表加入到ListBox中【带复选框】
        public static void AddStandaloneTablesToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有独立表
            List<string> standaloneTables = map.AllStandaloneTables();

            foreach (var standaloneTable in standaloneTables)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(standaloneTable))
                    {
                        CheckBox cb = new()
                        {
                            Content = standaloneTable,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = standaloneTable,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将所有图层和独立表加入到ListBox中【带复选框】
        public static void AddFeatureLayersAndTablesToListbox(ListBox listBox, List<string> excludeItems = null)
        {
            // 清空listBox
            listBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层和独立表
            List<string> featureLayers = map.AllFeatureLayers();
            List<string> standaloneTables = map.AllStandaloneTables();

            // 加入图层
            foreach (var featureLayer in featureLayers)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(featureLayer))
                    {
                        CheckBox cb = new()
                        {
                            Content = featureLayer,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = featureLayer,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }


            // 加入独立表
            foreach (var standaloneTable in standaloneTables)
            {
                if (excludeItems is not null)           // 如果有排除图层
                {
                    if (!excludeItems.Contains(standaloneTable))
                    {
                        CheckBox cb = new()
                        {
                            Content = standaloneTable,
                            IsChecked = false
                        };
                        listBox.Items.Add(cb);
                    }
                }
                else              // 如果默认没有排除图层
                {
                    CheckBox cb = new()
                    {
                        Content = standaloneTable,
                        IsChecked = false
                    };
                    listBox.Items.Add(cb);
                }
            }
        }

        // 全选ListBox部件
        public static void SelectAllItems(ListBox listBox)
        {
            if (listBox.Items.Count == 0)
            {
                MessageBox.Show("列表内没有要素！");
                return;
            }

            var list_field = listBox.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = true;
            }
        }

        // 取消全选ListBox部件
        public static void UnSelectAllItems(ListBox listBox)
        {
            if (listBox.Items.Count == 0)
            {
                MessageBox.Show("列表内没有要素！");
                return;
            }

            var list_field = listBox.Items;
            foreach (CheckBox item in list_field)
            {
                item.IsChecked = false;
            }
        }

        // 将图层字段加入到Combox列表中
        public static async void AddFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName);
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【TEXT】加入到Combox列表中
        public static async void AddTextFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }

            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                List<Field> fields = GisTool.GetFieldsFromTarget(LayerName, "text");
                if (fields is null)
                {
                    return;
                }
                foreach (Field field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddFloatFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "float");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Float】加入到Combox列表中
        public static async void AddAllFloatFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "float_all");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将图层字段【Int】加入到Combox列表中
        public static async void AddIntFieldsToCombox(string LayerName, ComboBox comboBox)
        {
            // 判断是否选择了图层
            if (LayerName == "")
            {
                MessageBox.Show("请选择一个要素图层或表");
                return;
            }
            // 清空combox_field
            comboBox.Items.Clear();

            await QueuedTask.Run(() =>
            {
                // 获取所选图层的所有字段
                var fields = GisTool.GetFieldsFromTarget(LayerName, "int");
                if (fields is null)
                {
                    return;
                }
                foreach (var field in fields)
                {
                    // 在UI线程上执行添加item的操作
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 将所有字段名添加到combox_field中
                        comboBox.Items.Add(field.Name);
                    });
                }
            });
        }

        // 将当前地图的图层组或单个图层加入到Combox中
        public static void AddLayerGroupToCombox(ComboBox comboBox)
        {
            // 清空combox_field
            comboBox.Items.Clear();
            // 获取当前地图
            Map map = MapView.Active.Map;
            // 获取所有要素图层
            var lys = map.GetLayersAsFlattenedList().OfType<Layer>().ToList();
            foreach (var ly in lys)
            {
                if (ly.Parent == map)
                {
                    // 向combox框中添加图层
                    if (ly is GroupLayer)
                    {
                        comboBox.Items.Add(@"【组】" + ly.Name);
                    }
                    else
                    {
                        comboBox.Items.Add(ly.Name);
                    }
                }
            }
        }

        // 将布局视图加入到Combox中
        public static void AddAllLayoutToCombox(ComboBox comboBox)
        {
            // 清空combox_field
            comboBox.Items.Clear();
            // 获取当前工程中的布局
            var layouts = Project.Current.GetItems<LayoutProjectItem>().ToList();
            foreach (var layout in layouts)
            {
                // combox框中添加所有要素图层
                comboBox.Items.Add(layout.Name);
            }
        }

        // 将ListBox中选中的要素加入到List中
        public static List<string> GetCheckboxStringFromListBox(ListBox listBox)
        {      
            List<string> result = new List<string>();
            foreach (CheckBox checkbox in listBox.Items)
            {
                if (checkbox.IsChecked == true)
                {
                    result.Add(checkbox.Content.ToString());
                }
            }
            return result;
        }

        // 将要素的字段加入到ListBox中
        public static async void AddFieldsToListBox(ListBox listBox, string fcPath)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath);
            });

            foreach (Field field in fields)
            {
                if (field.FieldType == FieldType.OID || field.FieldType == FieldType.Geometry || !field.IsEditable)
                {
                    continue;
                }
                else
                {
                    // 将filed做成checkbox放入列表中
                    CheckBox cb = new CheckBox();
                    cb.Content = field.Name;
                    cb.IsChecked = true;
                    listBox.Items.Add(cb);
                }
            }
        }

        // 将要素的字段加入到ListBox中【文本型】
        public static async void AddTextFieldsToListBox(ListBox listBox, string fcPath)
        {
            // 清除listbox
            listBox.Items.Clear();

            // 获取所有非Geo字段
            var fields = await QueuedTask.Run(() =>
            {
                return GisTool.GetFieldsFromTarget(fcPath);
            });

            foreach (Field field in fields)
            {
                if (field.FieldType == FieldType.String)
                {
                    // 将filed做成checkbox放入列表中
                    CheckBox cb = new CheckBox();
                    cb.Content = field.Name;
                    cb.IsChecked = true;
                    listBox.Items.Add(cb);
                }
            }
        }

        // 打开Excel文件
        public static string OpenDialogExcel()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "Excel文件|*.xlsx",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开MDB文件
        public static string OpenDialogMDB()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "MDB文件|*.mdb",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开图片文件
        public static string OpenDialogPicture()
        {
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Title = "选择一个图片",        //打开对话框标题
                Multiselect = false,             //是否可以多选
                Filter = "图片文件|*.jpg;*.png",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 打开文件夹
        public static string OpenDialogFolder()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Folders,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开FeatureClass文件
        public static string OpenDialogFeatureClass()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.FeatureClasses_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开GDB数据库文件
        public static string OpenDialogGDB()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Geodatabases,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 打开Table文件
        public static string OpenTable()
        {
            OpenItemDialog openDlg = new OpenItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                MultiSelect = false,             //是否可以多选
                Filter = ItemFilters.Tables_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.Items.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.Items.First();
            // 返回路径
            return item.Path;
        }

        // 保存Excel文件
        public static string SaveDialogExcel()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Filter = "Excel文件|*.xlsx",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存图片文件
        public static string SaveDialogPicture()
        {
            SaveFileDialog openDlg = new SaveFileDialog()
            {
                Title = "选择一个图片",        //打开对话框标题
                Filter = "Excel文件|*.jpg;*.png",       //类型筛选
            };
            // 打开对话框
            bool? ok = openDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue || openDlg.FileNames.Count() == 0)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = openDlg.FileNames.First();
            // 返回路径
            return item;
        }

        // 保存FeatureClass文件
        public static string SaveDialogFeatureClass()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Filter = ItemFilters.FeatureClasses_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item;
        }

        // 保存GDB文件
        public static string SaveDialogGDB()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Filter = ItemFilters.Geodatabases,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item+".gdb";
        }

        // 保存独立表文件
        public static string SaveDialogTable()
        {
            SaveItemDialog saveDlg = new SaveItemDialog()
            {
                Title = "选择一个要素",        //打开对话框标题
                Filter = ItemFilters.Tables_All,       //类型筛选
            };
            // 打开对话框
            bool? ok = saveDlg.ShowDialog();
            // 如果没有选择内容，则返回
            if (!ok.HasValue)
                return null;
            // 如果有选择内容，返回选择的内容
            var item = saveDlg.FilePath;
            // 返回路径
            return item;
        }

        // 打开自定义的进度框
        public static ProcessWindow OpenProcessWindow(ProcessWindow processWindow, string tool_name)
        {
            processWindow = new ProcessWindow();
            processWindow.Owner = FrameworkApplication.Current.MainWindow;
            processWindow.Closed += (o, e) => { processWindow = null; };
            processWindow.Title = tool_name;
            processWindow.Show();
            return processWindow;
        }
    }
}
