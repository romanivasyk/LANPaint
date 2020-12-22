using LANPaint_vNext.Model;
using LANPaint_vNext.Services;
using System.Collections.Generic;
using System.Windows;

namespace UDPBroadcastTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var strokeAttr = new StrokeAttributes();
            strokeAttr.Color = ARGBColor.Default;
            strokeAttr.IgnorePressure = true;
            strokeAttr.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
            strokeAttr.Width = 3;
            strokeAttr.Height = 3;

            List<Point> points = new List<Point>() { new Point { X = 25, Y = 100 }, new Point { X = 26, Y = 100 }, new Point { X = 27, Y = 100 },
                                                    new Point { X=28, Y=100},new Point { X=29, Y=100},new Point { X=30, Y=100},new Point { X=31, Y=100},};

            var stroke = new SerializableStroke(strokeAttr, points);
            var info = new DrawingInfo(new ARGBColor(255, 180, 190, 200), stroke);

            var serializer = new BinarySerializerService();

            var bytes = serializer.Serialize(info);
            var deserializedInfo = serializer.Deserialize<DrawingInfo>(bytes);

            System.Console.WriteLine(info.Equals(deserializedInfo));
        }
    }
}
