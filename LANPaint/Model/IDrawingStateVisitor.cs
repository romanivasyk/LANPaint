using System;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint.Model
{
    public interface IDrawingStateVisitor
    {
        public void ClearState();
        public void ChangeBackgroundState(BackgroundStateComponent backgroundComponent);
        public void EraseState(EraseStateComponent eraseComponent);
        public void DrawState(DrawStateComponent drawComponent);
        public void SnapshotState(SnapshotStateComponent snapshotComponent);
    }

    public interface IDrawingComponent
    {
        public void AcceptDrawingState(IDrawingStateVisitor visitor);
    }

    [Serializable]
    public class ClearDrawingStateComponent : IDrawingComponent
    {
        public void AcceptDrawingState(IDrawingStateVisitor visitor)
        {
            visitor.ClearState();
        }
    }
    [Serializable]
    public class BackgroundStateComponent : IDrawingComponent
    {
        public ARGBColor Background { get; }

        public BackgroundStateComponent(ARGBColor background)
        {
            Background = background;
        }

        public BackgroundStateComponent(Color background) : this(ARGBColor.FromColor(background))
        { }

        public void AcceptDrawingState(IDrawingStateVisitor visitor)
        {
            visitor.ChangeBackgroundState(this);
        }
    }
    [Serializable]
    public class EraseStateComponent : IDrawingComponent
    {
        public SerializableStroke Stroke { get; }

        public EraseStateComponent(SerializableStroke stroke)
        {
            Stroke = stroke;
        }

        public EraseStateComponent(Stroke stroke) : this(SerializableStroke.FromStroke(stroke))
        { }

        public void AcceptDrawingState(IDrawingStateVisitor visitor)
        {
            visitor.EraseState(this);
        }
    }
    [Serializable]
    public class DrawStateComponent : IDrawingComponent
    {
        public SerializableStroke Stroke { get; }

        public DrawStateComponent(SerializableStroke stroke)
        {
            Stroke = stroke;
        }

        public DrawStateComponent(Stroke stroke) : this(SerializableStroke.FromStroke(stroke))
        { }

        public void AcceptDrawingState(IDrawingStateVisitor visitor)
        {
            visitor.DrawState(this);
        }
    }
    [Serializable]
    public class SnapshotStateComponent : IDrawingComponent
    {
        public ARGBColor Background { get; }
        public IEnumerable<SerializableStroke> Strokes { get; }

        public SnapshotStateComponent(ARGBColor background, IEnumerable<SerializableStroke> strokes)
        {
            Background = background;
            Strokes = strokes;
        }

        public void AcceptDrawingState(IDrawingStateVisitor visitor)
        {
            visitor.SnapshotState(this);
        }
    }
}
