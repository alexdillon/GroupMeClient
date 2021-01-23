using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MicroCubeAvalonia.Controls
{
    public class HamburgerMenuItem : ContentControl
    {
        public static AvaloniaProperty IconProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenuItem, object>(
                nameof(Icon),
                (hmi) => hmi.Icon,
                (hmi, value) => hmi.Icon = value);

        public static AvaloniaProperty LabelProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenuItem, string>(
                nameof(Label),
                (hmi) => hmi.Label,
                (hmi, value) => hmi.Label = value);

        public static AvaloniaProperty ToolTipProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenuItem, string>(
                nameof(ToolTip),
                (hmi) => hmi.ToolTip,
                (hmi, value) => hmi.ToolTip = value);

        public static AvaloniaProperty TagProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenuItem, object>(
                nameof(Tag),
                (hmi) => hmi.Tag,
                (hmi, value) => hmi.Tag = value);

        //public static AvaloniaProperty ShowItemProperty =
        //  AvaloniaProperty.RegisterDirect<HamburgerMenuItem, bool>(
        //      nameof(ShowItem),
        //      (hmi) => hmi.ShowItem,
        //      (hmi, value) => hmi.ShowItem = value);

        public object Icon { get; set; }

        public string Label { get; set; }

        public string ToolTip { get; set; }

        public object Tag { get; set; }

        //public bool ShowItem { get; set; } = true;
    }
}
