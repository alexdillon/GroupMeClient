using System;
using Avalonia;
using Avalonia.Controls;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="LikesCanvasExtension"/> provides extension methods for <see cref="Canvas"/> for controlling liker display behavior.
    /// </summary>
    public class LikesCanvasExtension
    {
        /// <summary>
        /// An Avalonia Attached property to represent the like count.
        /// </summary>
        public static readonly AvaloniaProperty<string> LikeCountProperty =
               AvaloniaProperty.RegisterAttached<Canvas, string>(
                "LikeCount",
                typeof(LikesCanvasExtension),
                defaultValue: default(string));

        /// <summary>
        /// An Avalonia Attached property to represent the like canvas area mouse over.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsMouseOverAreaProperty =
              AvaloniaProperty.RegisterAttached<Canvas, bool>(
                  "IsMouseOverArea",
                  typeof(LikesCanvasExtension),
                  defaultValue: default(bool));

        /// <summary>
        /// An Avalonia Attached property to represent the like bubble mouse over.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsMouseOverPopupProperty =
            AvaloniaProperty.RegisterAttached<Canvas, bool>(
                "IsMouseOverPopup",
                typeof(LikesCanvasExtension),
                defaultValue: default(bool));

        static LikesCanvasExtension()
        {
            LikeCountProperty.Changed.Subscribe(PropertyChanged);
            IsMouseOverAreaProperty.Changed.Subscribe(PropertyChanged);
            IsMouseOverPopupProperty.Changed.Subscribe(PropertyChanged);
        }

        /// <summary>
        /// Gets the like count.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetLikeCount(Control element)
        {
            return (string)element.GetValue(LikeCountProperty);
        }

        /// <summary>
        /// Sets the like count.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetLikeCount(Control element, string value)
        {
            element.SetValue(LikeCountProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the like area pointer over value.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>A <see cref="Grid"/>.</returns>
        public static bool GetIsMouseOverArea(Control element)
        {
            return (bool)element.GetValue(IsMouseOverAreaProperty);
        }

        /// <summary>
        /// Sets the like area pointer over value.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetIsMouseOverArea(Control element, Grid value)
        {
            element.SetValue(IsMouseOverAreaProperty, value);
            UpdateData(element);
        }

        /// <summary>
        /// Gets the like bubble pointer over value.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <returns>A <see cref="Border"/>.</returns>
        public static bool GetIsMouseOverPopup(Control element)
        {
            return (bool)element.GetValue(IsMouseOverPopupProperty);
        }

        /// <summary>
        /// Sets the like bubble pointer over value.
        /// </summary>
        /// <param name="element">The <see cref="Control"/ to assign the property value to.</param>
        /// <param name="value">The new value to assign.</param>
        public static void SetIsMouseOverPopup(Control element, Border value)
        {
            element.SetValue(IsMouseOverPopupProperty, value);
            UpdateData(element);
        }

        private static void PropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is Canvas ca)
            {
                UpdateData(ca);
            }
        }

        private static void UpdateData(Control element)
        {
            if (element is Canvas canvas)
            {
                if (!string.IsNullOrEmpty(GetLikeCount(canvas)))
                {
                    if (GetIsMouseOverArea(canvas) || GetIsMouseOverPopup(canvas))
                    {
                        canvas.IsVisible = true;
                    }
                    else
                    {
                        canvas.IsVisible = false;
                    }
                }
                else
                {
                    canvas.IsVisible = false;
                }
            }
        }
    }
}
