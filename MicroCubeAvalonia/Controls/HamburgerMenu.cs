using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace MicroCubeAvalonia.Controls
{
    public class HamburgerMenu : ContentControl
    {
        private bool? isPaneOpen = false;

        public static AvaloniaProperty<bool?> IsPaneOpenProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenu, bool?>(
                nameof(IsPaneOpen),
                o => o.IsPaneOpen,
                (o, v) => o.IsPaneOpen = v,
                unsetValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        public static AvaloniaProperty ItemsProperty =
            AvaloniaProperty.Register<HamburgerMenu, AvaloniaList<HamburgerMenuItem>>(
            nameof(Items));

        public static AvaloniaProperty OptionItemsProperty =
            AvaloniaProperty.Register<HamburgerMenu, AvaloniaList<HamburgerMenuItem>>(
                nameof(OptionItems));

        public static AvaloniaProperty SidebarBrushProperty =
            AvaloniaProperty.Register<HamburgerMenu, IBrush>(
                nameof(SidebarBrush));

        public static AvaloniaProperty SidebarForegroundProperty =
           AvaloniaProperty.Register<HamburgerMenu, IBrush>(
               nameof(SidebarForeground));

        public static AvaloniaProperty HoverHighlightBrushProperty =
            AvaloniaProperty.Register<HamburgerMenu, IBrush>(
                nameof(HoverHighlightBrush));

        public static AvaloniaProperty SelectedItemProperty =
            AvaloniaProperty.Register<HamburgerMenu, HamburgerMenuItem>(
                nameof(SelectedItem),
                inherits: true,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
                notifying: (a,b) => SelectionChanged(a, b),
                validate: SelectionValidator);

        public static AvaloniaProperty SelectedContentProperty =
            AvaloniaProperty.RegisterDirect<HamburgerMenu, object>(
                nameof(SelectedContent),
                (hm) => hm.SelectedContent);

        public bool? IsPaneOpen
        {
            get => this.isPaneOpen;
            set => this.SetAndRaise(IsPaneOpenProperty, ref this.isPaneOpen, value);
        }

        public AvaloniaList<HamburgerMenuItem> Items
        {
            get => (AvaloniaList<HamburgerMenuItem>)this.GetValue(ItemsProperty);
            set => this.SetValue(ItemsProperty, value);
        }

        public AvaloniaList<HamburgerMenuItem> OptionItems
        {
            get => (AvaloniaList<HamburgerMenuItem>)this.GetValue(OptionItemsProperty);
            set => this.SetValue(OptionItemsProperty, value);
        }

        public IBrush SidebarBrush
        {
            get => (IBrush)this.GetValue(SidebarBrushProperty);
            set => this.SetValue(SidebarBrushProperty, value);
        }

        public IBrush SidebarForeground
        {
            get => (IBrush)this.GetValue(SidebarForegroundProperty);
            set => this.SetValue(SidebarForegroundProperty, value);
        }

        public IBrush HoverHighlightBrush
        {
            get => (IBrush)this.GetValue(HoverHighlightBrushProperty);
            set => this.SetValue(HoverHighlightBrushProperty, value);
        }

        public HamburgerMenuItem SelectedItem
        {
            get => (HamburgerMenuItem)this.GetValue(SelectedItemProperty);
            set
            {
                this.SetValue(SelectedItemProperty, value);
                SelectionChanged(this, true);
            }
        }

        public object SelectedContent
        {
            get => this.SelectedItem?.Tag;
        }

        public static void SelectionChanged(IAvaloniaObject avaloniaObject, bool done)
        {
            if (avaloniaObject is HamburgerMenu hamburgerMenu && done)
            {
                hamburgerMenu.RaisePropertyChanged(SelectedContentProperty, null, hamburgerMenu.SelectedContent);
            }
        }

        public static HamburgerMenuItem SelectionValidator(HamburgerMenu menu, HamburgerMenuItem item)
        {
            if (item != null)
            {
                return item;
            }
            else
            {
                return menu.SelectedItem;
            }
        }
    }
}
