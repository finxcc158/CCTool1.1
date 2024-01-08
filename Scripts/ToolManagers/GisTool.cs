using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.ToolManagers
{
    public class GisTool
    {
        // 在表中插入行并赋值
        public static string UpdataRowToTable(string in_table, string insert_value)
        {
            // 获取字段和值的列表
            List<string> keyAndValues = insert_value.Split(";").ToList();
            // 获取Table
            using Table sta_table = in_table.TargetTable();
            // 判断是不是已有该字段
            bool isHaveRow = false;
            string firstKey = keyAndValues[0].Split(",")[0];
            string firstValue = keyAndValues[0].Split(",")[1];
            using (RowCursor rowCursor = sta_table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {

                        // 获取value
                        var va = row[firstKey];
                        if (va is not null)
                        {
                            if (va.ToString() == firstValue)
                            {
                                isHaveRow = true;
                            }
                        }
                    }
                }
            }
            // 获取表定义
            TableDefinition tableDefinition = sta_table.GetDefinition();

            // 如果有符合该行的字段，则更新字段值
            if (isHaveRow)
            {
                using (RowCursor rowCursor2 = sta_table.Search())
                {
                    while (rowCursor2.MoveNext())
                    {
                        using (Row row2 = rowCursor2.Current)
                        {
                            // 获取value
                            var va = row2[firstKey];
                            // 如果符合
                            if (va.ToString() == firstValue)
                            {
                                // 写入字段值
                                foreach (var keyAndValue in keyAndValues)
                                {
                                    string key = keyAndValue.Split(",")[0];
                                    string value = keyAndValue.Split(",")[1];
                                    row2[key] = value;
                                }
                            }
                            row2.Store();
                        }
                    }
                }
            }
            // 如果没有符合该行的字段，则插入并更新
            else
            {
                EditOperation editOperation = new EditOperation();  // 创建编辑操作对象
                // 创建RowBuffer
                using RowBuffer rowBuffer = sta_table.CreateRowBuffer();
                // 写入字段值
                foreach (var keyAndValue in keyAndValues)
                {
                    string key = keyAndValue.Split(",")[0];
                    string value = keyAndValue.Split(",")[1];
                    rowBuffer[key] = value;
                }
                // 在表中创建新行
                using Row row = sta_table.CreateRow(rowBuffer);
                editOperation.Execute();
            }
            // 返回值
            return in_table;
        }

        // 更新字段值（增加值）
        public static string IncreRowValueToTable(string in_table, string insert_value)
        {
            // 获取字段和值的列表
            List<string> keyAndValues = insert_value.Split(";").ToList();
            // 获取Table
            using Table sta_table = in_table.TargetTable();
            // 判断是不是已有该字段
            bool isHaveRow = false;
            string firstKey = keyAndValues[0].Split(",")[0];
            string firstValue = keyAndValues[0].Split(",")[1];
            using (RowCursor rowCursor = sta_table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {

                        // 获取value
                        var va = row[firstKey];
                        if (va is not null)
                        {
                            if (va.ToString() == firstValue)
                            {
                                isHaveRow = true;
                            }
                        }
                    }
                }
            }
            // 获取表定义
            TableDefinition tableDefinition = sta_table.GetDefinition();

            // 如果有符合该行的字段，则更新字段值
            if (isHaveRow)
            {
                using (RowCursor rowCursor2 = sta_table.Search(null, false))
                {
                    while (rowCursor2.MoveNext())
                    {
                        using (Row row2 = rowCursor2.Current)
                        {
                            // 获取value
                            var va = row2[firstKey];
                            // 如果符合
                            if (va.ToString() == firstValue)
                            {
                                // 写入字段值
                                foreach (var keyAndValue in keyAndValues)
                                {
                                    string key = keyAndValue.Split(",")[0];
                                    string value = keyAndValue.Split(",")[1];
                                    if (key != firstKey)
                                    {
                                        double double_value = double.Parse(row2[key].ToString());
                                        row2[key] = double_value + double.Parse(value);
                                    }
                                }
                            }
                            row2.Store();
                        }
                    }
                }
            }
            // 如果没有符合该行的字段，则插入并更新
            else
            {
                EditOperation editOperation = new EditOperation();  // 创建编辑操作对象
                // 创建RowBuffer
                using RowBuffer rowBuffer = sta_table.CreateRowBuffer();
                // 写入字段值
                foreach (var keyAndValue in keyAndValues)
                {
                    string key = keyAndValue.Split(",")[0];
                    string value = keyAndValue.Split(",")[1];
                    rowBuffer[key] = value;
                }
                // 在表中创建新行
                using Row row = sta_table.CreateRow(rowBuffer);
                editOperation.Execute();
            }
            // 返回值
            return in_table;
        }

        // 清除所选字段值中的空值
        public static string ClearStringNull(string initlayer, List<string> fieldNames)
        {
            foreach (string fieldName in fieldNames)
            {
                Arcpy.CalculateField(initlayer, fieldName, $"ss(!{fieldName}!)", "def ss(a):\r\n    if a is None:\r\n        return \"\"\r\n    else:\r\n        return a");
            }
            return initlayer;
        }

        // 拓扑检查
        public static string TopologyCheck(string in_data_path, List<string> rules, string outGDBPath)
        {
            string in_data = in_data_path.Replace(@"/", @"\");  // 兼容两种符号
            string db_name = "Top2Check";    // 要素数据集名
            string fc_name = "top_fc";        // 要素名
            string top_name = "Topology";       // TOP名
            string db_path = outGDBPath + "\\" + db_name;    // 要素数据集路径
            string fc_top_path = db_path + "\\" + fc_name;         // 要素路径
            string top_path = db_path + "\\" + top_name;         // TOP路径

            string in_gdb = in_data[..(in_data.IndexOf(@".gdb") + 4)];
            string in_fc = in_data[(in_data.LastIndexOf(@"\") + 1)..];

            // 打开GDB数据库
            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(in_gdb)));
            // 获取要素类
            FeatureClassDefinition featureClasse = gdb.GetDefinition<FeatureClassDefinition>(in_fc);
            //获取图层的坐标系
            var sr = featureClasse.GetSpatialReference();
            //在数据库中创建要素数据集
            Arcpy.CreateFeatureDataset(outGDBPath, db_name, sr);
            // 将所选要素复制到创建的要素数据集中
            Arcpy.CopyFeatures(in_data, fc_top_path);
            // 新建拓扑
            Arcpy.CreateTopology(db_path, top_name);
            // 向拓扑中添加要素
            Arcpy.AddFeatureClassToTopology(top_path, fc_top_path);
            // 添加拓扑规则
            foreach (var rule in rules)
            {
                Arcpy.AddRuleToTopology(top_path, rule, fc_top_path);
            }
            // 验证拓扑
            Arcpy.ValidateTopology(top_path);
            // 输出TOP错误
            Arcpy.ExportTopologyErrors(top_path, outGDBPath, "TopErr");
            // 删除中间数据
            Arcpy.Delect(db_path);
            // 返回值
            return outGDBPath;
        }

        // 从路径或图层中获取字段Field列表【字段类型：all=>全部字段，text=>可写的字符串字段，float=>浮点型，int=>整型, notEdit=>不可编辑的】
        public static List<Field> GetFieldsFromTarget(string layerName, string field_type = "all")
        {
            List<Field> fields_ori = new List<Field>();
            List<Field> editFields = new List<Field>();
            List<Field> notEditFields = new List<Field>();
            // 获取Table
            Table table = layerName.TargetTable();

            // 获取所有字段
            fields_ori = table.GetDefinition().GetFields().ToList();
            // 移除不可编辑的字段
            foreach (var field in fields_ori)
            {
                if (field.IsEditable)
                {
                    editFields.Add(field);
                }
                else
                {
                    notEditFields.Add(field);
                }
            }

            // 输出字段类型
            if (field_type == "allof")  // 真的全部
            {
                return fields_ori;
            }
            else if (field_type == "all")  //可编辑的全部
            {
                return editFields;
            }
            else if (field_type == "text")
            {
                // 获取可编辑的字符串字段
                List<Field> text_fields = new List<Field>();
                foreach (Field field in editFields)
                {
                    if (field.FieldType == FieldType.String)
                    {
                        text_fields.Add(field);
                    }
                }
                return text_fields;
            }
            else if (field_type == "float")
            {
                // 获取可编辑的数字型字段
                List<Field> float_fields = new List<Field>();
                foreach (Field field in editFields)
                {
                    if (field.FieldType == FieldType.Single || field.FieldType == FieldType.Double)
                    {
                        float_fields.Add(field);
                    }
                }
                return float_fields;
            }

            else if (field_type == "float_all")
            {
                // 获取可编辑的数字型字段
                List<Field> float_fields = new List<Field>();
                foreach (Field field in fields_ori)
                {
                    if (field.FieldType == FieldType.Single || field.FieldType == FieldType.Double)
                    {
                        float_fields.Add(field);
                    }
                }
                return float_fields;
            }

            else if (field_type == "int")
            {
                // 获取可编辑的数字型字段
                List<Field> int_fields = new List<Field>();
                foreach (Field field in editFields)
                {
                    if (field.FieldType == FieldType.SmallInteger || field.FieldType == FieldType.Integer)
                    {
                        int_fields.Add(field);
                    }
                }
                return int_fields;
            }
            else if (field_type == "notEdit")
            {
                return notEditFields;
            }
            else
            {
                return null;
            }
        }

        // 从路径或图层中按字段名获取字段Field
        public static Field GetFieldFromString(string layerName, string fieldName)
        {
            Field field = null;
            // 获取Table
            Table table = layerName.TargetTable();

            // 获取所有字段
            List<Field> fields_ori = table.GetDefinition().GetFields().ToList();
            // 移除不可编辑的字段
            foreach (Field fd in fields_ori)
            {
                if (fd.Name == fieldName)
                {
                    field = fd;
                }
            }
            // 返回字段
            return field;
        }

        // 从路径或图层中获取字段Field的名称列表【字段类型：all=>全部字段，text=>可写的字符串字段，float=>浮点型，int=>整型】
        public static List<string> GetFieldsNameFromTarget(string layerName, string field_type = "all")
        {
            List<Field> fields = GetFieldsFromTarget(layerName, field_type);
            List<string> names = new List<string>();
            foreach (var field in fields)
            {
                names.Add(field.Name);
            }
            return names;

        }

        // 从路径或图层中获取对象ID字段
        public static string GetIDFieldNameFromTarget(string layerName)
        {
            //  获取不可编辑的字段
            List<Field> fields = GetFieldsFromTarget(layerName, "notEdit");
            string IDField = "";
            foreach (var field in fields)
            {
                if (field.FieldType == FieldType.OID)
                {
                    IDField = field.Name;
                }
            }
            return IDField;
        }

        // 清除字符串字段值中的空格
        public static string ClearTextSpace(string input_table, string field_name)
        {
            string exp = $"!{field_name}!.replace(' ','')";
            Arcpy.CalculateField(input_table, field_name, exp);
            return input_table;
        }

        // 清除字符串字段值中的空值，字符串转为""
        public static string ClearTextNull(string input_table, string field_name)
        {
            string block = "def ss(a):\r\n    if a is None:\r\n        return \"\"\r\n    else:\r\n        return a";
            Arcpy.CalculateField(input_table, field_name, $"ss(!{field_name}!)", block);
            return input_table;
        }

        // 清除数字值字段值中的空值，数字型转为0
        public static string ClearMathNull(string input_table, string field_name)
        {
            string block = "def ss(a):\r\n    if a is None:\r\n        return 0\r\n    else:\r\n        return a";
            Arcpy.CalculateField(input_table, field_name, $"ss(!{field_name}!)", block);
            return input_table;
        }

        // 将数字0转换为空值
        public static string Zero2Null(string input_table, string field_name)
        {
            string block = "def ss(a):\r\n    if a==0:\r\n        return None\r\n    else:\r\n        return a";
            Arcpy.CalculateField(input_table, field_name, $"ss(!{field_name}!)", block);
            return input_table;
        }

        // 属性映射
        public static string AttributeMapper(string in_data, string in_field, string map_field, string map_tabel, bool reverse = false)
        {
            // 获取连接表的2个字段名
            string exl_field01;
            string exl_field02;
            if (reverse)
            {
                exl_field01 = OfficeTool.GetCellFromExcel(map_tabel, 0, 1);
                exl_field02 = OfficeTool.GetCellFromExcel(map_tabel, 0, 0);
            }
            else
            {
                exl_field01 = OfficeTool.GetCellFromExcel(map_tabel, 0, 0);
                exl_field02 = OfficeTool.GetCellFromExcel(map_tabel, 0, 1);
            }

            List<string> fields = new List<string>() { exl_field02 };
            // 连接字段
            Arcpy.JoinField(in_data, in_field, map_tabel, exl_field01, fields);
            // 计算字段
            Arcpy.CalculateField(in_data, map_field, "!" + exl_field02 + "!");
            // 删除多余字段
            Arcpy.DeleteField(in_data, fields);

            return in_data;
        }

        // 用地用海编码和名称互转
        public static string YDYHChange(string fcPath, string fieldBefore, string fieldAfter, string model = "代码转名称")
        {
            // 复制表格
            string def_path = Project.Current.HomeFolderPath;
            string excel_map = def_path + @"\用地用海_DM_to_MC.xlsx";
            BaseTool.CopyResourceFile(@"CCTool.Data.Excel.用地用海_DM_to_MC.xlsx", excel_map);

            if (model == "代码转名称")
            {
                // 属性映射
                AttributeMapper(fcPath, fieldBefore, fieldAfter, excel_map + @"\sheet1$");
            }
            else if (model == "名称转代码")
            {
                // 属性映射
                AttributeMapper(fcPath, fieldBefore, fieldAfter, excel_map + @"\sheet1$", true);
            }
            // 删除中间数据
            File.Delete(excel_map);
            // 返回值
            return fcPath;
        }

        // 生成用地用海三级用地编码和名称
        public static string CreateYDYHBM(string fcPath, string bmField, int model = 3, bool isCreateMC = false)
        {
            // 根据需求生成三级用地编码和名称
            if (model >= 1)
            {
                Arcpy.AddField(fcPath, "BM_1", "TEXT");     // 添加大类编码
                Arcpy.CalculateField(fcPath, "BM_1", $"!{bmField}![:2]");   // 计算大类编码
                if (isCreateMC)
                {
                    Arcpy.AddField(fcPath, "MC_1", "TEXT");     // 添加大类名称
                    YDYHChange(fcPath, "BM_1", "MC_1");    // 编码转名称
                }
            }
            if (model >= 2)
            {
                Arcpy.AddField(fcPath, "BM_2", "TEXT");     // 添加中类编码
                Arcpy.CalculateField(fcPath, "BM_2", $"!{bmField}![:4]");   // 计算中类编码
                if (isCreateMC)
                {
                    Arcpy.AddField(fcPath, "MC_2", "TEXT");     // 添加中类名称
                    YDYHChange(fcPath, "BM_2", "MC_2");    // 编码转名称
                }
            }
            if (model >= 3)
            {
                Arcpy.AddField(fcPath, "BM_3", "TEXT");     // 添加小类编码
                Arcpy.CalculateField(fcPath, "BM_3", $"!{bmField}![:6]");   // 计算小类编码
                if (isCreateMC)
                {
                    Arcpy.AddField(fcPath, "MC_3", "TEXT");     // 添加小类名称
                    YDYHChange(fcPath, "BM_3", "MC_3");    // 编码转名称
                }
            }
            return fcPath;
        }

        // 从路径或图层中获取Dictionary
        public static Dictionary<string, string> GetDictFromPath(string inputData, string in_field_01, string in_field_02)
        {
            Dictionary<string, string> dict = new();
            // 获取Table
            Table table = inputData.TargetTable();
            // 逐行游标
            using (RowCursor rowCursor = table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        // 获取value
                        var key = row[in_field_01];
                        if (key is not null)
                        {
                            var value = row[in_field_02].ToString();
                            // 如果没有重复key值，则纳入dict
                            if (!dict.Keys.Contains(key.ToString()))
                            {
                                dict.Add(key.ToString(), value);
                            }
                        }
                    }
                }
            }
            return dict;
        }

        // 从路径或图层中获取list
        public static List<string> GetListFromPath(string inputData, string inputField)
        {
            List<string> list = new List<string>();
            // 获取Table
            Table table = inputData.TargetTable();
            using (RowCursor rowCursor = table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    // 获取value
                    var va = row[inputField];
                    if (va is not null)
                    {
                        // 如果是新值，就加入到list中
                        if (!list.Contains(va.ToString()))
                        {
                            list.Add(va.ToString());
                        }
                    }
                }
            }
            return list;
        }

        // 从路径或图层中获取value
        public static string GetCellFromPath(string inputData, string inputField, string sql)
        {
            string value = "";

            // 获取Table
            Table table = inputData.TargetTable();
            // 设定筛选语句
            var queryFilter = new QueryFilter();
            queryFilter.WhereClause = sql;
            // 逐行搜索
            using RowCursor rowCursor = table.Search(queryFilter, false);
            while (rowCursor.MoveNext())
            {
                using Row row = rowCursor.Current;
                // 获取value
                var va = row[inputField];
                if (va is not null)
                {
                    value = va.ToString();
                }
            }
            return value;
        }

        // 判断要素数据集是否存在
        public static bool IsHaveDataset(string gdb_path, string dt_name)
        {
            // 打开地理数据库
            using var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素数据集的名称
            var datasets = geodatabase.GetDefinitions<FeatureDatasetDefinition>();
            // 检查要素数据集是否存在
            var exists = datasets.Any(datasetName => datasetName.GetName().Equals(dt_name));
            return exists;
        }

        // 判断要素类是否存在
        public static bool IsHaveFeaturClass(string gdb_path, string fc_name)
        {
            // 打开地理数据库
            using var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素数据集的名称
            var fcs = geodatabase.GetDefinitions<FeatureClassDefinition>();
            // 检查要素数据集是否存在
            var exists = fcs.Any(datasetName => datasetName.GetName().Equals(fc_name));
            return exists;
        }

        // 判断独立表是否存在
        public static bool IsHaveStandaloneTable(string gdb_path, string tb_name)
        {
            // 打开地理数据库
            using var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdb_path)));
            // 获取所有要素数据集的名称
            var tables = geodatabase.GetDefinitions<TableDefinition>();
            // 检查要素数据集是否存在
            var exists = tables.Any(datasetName => datasetName.GetName().Equals(tb_name));
            return exists;
        }

        // 汇总统计加强版
        public static void MultiStatistics(string in_table, string out_table, string statistics_fields, List<string> case_fields, string total_field = "合计", int unit = 0, bool is_output = false)
        {
            List<string> list_table = new List<string>();
            for (int i = 0; i < case_fields.Count; i++)
            {
                Arcpy.Statistics(in_table, out_table + i.ToString(), statistics_fields, case_fields[i]);    // 调用GP工具【汇总】
                Arcpy.AlterField(out_table + i.ToString(), case_fields[i], @"分组", @"分组");  // 调用GP工具【更改字段】
                list_table.Add(out_table + i.ToString());
            }
            Arcpy.Statistics(in_table, out_table + "_total", statistics_fields, "");    // // 调用GP工具【汇总】
            Arcpy.AddField(out_table + "_total", @"分组", "TEXT");    // 调用GP工具【更改字段】
            Arcpy.CalculateField(out_table + "_total", @"分组", "\"" + total_field + "\"");    // 调用GP工具【计算字段】
            list_table.Add(out_table + "_total");     // 加入列表
            // 合并汇总表
            Arcpy.Merge(list_table, out_table, is_output);       // 调用GP工具【合并】
                                                                 // 单位转换
            if (unit > 0)
            {
                string fd = "SUM_" + statistics_fields.Replace(" SUM", "");
                ChangeUnit(out_table, fd, unit);        // 单位转换
            }
            // 删除中间要素
            for (int i = 0; i < case_fields.Count; i++)
            {
                Arcpy.Delect(out_table + i.ToString());
            }
            Arcpy.Delect(out_table + "_total");
        }

        // 表中的值从平方米改为公顷、平方公里或亩
        public static void ChangeUnit(string in_data, string field, int unit = 1)
        {
            // 选择修改的单位
            double cg = 10000;          // 公顷
            if (unit == 2)
            {
                cg = 1000000;        // 平方公里
            }
            else if (unit == 3)
            {
                cg = 666.66667;       // 亩
            }
            else if (unit == 4)
            {
                cg = 1;       // 平方米
            }
            // 单位换算
            Arcpy.CalculateField(in_data, field, "!" + field + "!/" + cg.ToString());
        }

        // 裁剪平差计算
        public static string Adjustment(string yd, string area, string clipfc_sort, string area_type = "投影", string unit = "平方米", int digit = 2, string areaField="")
        {
            string def_gdb = Project.Current.DefaultGeodatabasePath;
            string area_line = def_gdb + @"\area_line";
            string clipfc = def_gdb + @"\clipfc";
            string clipfc_sta = def_gdb + @"\clipfc_sta";
            string clipfc_updata = def_gdb + @"\clipfc_updata";

            // 单位系数设置
            double unit_xs = 0;
            if (unit == "平方米") { unit_xs = 1; }
            else if (unit == "公顷") { unit_xs = 10000; }
            else if (unit == "平方公里") { unit_xs = 1000000; }
            else if (unit == "亩") { unit_xs = 666.66667; }

            // 计算图斑的投影面积和图斑面积
            Arcpy.Clip(yd, area, clipfc);

            if (areaField!="")
            {
                area_type = "图斑";

                Arcpy.AddField(clipfc, area_type, "DOUBLE");
                Arcpy.AddField(area, area_type, "DOUBLE");

                Arcpy.CalculateField(clipfc, area_type, $"!{areaField}!");
                Arcpy.Statistics(clipfc, clipfc_sta, area_type, "");          // 汇总
                // 计算范围的投影面积和图斑面积
                Arcpy.CalculateField(area, area_type, $"round(!shape.geodesicarea!/{unit_xs},{digit})");

            }
            else
            {
                Arcpy.AddField(clipfc, area_type, "DOUBLE");
                Arcpy.AddField(area, area_type, "DOUBLE");

                if (area_type == "投影")
                {
                    Arcpy.CalculateField(clipfc, area_type, $"round(!shape_area!/{unit_xs},{digit})");
                    Arcpy.Statistics(clipfc, clipfc_sta, area_type, "");          // 汇总
                    // 计算范围的投影面积和图斑面积
                    Arcpy.CalculateField(area, area_type, $"round(!shape_area!/{unit_xs},{digit})");
                }
                else if (area_type == "图斑")
                {
                    Arcpy.CalculateField(clipfc, area_type, $"round(!shape.geodesicarea!/{unit_xs},{digit})");
                    Arcpy.Statistics(clipfc, clipfc_sta, area_type, "");          // 汇总
                    // 计算范围的投影面积和图斑面积
                    Arcpy.CalculateField(area, area_type, $"round(!shape.geodesicarea!/{unit_xs},{digit})");
                }
            }

            // 获取投影面积，图斑面积
            double mj_fc = double.Parse(GisTool.GetCellFromPath(clipfc_sta, $"SUM_{area_type}", ""));
            double mj_area = double.Parse(GisTool.GetCellFromPath(area, area_type, ""));

            // 面积差值
            double dif_mj = Math.Round(Math.Round(mj_area, digit) - Math.Round(mj_fc, digit), digit);

            // 空间连接，找出变化图斑（即需要平差的图斑）
            Arcpy.FeatureToLine(area, area_line);
            Arcpy.SpatialJoin(clipfc, area_line, clipfc_updata);
            Arcpy.AddField(clipfc_updata, "平差", "TEXT");
            Arcpy.CalculateField(clipfc_updata, "平差", "''");
            // 排序
            Arcpy.Sort(clipfc_updata, clipfc_sort, "Shape_Area DESCENDING", "UR");
            double area_total = 0;

            // 获取Table
            using Table table = clipfc_sort.TargetTable();

            // 汇总变化图斑的面积
            using (RowCursor rowCursor = table.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        var va = int.Parse(row["Join_Count"].ToString());
                        if (va == 1)     // 如果是变化图斑
                        {
                            area_total += double.Parse(row[area_type].ToString());
                        }
                    }
                }
            }
            // 第一轮平差
            double area_pc_1 = 0;
            using (RowCursor rowCursor1 = table.Search())
            {
                while (rowCursor1.MoveNext())
                {
                    using (Row row = rowCursor1.Current)
                    {
                        var va = int.Parse(row["Join_Count"].ToString());
                        if (va == 1)
                        {
                            double area_1 = double.Parse(row[area_type].ToString());
                            // 单个图斑需要平差的值
                            double area_pc = Math.Round(area_1 / area_total * dif_mj, digit);
                            area_pc_1 += area_pc;
                            // 面积平差
                            row[area_type] = area_1 + area_pc;
                        }
                        row.Store();
                    }
                }
            }
            // 计算剩余平差面积，进行第二轮平差
            double area_total_next = Math.Round(dif_mj - area_pc_1, digit);
            using (RowCursor rowCursor2 = table.Search())
            {
                while (rowCursor2.MoveNext())
                {
                    using (Row row = rowCursor2.Current)
                    {
                        // 最小平差值
                        double diMin = Math.Round(Math.Pow(0.1, digit), digit);

                        var va = int.Parse(row["Join_Count"].ToString());
                        if (va == 1)
                        {
                            double area_2 = double.Parse(row[area_type].ToString());
                            // 面积平差
                            if (area_total_next > 0)
                            {
                                row[area_type] = area_2 + diMin;
                                area_total_next -= diMin;
                            }
                            else if (area_total_next < 0)
                            {
                                row[area_type] = area_2 - diMin;
                                area_total_next += diMin;
                            }
                            row.Store();
                        }
                    }
                }
            }
            // 删除中间要素
            List<string> all = new List<string>() { "area_line", "clipfc", "clipfc_sta", "clipfc_updata" };
            foreach (var item in all)
            {
                Arcpy.Delect(def_gdb + @"\" + item);
            }
            // 返回值
            return clipfc_sort;
        }

        // 裁剪并计算面积
        public static string AdjustmentNot(string yd, string area, string clipfc_sort, string area_type = "投影", string unit = "平方米", int digit = 2)
        {
            // 单位系数设置
            double unit_xs = 0;
            if (unit == "平方米") { unit_xs = 1; }
            else if (unit == "公顷") { unit_xs = 10000; }
            else if (unit == "平方公里") { unit_xs = 1000000; }
            else if (unit == "亩") { unit_xs = 666.66667; }

            // 计算图斑的投影面积和图斑面积
            Arcpy.Clip(yd, area, clipfc_sort);

            Arcpy.AddField(clipfc_sort, area_type, "DOUBLE");

            if (area_type == "投影")
            {
                Arcpy.CalculateField(clipfc_sort, area_type, $"round(!shape_area!/{unit_xs},{digit})");
            }
            else if (area_type == "图斑")
            {
                Arcpy.CalculateField(clipfc_sort, area_type, $"round(!shape.geodesicarea!/{unit_xs},{digit})");
            }
            // 返回值
            return yd;
        }

        // 获取面空洞【输出模式：空洞 | 外边界】
        public static string GetCave(string in_featureClass, string out_featureClass, string model = "空洞")
        {
            // 获取默认数据库
            var gdb = Project.Current.DefaultGeodatabasePath;
            // 融合要素
            Arcpy.Dissolve(in_featureClass, gdb + @"\dissolve_fc");
            // 面转线
            Arcpy.PolygonToLine(gdb + @"\dissolve_fc", gdb + @"\dissolve_line");
            // 要素转面
            Arcpy.FeatureToPolygon(gdb + @"\dissolve_line", gdb + @"\dissolve_polygon");
            // 再融合，获取边界
            Arcpy.Dissolve(gdb + @"\dissolve_polygon", gdb + @"\dissolve_fin");
            // 擦除，获取空洞
            Arcpy.Erase(gdb + @"\dissolve_fin", gdb + @"\dissolve_fc", gdb + @"\single_fc");
            // 单部件转多部件，输出
            if (model == @"空洞")
            {
                Arcpy.MultipartToSinglepart(gdb + @"\single_fc", out_featureClass);
            }
            else if (model == @"外边界")
            {
                Arcpy.MultipartToSinglepart(gdb + @"\dissolve_fin", out_featureClass);
            }
            // 删除中间要素
            List<string> list_fc = new List<string>() { "dissolve_fc", "dissolve_line", "dissolve_polygon", "dissolve_fin", "single_fc" };
            foreach (var fc in list_fc)
            {
                Arcpy.Delect(gdb + @"\" + fc);
            }
            // 返回值
            return out_featureClass;
        }

        // 要素类消除工具
        public static string FeatureClassEliminate(string in_fc, string out_fc, string sql, string ex_sql = "")
        {
            string layer = "待消除要素";
            string el_layer = layer;
            // 创建要素图层
            Arcpy.MakeFeatureLayer(in_fc, layer, true);
            // 按属性选择图层
            Arcpy.SelectLayerByAttribute(layer, sql);
            // 消除
            Arcpy.Eliminate(el_layer, out_fc, ex_sql);
            // 移除图层
            MapCtlTool.RemoveLayer(layer);
            // 返回值
            return out_fc;
        }


    }
}
