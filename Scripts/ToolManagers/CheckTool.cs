using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.ToolManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Table = ArcGIS.Core.Data.Table;

namespace CCTool.Scripts.Manager
{
    public class CheckTool
    {
        // 检查要素图层的数据类型是否正确
        public static string CheckFeatureClassType(string lyName, string fcType)
        {
            string result = "";

            // FeatureLayer
            FeatureLayer init_featurelayer = lyName.TargetFeatureLayer();

            if (init_featurelayer is not null)
            {
                // 获取要素图层的要素类（FeatureClass）
                FeatureClass featureClass = init_featurelayer.GetFeatureClass();

                // 获取要素类的几何类型
                string featureClassType = featureClass.GetDefinition().GetShapeType().ToString();

                // 判断
                if (featureClassType != fcType)
                {
                    result += $"【{lyName}】的要素类型不是【{fcType}】。\r";
                }
            }

            return result;
        }

        // 检查图层中是否包含相应字段
        public static string IsHaveFieldInLayer(string lyName, string fieldName)
        {
            string result = "";
            List<string> str_fields = new List<string>();
            List<FieldDescription> fields = new List<FieldDescription>();

            var init_featurelayer = lyName.TargetFeatureLayer();
            var init_table = lyName.TargetStandaloneTable();

            // 判断当前选择的是要素图层还是独立表
            if (init_table is not null)
            {
                fields = init_table.GetFieldDescriptions();
            }
            else if (init_featurelayer is not null)
            {
                fields = init_featurelayer.GetFieldDescriptions();
            }
            // 生成字段列表
            foreach (var item in fields)
            {
                str_fields.Add(item.Name);
            }
            // 提取错误信息
            if (!str_fields.Contains(fieldName))
            {
                result += $"【{lyName}】中缺少【{fieldName}】字段\r";
            }

            return result;
        }

        // 检查图层中是否包含相应字段【多个字段】
        public static  string IsHaveFieldInLayer(string lyName, List<string> fieldName)
        {
            string result = "";
            List<string> str_fields = new List<string>();
            List<FieldDescription> fields = new List<FieldDescription>();

            var init_featurelayer = lyName.TargetFeatureLayer();
            var init_table = lyName.TargetStandaloneTable();

            // 判断当前选择的是要素图层还是独立表
            if (init_table is not null)
            {
                fields = init_table.GetFieldDescriptions();
            }
            else if (init_featurelayer is not null)
            {
                fields = init_featurelayer.GetFieldDescriptions();
            }
            // 生成字段列表
            foreach (var item in fields)
            {
                str_fields.Add(item.Name);
            }
            // 提取错误信息
            foreach (var item in fieldName)
            {
                if (!str_fields.Contains(item))
                {
                    result += $"【{lyName}】中缺少【{item}】字段\r";
                }
            }

            return result;
        }

        // 检查字段值是否符合要求
        public static string CheckFieldValue(string lyName, string check_field, List<string> checkStringList)
        {
            string result = "";

            // 判断是否有这个字段
            string result_isHaveField = IsHaveFieldInLayer(lyName, check_field);
            result += result_isHaveField;

            // 获取ID字段
            string IDField = GisTool.GetIDFieldNameFromTarget(lyName);

            // 如果没有字段缺失，刚进行下一步判断
            if (!result_isHaveField.Contains("】中缺少【"))
            {
                // 获取Table
                Table table = lyName.TargetTable();
                // 逐行找出错误
                using RowCursor rowCursor = table.Search();
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    // 获取value
                    var va = row[check_field];
                    if (va == null)
                    {
                        result += $"(OBJECTID:{row[IDField]})：【{lyName}】中的【{check_field}】字段存在空值。\r";
                    }
                    else
                    {
                        string value = va.ToString();
                        if (!checkStringList.Contains(value))
                        {
                            result += $"(OBJECTID:{row[IDField]})：【{lyName}】中的【{check_field}】字段存在不符合要求的字段值【{value}】。\r";
                        }
                    }
                }
            }
            return result;
        }

        // 检查字段值是否有空值
        public static string CheckFieldValueEmpty(string lyName, string check_field, string sql = "")
        {
            string result = "";
            List<string> str_fields = new List<string>();
            List<FieldDescription> fields = new List<FieldDescription>();

            // 判断是否有这个字段
            string result_isHaveField =IsHaveFieldInLayer(lyName, check_field);
            result += result_isHaveField;

            // 获取ID字段
            string IDField = GisTool.GetIDFieldNameFromTarget(lyName);

            if (!result_isHaveField.Contains("】中缺少【"))
            {
                // 判断当前选择的是要素图层还是独立表
                Table table = lyName.TargetTable();
              
                // 逐行找出错误
                using RowCursor rowCursor = table.Search();
                while (rowCursor.MoveNext())
                {
                    using Row row = rowCursor.Current;
                    // 获取value
                    var va = row[check_field];
                    if (va == null)
                    {
                        result += $"(OBJECTID:{row[IDField]})：【{lyName}】中的【{check_field}】字段存在空值。\r";
                    }
                }
            }
            return result;
        }

        // 检查字段值是否有空值【多个字段】
        public static string CheckFieldValueEmpty(string lyName, List<string> check_field)
        {
            string result = "";
            List<string> str_fields = new List<string>();
            List<FieldDescription> fields = new List<FieldDescription>();

            // 判断是否有这个字段
            string result_isHaveField =IsHaveFieldInLayer(lyName, check_field);
            result += result_isHaveField;

            // 获取ID字段
            string IDField = GisTool.GetIDFieldNameFromTarget(lyName);

            foreach (string fd in check_field)
            {
                if (!result_isHaveField.Contains(fd))
                {
                    // 获取Table
                    Table table = lyName.TargetTable();

                    // 逐行找出错误
                    using RowCursor rowCursor = table.Search();
                    while (rowCursor.MoveNext())
                    {
                        using Row row = rowCursor.Current;
                        // 获取value
                        var va = row[fd];
                        if (va == null)
                        {
                            result += $"(OBJECTID:{row[IDField]})：【{lyName}】中的【{fd}】字段存在空值。\r";
                        }
                    }
                }
            }

            return result;
        }
    }
}
