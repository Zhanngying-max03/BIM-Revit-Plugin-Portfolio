using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RevitPluginDemo
{
    [Transaction(TransactionMode.Manual)]
    public class HelloWorld : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 1. 获取Revit文档核心对象（固定写法，所有插件通用）
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 2. 构件收集器：筛选模型里所有【墙体】
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallElements = collector.OfClass(typeof(Wall)).ToElements();

            // 3. 定义统计变量（造价常用：数量、总面积、总体积）
            int wallCount = 0;          // 墙体总数
            double totalArea = 0;       // 墙体总面积（Revit内部单位：平方英尺）
            double totalVolume = 0;     // 墙体总体积（Revit内部单位：立方英尺）

            // 4. 循环遍历每一面墙，累加数据
            foreach (Element elem in wallElements)
            {
                Wall wall = elem as Wall;
                if (wall == null) continue;

                wallCount++;
                // 读取墙的面积、体积参数（Revit内置参数）
                totalArea += wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble();
                totalVolume += wall.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble();
            }

            // 5. 手动单位转换：Revit内部是英尺，转成国内常用 ㎡ / m³
            // 换算系数：1英尺=0.3048米，所以1平方英尺=0.09290304㎡，1立方英尺=0.0283168466m³
            double areaM2 = totalArea * 0.09290304;
            double volumeM3 = totalVolume * 0.0283168466;

            // 6. 弹窗展示统计结果
            string result = $"墙体统计结果：\n" +
                            $"墙体总数：{wallCount} 面\n" +
                            $"总面积：{areaM2:F2} ㎡\n" +
                            $"总体积：{volumeM3:F2} m³";

            TaskDialog.Show("构件统计", result);

            return Result.Succeeded;
        }
    }
}