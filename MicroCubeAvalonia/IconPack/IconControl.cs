using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using MicroCubeAvalonia.IconPack.Icons;
using System;

namespace MicroCubeAvalonia.IconPack
{
    public class IconControl : ContentControl
    {
        public static readonly AvaloniaProperty KindProperty =
            AvaloniaProperty.Register<IconControl, string>(
                "Kind",
                inherits: true,
                notifying: (o, b) => UpdatedBindings(o));

        public static readonly AvaloniaProperty BindableKindProperty =
            AvaloniaProperty.Register<IconControl, object>(
                "BindableKind",
                inherits: true,
                notifying: (o, b) => UpdatedBindings(o));

        public static readonly DirectProperty<IconControl, Geometry> IconDataProperty =
            AvaloniaProperty.RegisterDirect<IconControl, Geometry>(
            "IconData",
            (ic) =>
            ic.iconData);

        private Geometry iconData;

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public string Kind
        {
            get => (string)this.GetValue(KindProperty);
            set
            {
                this.SetValue(KindProperty, value);
                this.UpdateData();
            }
        }

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public object BindableKind
        {
            get => (object)this.GetValue(BindableKindProperty);
            set
            {
                this.SetValue(BindableKindProperty, value);
                this.UpdateData();
            }
        }

        private static void UpdatedBindings(IAvaloniaObject avaloniaObject)
        {
            if (avaloniaObject is IconControl ic)
            {
                ic.UpdateData();
            }
        }

        private void UpdateData()
        {
            var newIconPathData = string.Empty;

            if (this.BindableKind != null)
            {
                newIconPathData = this.GetPathData(this.BindableKind);
            }
            else if (!string.IsNullOrEmpty(this.Kind))
            {
                newIconPathData = this.GetPathData(this.Kind);
            }

            var newGeometry = Geometry.Parse(newIconPathData);
            this.SetAndRaise(IconDataProperty, ref this.iconData, newGeometry);
        }

        private string GetPathData(string iconPath)
        {
            var iconNameParts = iconPath.Split(new char[] { '.' }, 2);
            var iconPackName = iconNameParts[0];
            var iconName = iconNameParts[1];

            var data = string.Empty;

            switch (iconPackName)
            {
                case nameof(PackIconEntypoKind):
                    PackIconEntypoDataFactory.DataIndex.Value?.TryGetValue((PackIconEntypoKind)Enum.Parse(typeof(PackIconEntypoKind), iconName), out data);
                    return data;
                case nameof(PackIconFeatherIconsKind):
                    PackIconFeatherIconsDataFactory.DataIndex.Value?.TryGetValue((PackIconFeatherIconsKind)Enum.Parse(typeof(PackIconFeatherIconsKind), iconName), out data);
                    return data;
                case nameof(PackIconFontAwesomeKind):
                    PackIconFontAwesomeDataFactory.DataIndex.Value?.TryGetValue((PackIconFontAwesomeKind)Enum.Parse(typeof(PackIconFontAwesomeKind), iconName), out data);
                    return data;
                case nameof(PackIconMaterialKind):
                    PackIconMaterialDataFactory.DataIndex.Value?.TryGetValue((PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), iconName), out data);
                    return data;
                case nameof(PackIconOcticonsKind):
                    PackIconOcticonsDataFactory.DataIndex.Value?.TryGetValue((PackIconOcticonsKind)Enum.Parse(typeof(PackIconOcticonsKind), iconName), out data);
                    return data;
                default:
                    return null;
            }
        }

        private string GetPathData(object iconKind)
        {
            string data = null;
            switch (iconKind)
            {
                case PackIconEntypoKind entypoKind:
                    PackIconEntypoDataFactory.DataIndex.Value?.TryGetValue(entypoKind, out data);
                    return data;
                case PackIconFeatherIconsKind featherKind:
                    PackIconFeatherIconsDataFactory.DataIndex.Value?.TryGetValue(featherKind, out data);
                    return data;
                case PackIconFontAwesomeKind fontAwesomeKind:
                    PackIconFontAwesomeDataFactory.DataIndex.Value?.TryGetValue(fontAwesomeKind, out data);
                    return data;
                case PackIconMaterialKind materialKind:
                    PackIconMaterialDataFactory.DataIndex.Value?.TryGetValue(materialKind, out data);
                    return data;
                case PackIconOcticonsKind octiconsKind:
                    PackIconOcticonsDataFactory.DataIndex.Value?.TryGetValue(octiconsKind, out data);
                    return data;
                default:
                    return null;
            }
        }
    }
}
