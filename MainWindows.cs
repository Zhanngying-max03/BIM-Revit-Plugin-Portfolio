using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RevitPluginDemo
{
    public partial class MainWindow : Window
    {
        private Document _doc;
        private string _statResult;

        public MainWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            txtStatus.Text = "已连接到Revit模型";
        }

        private void BtnStat_Click(object sender, RoutedEventArgs e)
        {
            if (_doc == null)
            {
                MessageBox.Show("未连接到Revit文档");
                return;
            }

            try
            {
                string dimension = (cmbDimension.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "按构件类型";
                var result = RunStatistics(dimension);
                _statResult = result;
                txtResult.Text = result;
                txtStatus.Text = $"✅ 统计完成！维度：{dimension}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"统计出错：{ex.Message}");
                txtStatus.Text = "❌ 统计出错";
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_statResult))
            {
                MessageBox.Show("请先执行统计");
                return;
            }

            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktop, "工程量统计结果.csv");
                File.WriteAllText(filePath, _statResult, Encoding.UTF8);
                MessageBox.Show($"✅ 已导出到桌面：\n{filePath}");
                txtStatus.Text = $"📤 已导出：{Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}");
            }
        }

        // ========== 核心统计函数 ==========
        private string RunStatistics(string dimension)
        {
            // 1. 先收集所有墙（注意：是 FilteredElementCollector，不是 FilteredList）
            var allWalls = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .ToList();

            // 2. 根据维度分组
            IEnumerable<IGrouping<string, Wall>> groups = null;

            if (dimension == "按楼层")
            {
                groups = allWalls
                    .GroupBy(w =>
                    {
                        // 获取墙所在的楼层名称
                        var level = _doc.GetElement(w.LevelId) as Level;
                        return level?.Name ?? "未指定楼层";
                    });
            }
            else if (dimension == "按材质")
            {
                groups = allWalls
                    .GroupBy(w =>
                    {
                        var materialId = w.GetMaterialIds(false).FirstOrDefault();
                        if (materialId != null && materialId != ElementId.InvalidElementId)
                        {
                            var material = _doc.GetElement(materialId) as Material;
                            return material?.Name ?? "无材质";
                        }
                        return "无材质";
                    });
            }
            else // 默认按构件类型
            {
                return RunDefaultStatistics();
            }

            // 3. 如果按楼层或材质分组，构建分组结果
            if (groups != null)
            {
                var result = new StringBuilder();
                result.AppendLine($"📊 统计维度：{dimension}");
                result.AppendLine(new string('=', 40));

                foreach (var group in groups)
                {
                    double totalArea = 0;
                    double totalVolume = 0;
                    int count = 0;

                    foreach (var wall in group)
                    {
                        count++;
                        var areaParam = wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                        var volumeParam = wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                        totalArea += areaParam != null ? areaParam.AsDouble() : 0;
                        totalVolume += volumeParam != null ? volumeParam.AsDouble() : 0;
                    }

                    double areaM2 = totalArea * 0.092903;
                    double volumeM3 = totalVolume * 0.0283168;

                    result.AppendLine($"\n🏷️ {group.Key}");
                    result.AppendLine($"  数量：{count}");
                    result.AppendLine($"  总面积：{areaM2:F2} ㎡");
                    result.AppendLine($"  总体积：{volumeM3:F2} m³");
                }

                result.AppendLine($"\n{new string('=', 40)}");
                result.AppendLine($"⏱ 统计时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                return result.ToString();
            }

            return "未找到任何构件";
        }

        // ========== 默认统计（按构件类型） ==========
        private string RunDefaultStatistics()
        {
            var walls = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList();

            int wallCount = 0;
            double totalArea = 0;
            double totalVolume = 0;

            foreach (Element elem in walls)
            {
                Wall wall = elem as Wall;
                if (wall == null) continue;
                wallCount++;
                Parameter areaParam = wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                Parameter volumeParam = wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                totalArea += areaParam != null ? areaParam.AsDouble() : 0;
                totalVolume += volumeParam != null ? volumeParam.AsDouble() : 0;
            }

            double areaM2 = totalArea * 0.092903;
            double volumeM3 = totalVolume * 0.0283168;

            var doors = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToList();

            var windows = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .ToList();

            string result =
                $"📊 统计维度：按构件类型\n" +
                $"{new string('=', 40)}\n\n" +
                $"🧱 墙体\n" +
                $"  数量：{wallCount}\n" +
                $"  总面积：{areaM2:F2} ㎡\n" +
                $"  总体积：{volumeM3:F2} m³\n\n" +
                $"🚪 门\n  数量：{doors.Count}\n\n" +
                $"🪟 窗\n  数量：{windows.Count}\n" +
                $"{new string('=', 40)}\n" +
                $"⏱ 统计时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            return result;
        }
    }
}