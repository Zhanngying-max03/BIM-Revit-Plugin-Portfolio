using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitPluginDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevitHelloWorld.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            MainWindow window = new MainWindow(doc);
            window.ShowDialog();
            return Result.Succeeded;

            // ===== 1. 收集墙（并计算面积体积） =====
            var wallCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList();

            int wallCount = 0;
            double totalArea = 0;
            double totalVolume = 0;

            foreach (Element elem in wallCollector)
            {
                Wall wall = elem as Wall;
                if (wall == null) continue;

                wallCount++;

                Parameter areaParam = wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                Parameter volumeParam = wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);

                double area = areaParam != null ? areaParam.AsDouble() : 0;
                double volume = volumeParam != null ? volumeParam.AsDouble() : 0;

                totalArea += area;
                totalVolume += volume;
            }

            // 单位转换：平方英尺 → 平方米，立方英尺 → 立方米
            double totalAreaM2 = totalArea * 0.092903;
            double totalVolumeM3 = totalVolume * 0.0283168;

            // ===== 2. 收集门 =====
            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToList();

            // ===== 3. 收集窗 =====
            var windows = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .ToList();

            // ===== 4. 导出CSV到桌面 =====
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktop, "模型统计结果.csv");

            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("构件类型,数量,总面积(㎡),总体积(m³)");
                sw.WriteLine($"墙,{wallCount},{totalAreaM2:F2},{totalVolumeM3:F2}");
                sw.WriteLine($"门,{doors.Count},N/A,N/A");
                sw.WriteLine($"窗,{windows.Count},N/A,N/A");
            }

            // ===== 5. 弹窗显示结果 =====
            string result =
                "✅ 模型统计完成！\n\n" +
                $"墙数量：{wallCount}\n" +
                $"墙总面积：{totalAreaM2:F2} ㎡\n" +
                $"墙总体积：{totalVolumeM3:F2} m³\n\n" +
                $"门数量：{doors.Count}\n" +
                $"窗数量：{windows.Count}\n\n" +
                $"📁 详细数据已导出到桌面：\n{filePath}";

            TaskDialog.Show("模型治理工具 v1.0", result);

            return Result.Succeeded;
        }
    }
}