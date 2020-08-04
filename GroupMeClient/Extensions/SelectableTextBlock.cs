using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GroupMeClient.Wpf.Extensions
{
    /// <summary>
    /// <see cref="SelectableTextBlock"/> provides a TextBlock control that supports native Windows selection and copy operations.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/45627524.
    /// </remarks>
    public class SelectableTextBlock : TextBlock
    {
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata((object)null));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableTextBlock"/> class.
        /// </summary>
        public SelectableTextBlock()
        {
            TextEditorWrapper.CreateFor(this);
        }

        private class TextEditorWrapper
        {
            private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

            private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");

            private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

            private readonly object editor;

            public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
            {
                this.editor = Activator.CreateInstance(
                    TextEditorType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                    null,
                    new[] { textContainer, uiScope, isUndoEnabled },
                    null);
            }

            public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
            {
                RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
            }

            public static TextEditorWrapper CreateFor(TextBlock tb)
            {
                var textContainer = TextContainerProp.GetValue(tb);

                var editor = new TextEditorWrapper(textContainer, tb, false);
                IsReadOnlyProp.SetValue(editor.editor, true);
                TextViewProp.SetValue(editor.editor, TextContainerTextViewProp.GetValue(textContainer));

                return editor;
            }
        }
    }
}
