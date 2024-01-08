using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.GeoProcessing;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Internal.Framework;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.ToolManagers;
using ICSharpCode.SharpZipLib.Zip.Compression;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.OpenXmlFormats.Vml;
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.Manager
{
    public class Arcpy
    {
        // 设置GP工具生成结果的状态，是否加载到当前地图
        public static GPExecuteToolFlags SetGPFlag(bool is_output)
        {
            GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddToHistory;     // 默认将执行工具添加到历史记录
            if (is_output)    // is_output为true时，生成结果加载到当前地图。
            {
                executeFlags = GPExecuteToolFlags.AddToHistory | GPExecuteToolFlags.AddOutputsToMap;
            }
            return executeFlags;  // 返回executeFlags
        }


        // 初始化输入的图层数据
        public static Object InitData(Object inData)
        {
            if (inData is string)
            {
                string text = (string)inData;
                // 如果是带重复标记的图层
                if (text.Contains("："))
                {
                    return text.TargetFeatureLayer();
                }
                else
                {
                    return text;
                }
            }
            else
            {
                return inData;
            }
        }

        // 更改字段
        public static object AlterField(object in_data, string old_field, string new_field, string alias_name, bool is_output = false)
        {
            // 初始化输入数据
            in_data = InitData(in_data);
            // 设置默认GPExecuteToolFlags
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            // 生成GP参数
            var par = Geoprocessing.MakeValueArray(in_data, old_field, new_field, alias_name);
            // 执行GP工具
            Geoprocessing.ExecuteToolAsync("management.AlterField", par, null, null, null, executeFlags);
            // 返回结果
            return in_data;
        }

        // 复制要素
        public static string CopyFeatures(object in_featureClass, string out_featureClass, bool is_output = false, string Z = "Disabled", string M = "Disabled")
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var environments = Geoprocessing.MakeEnvironmentArray(outputZFlag: Z, outputMFlag: M);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass);
            Geoprocessing.ExecuteToolAsync("management.CopyFeatures", par, environments, null, null, executeFlags);
            return out_featureClass;
        }

        // 要素折点转点
        public static string FeatureVerticesToPoints(object in_featureClass, string out_featureClass, string pointType = "ALL", bool is_output = false, string Z = "Disabled", string M = "Disabled")
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var environments = Geoprocessing.MakeEnvironmentArray(outputZFlag: Z, outputMFlag: M);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, pointType);
            Geoprocessing.ExecuteToolAsync("management.FeatureVerticesToPoints", par, environments, null, null, executeFlags);
            return out_featureClass;
        }

        // 复制表
        public static object CopyRows(object in_table, object out_table, bool is_output = false)
        {
            in_table = InitData(in_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_table, out_table);
            Geoprocessing.ExecuteToolAsync("management.CopyRows", par, null, null, null, executeFlags);
            return out_table;
        }

        // 追加
        public static object Append(object append_featureClass, object target_featureClass, bool is_output = false)
        {
            append_featureClass = InitData(append_featureClass);
            target_featureClass = InitData(target_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(append_featureClass, target_featureClass, "NO_TEST");
            Geoprocessing.ExecuteToolAsync("management.Append", par, null, null, null, executeFlags);
            return target_featureClass;
        }

        // 裁剪栅格
        public static object RasterClip(object in_raster, object out_raster, object mask, bool is_output = false)
        {
            in_raster = InitData(in_raster);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_raster, "", out_raster, mask, "-1", "ClippingGeometry", "MAINTAIN_EXTENT");
            Geoprocessing.ExecuteToolAsync("management.Clip", par, null, null, null, executeFlags);
            return out_raster;
        }
        // 合并
        public static object Merge(List<string> in_tables, object out_table, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_tables, out_table);
            Geoprocessing.ExecuteToolAsync("management.Merge", par, null, null, null, executeFlags);
            return out_table;
        }

        // 按属性选择图层
        public static object SelectLayerByAttribute(object in_data, string sql, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, "NEW_SELECTION", sql, null);
            Geoprocessing.ExecuteToolAsync("management.SelectLayerByAttribute", par, null, null, null, executeFlags);
            return in_data;
        }

        // 创建要素图层
        public static string MakeFeatureLayer(object in_data, string out_layer, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, out_layer, "", null);
            Geoprocessing.ExecuteToolAsync("management.MakeFeatureLayer", par, null, null, null, executeFlags);
            return out_layer;
        }

        // 图层转KML
        public static object LayerToKML(object in_data, object out_data, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, out_data, 0, "NO_COMPOSITE");
            Geoprocessing.ExecuteToolAsync("conversion.LayerToKML", par, null, null, null, executeFlags);
            return out_data;
        }

        // 消除
        public static object Eliminate(object in_layer, object out_data, string ex_sql, bool is_output = false)
        {
            in_layer = InitData(in_layer);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_layer, out_data, "LENGTH", ex_sql, null);
            Geoprocessing.ExecuteToolAsync("management.Eliminate", par, null, null, null, executeFlags);
            return out_data;
        }

        // 最小边界几何
        public static object MinimumBoundingGeometry(object in_data, object out_data, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, out_data, "ENVELOPE");
            Geoprocessing.ExecuteToolAsync("management.MinimumBoundingGeometry", par, null, null, null, executeFlags);
            return out_data;
        }

        // 单部件转多部件
        public static object MultipartToSinglepart(object in_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass);
            Geoprocessing.ExecuteToolAsync("management.MultipartToSinglepart", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 创建要素类【类型：POINT；POLYLINE；POLYGON】【坐标系，例：CGCS2000_3_Degree_GK_Zone_39】
        public static string CreateFeatureclass(string in_gdb, string fc_name, string fc_type, string spatialReference, string alias_name = "", bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_gdb, fc_name, fc_type, null, "DISABLED", "DISABLED", spatialReference, "", 0, 0, 0, alias_name);
            Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", par, null, null, null, executeFlags);
            return @$"{in_gdb}\{fc_name}";
        }

        // 计算几何属性
        public static object CalculateGeometryAttributes(object in_data, string cal_text, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, cal_text, "", "", null, "SAME_AS_INPUT");
            Geoprocessing.ExecuteToolAsync("management.CalculateGeometryAttributes", par, null, null, null, executeFlags);
            return in_data;
        }

        // 排序
        public static string Sort(object in_data, string out_data, string cal_text, string direction, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, out_data, cal_text, direction);
            Geoprocessing.ExecuteToolAsync("management.Sort", par, null, null, null, executeFlags);
            return out_data;
        }

        // 创建独立表
        public static string CreateTable(string in_gdb, string table_name, string alias_name = "", bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_gdb, table_name, null, "", alias_name);
            Geoprocessing.ExecuteToolAsync("management.CreateTable", par, null, null, null, executeFlags);
            return @$"{in_gdb}\{table_name}";
        }

        // 创建GDB数据库
        public static string CreateFileGDB(string file_path, string gdb_name, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(file_path, gdb_name);
            Geoprocessing.ExecuteToolAsync("management.CreateFileGDB", par, null, null, null, executeFlags);
            return @$"{file_path}\{gdb_name}.gdb";
        }

        // 标识
        public static string Identity(object in_featureClass, object identity_featureClass, string out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            identity_featureClass = InitData(identity_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, identity_featureClass, out_featureClass, "ALL", "", "NO_RELATIONSHIPS");
            Geoprocessing.ExecuteToolAsync("analysis.Identity", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 应用图层的符号设置【2个参数】
        public static object ApplySymbologyFromLayer(object in_data, object in_layer, bool is_output = false)
        {
            in_data = InitData(in_data);
            in_layer = InitData(in_layer);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, in_layer);
            Geoprocessing.ExecuteToolAsync("management.ApplySymbologyFromLayer", par, null, null, null, executeFlags);
            return in_data;
        }

        // 应用图层的符号设置【3个参数】
        public static object ApplySymbologyFromLayer(object in_data, object in_layer, string symbology, bool is_output = false)
        {
            in_data = InitData(in_data);
            in_layer = InitData(in_layer);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, in_layer, symbology, "DEFAULT");
            Geoprocessing.ExecuteToolAsync("management.ApplySymbologyFromLayer", par, null, null, null, executeFlags);
            return in_data;
        }

        // 将图层符号与样式匹配
        public static object MatchLayerSymbologyToAStyle(object in_data, string field, string stylx, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, "$feature." + field, stylx);
            Geoprocessing.ExecuteToolAsync("management.MatchLayerSymbologyToAStyle", par, null, null, null, executeFlags);
            return in_data;
        }

        // 融合
        public static object Dissolve(object in_featureClass, object out_featureClass, string sql = "", bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, sql, "", "MULTI_PART", "DISSOLVE_LINES", "");
            Geoprocessing.ExecuteToolAsync("management.Dissolve", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 面转线
        public static object PolygonToLine(object in_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, "IDENTIFY_NEIGHBORS");
            Geoprocessing.ExecuteToolAsync("management.PolygonToLine", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 要素转面
        public static object FeatureToPolygon(object in_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, "", "ATTRIBUTES", "");
            Geoprocessing.ExecuteToolAsync("management.FeatureToPolygon", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 要素转线 
        public static object FeatureToLine(object in_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, "", "ATTRIBUTES");
            Geoprocessing.ExecuteToolAsync("management.FeatureToLine", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 筛选
        public static string Select(object in_featureClass, string out_featureClass, string sql, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, sql);
            Geoprocessing.ExecuteToolAsync("analysis.Select", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 表筛选
        public static object TableSelect(object in_table, object out_table, string sql, bool is_output = false)
        {
            in_table = InitData(in_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_table, out_table, sql);
            Geoprocessing.ExecuteToolAsync("analysis.TableSelect", par, null, null, null, executeFlags);
            return out_table;
        }

        // 按属性分割
        public static object SplitByAttributes(object in_table, object out_gdb, string splite_field, bool is_output = false)
        {
            in_table = InitData(in_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_table, out_gdb, splite_field);
            Geoprocessing.ExecuteToolAsync("analysis.SplitByAttributes", par, null, null, null, executeFlags);
            return out_gdb;
        }

        // 汇总统计数据
        public static object Statistics(object in_table, object out_table, string statistics_fields, string case_field, bool is_output = false)
        {
            in_table = InitData(in_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_table, out_table, statistics_fields, case_field, "");
            Geoprocessing.ExecuteToolAsync("analysis.Statistics", par, null, null, null, executeFlags);
            return out_table;
        }

        // 裁剪
        public static object Clip(object in_featureClass, object clip_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            clip_featureClass = InitData(clip_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, clip_featureClass, out_featureClass, "");
            Geoprocessing.ExecuteToolAsync("analysis.Clip", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 缓冲区
        public static object Buffer(object in_featureClass, object out_featureClass, string dis, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, out_featureClass, dis);
            Geoprocessing.ExecuteToolAsync("analysis.Buffer", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 擦除
        public static object Erase(object in_featureClass, object erase_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            erase_featureClass = InitData(erase_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, erase_featureClass, out_featureClass, "");
            Geoprocessing.ExecuteToolAsync("analysis.Erase", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 相交
        public static object Intersect(List<string> in_featureClasses, object out_featureClass, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClasses, out_featureClass, "ALL", "", "INPUT");
            Geoprocessing.ExecuteToolAsync("analysis.Intersect", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 联合
        public static object Union(List<string> in_featureClasses, object out_featureClass, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClasses, out_featureClass, "ALL", "", "GAPS");
            Geoprocessing.ExecuteToolAsync("analysis.Union", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 空间连接
        public static object SpatialJoin(object in_featureClass, object join_featureClass, object out_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            join_featureClass = InitData(join_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, join_featureClass, out_featureClass);
            Geoprocessing.ExecuteToolAsync("analysis.SpatialJoin", par, null, null, null, executeFlags);
            return out_featureClass;
        }

        // 修复几何
        public static object RepairGeometry(object in_featureClass, bool is_output = false)
        {
            in_featureClass = InitData(in_featureClass);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_featureClass, "DELETE_NULL", "ESRI");
            Geoprocessing.ExecuteToolAsync("management.RepairGeometry", par, null, null, null, executeFlags);
            return in_featureClass;
        }

        // 更新
        public static object Update(object in_fc, object updata_fc, object out_fc, bool is_output = false)
        {
            in_fc = InitData(in_fc);
            updata_fc = InitData(updata_fc);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_fc, updata_fc, out_fc, "BORDERS", null);
            Geoprocessing.ExecuteToolAsync("analysis.Update", par, null, null, null, executeFlags);
            return out_fc;
        }

        // 连接字段
        public static object JoinField(object in_data, string in_field, object join_table, string join_field, List<string> fields, bool is_output = false)
        {
            in_data = InitData(in_data);
            join_table = InitData(join_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, in_field, join_table, join_field, fields, "NOT_USE_FM", "");
            Geoprocessing.ExecuteToolAsync("management.JoinField", par, null, null, null, executeFlags);
            return in_data;
        }

        // 计算字段
        public static object CalculateField(object in_data, string field, string expression, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, field, expression, "PYTHON3", "", "", "NO_ENFORCE_DOMAINS");
            Geoprocessing.ExecuteToolAsync("management.CalculateField", par, null, null, null, executeFlags);
            return in_data;
        }

        // 计算字段(加代码块)
        public static object CalculateField(object in_data, string field, string expression, string block, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, field, expression, "PYTHON3", block, "", "NO_ENFORCE_DOMAINS");
            Geoprocessing.ExecuteToolAsync("management.CalculateField", par, null, null, null, executeFlags);
            return in_data;
        }


        // 删除字段(多个)【method为删除方法，包括删除字段{DELETE_FIELDS}和保留字段{KEEP_FIELDS}】
        public static object DeleteField(object in_data, string drop_field, string method = "DELETE_FIELDS", bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, drop_field, method);
            Geoprocessing.ExecuteToolAsync("management.DeleteField", par, null, null, null, executeFlags);
            return in_data;
        }

        // 删除字段(多个)【method为删除方法，包括删除字段{DELETE_FIELDS}和保留字段{KEEP_FIELDS}】
        public static object DeleteField(object in_data, List<string> drop_fields, string method = "DELETE_FIELDS", bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_data, drop_fields, method);
            Geoprocessing.ExecuteToolAsync("management.DeleteField", par, null, null, null, executeFlags);
            return in_data;
        }

        // 获取计数
        public static int GetCount(object in_table, bool is_output = false)
        {
            in_table = InitData(in_table);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_table);
            var result = Geoprocessing.ExecuteToolAsync("management.GetCount", par, null, null, null, executeFlags);
            int count = int.Parse(result.Result.Values[0].ToString());
            return count;
        }

        // 检查几何
        public static object CheckGeometry(object in_fc, object out_table, bool is_output = false)
        {
            in_fc = InitData(in_fc);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(in_fc, out_table, "ESRI");
            Geoprocessing.ExecuteToolAsync("management.CheckGeometry", par, null, null, null, executeFlags);
            return out_table;
        }

        // 新建拓扑
        public static string CreateTopology(string db_path, string top_name, double tolerance = 0.001, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(db_path, top_name, tolerance);
            Geoprocessing.ExecuteToolAsync("management.CreateTopology", par, null, null, null, executeFlags);
            return $@"{db_path}\{top_name}";
        }

        // 向拓扑中添加要素
        public static string AddFeatureClassToTopology(string top_path, string featureClass_path, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(top_path, featureClass_path);
            Geoprocessing.ExecuteToolAsync("management.AddFeatureClassToTopology", par, null, null, null, executeFlags);
            return featureClass_path;
        }

        // 添加拓扑规则
        public static string AddRuleToTopology(string top_path, string rule, string featureClass_path, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(top_path, rule, featureClass_path, null, null, null);
            Geoprocessing.ExecuteToolAsync("management.AddRuleToTopology", par, null, null, null, executeFlags);
            return top_path;
        }

        // 验证拓扑
        public static string ValidateTopology(string top_path, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(top_path);
            Geoprocessing.ExecuteToolAsync("management.ValidateTopology", par, null, null, null, executeFlags);
            return top_path;
        }

        // 输出TOP错误
        public static string ExportTopologyErrors(string top_path, string gdb_path, string err_word, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(top_path, gdb_path, err_word);
            Geoprocessing.ExecuteToolAsync("management.ExportTopologyErrors", par, null, null, null, executeFlags);
            return gdb_path;
        }

        // 添加字段
        public static object AddField(object in_data, string insert_field, string field_type, string field_alias = "", int? field_length = null, bool is_output = false)
        {
            in_data = InitData(in_data);
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            if (field_length.HasValue)
            {
                int length = field_length.Value;
                // 使用length进行计算
                var par = Geoprocessing.MakeValueArray(in_data, insert_field, field_type, null, null, length, field_alias, null, null, null);
                Geoprocessing.ExecuteToolAsync("management.AddField", par, null, null, null, executeFlags);
            }
            else
            {
                var par = Geoprocessing.MakeValueArray(in_data, insert_field, field_type, null, null, field_length, field_alias, null, null, null);
                Geoprocessing.ExecuteToolAsync("management.AddField", par, null, null, null, executeFlags);
            }
            return in_data;
        }

        // 在数据库中创建要素数据集
        public static string CreateFeatureDataset(string gdb_path, string db_name, SpatialReference sr, bool is_output = false)
        {
            GPExecuteToolFlags executeFlags = SetGPFlag(is_output);
            var par = Geoprocessing.MakeValueArray(gdb_path, db_name, sr);
            Geoprocessing.ExecuteToolAsync("management.CreateFeatureDataset", par, null, null, null, executeFlags);
            return $@"{gdb_path}\{db_name}";
        }

        // 删除数据
        public static void Delect(object in_featureClass)
        {
            var par = Geoprocessing.MakeValueArray(in_featureClass);
            Geoprocessing.ExecuteToolAsync("management.Delete", par);
        }

        // 删除数据(多个)
        public static void Delect(List<string> in_featureClasses)
        {
            var par = Geoprocessing.MakeValueArray(in_featureClasses);
            Geoprocessing.ExecuteToolAsync("management.Delete", par);
        }

    }
}
