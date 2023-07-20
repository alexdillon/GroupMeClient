using System;
using Avalonia.Data.Converters;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// Provides a set of useful <see cref="IValueConverter"/>s for working with numeric values.
    /// </summary>
    public static class NumericConverters
    {
        /// <summary>
        /// A value converter that returns <c>true</c> if the input value is <c>1</c>, or <c>false</c> otherwise.
        /// </summary>
        public static readonly IValueConverter IsOneConverter =
            new FuncValueConverter<int?, bool>(i => i == 1);

        /// <summary>
        /// A value converter that returns <c>false</c> if the input value is <c>1</c>, or <c>true</c> otherwise.
        /// </summary>
        public static readonly IValueConverter IsNotOneConverter =
            new FuncValueConverter<int, bool>(i => i != 1);

        /// <summary>
        /// A value converter that the input <see cref="int"/> value, incremented by <c>1</c>.
        /// </summary>
        public static readonly IValueConverter PlusOneConverter =
            new FuncValueConverter<int, int>(i => i + 1);
    }
}
