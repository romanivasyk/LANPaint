using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LANPaint.Converters;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace LANPaint.Views
{
    public partial class Paint : Window
    {
        public Paint()
        {
            InitializeComponent();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            if (toolBar?.Template.FindName("OverflowGrid", toolBar) is not FrameworkElement overflowGrid) return;

            var toolBarType = toolBar.GetType();
            var hasOverflowItemsPropertyInfo =
                toolBarType.GetField("HasOverflowItemsProperty", BindingFlags.Public | BindingFlags.Static);
            var hasOverflowItemsProperty = (DependencyProperty) hasOverflowItemsPropertyInfo.GetValue(toolBar);

            var overflowGridType = overflowGrid.GetType();
            var visibilityPropertyInfo = overflowGridType.GetField("VisibilityProperty",
                BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
            var visibilityProperty = (DependencyProperty) visibilityPropertyInfo.GetValue(overflowGrid);

            var binding = new Binding
            {
                Source = toolBar,
                Path = new PropertyPath(hasOverflowItemsProperty),
                Mode = BindingMode.OneWay,
                Converter = new VisibilityToBoolConverter {Inverted = true}
            };

            BindingOperations.SetBinding(overflowGrid, visibilityProperty, binding);
        }
    }
}