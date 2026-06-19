using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

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

            // ===== 1. 收集墙 =====
            var walls = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList();

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

            // ===== 4. 输出结果 =====
            string result =
                "模型统计结果：\n\n" +
                $"墙数量：{walls.Count}\n" +
                $"门数量：{doors.Count}\n" +
                $"窗数量：{windows.Count}";

            TaskDialog.Show("模型治理工具 v1.0", result);

            return Result.Succeeded;
        }
    }
}