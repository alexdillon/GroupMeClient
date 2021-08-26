using System;
using System.IO;
using System.Windows.Documents;
using System.Xaml;
using Markdig;

namespace GroupMeClient.WpfUI.Markdown
{
    /// <summary>
    /// Helper class for converting markdown text into WPF <see cref="FlowDocument"/> and XAML elements with GMDC styling.
    /// </summary>
    /// <remarks>
    /// Adapted from https://raw.githubusercontent.com/neolithos/NeoMarkdigXaml/master/NeoMarkdigXaml/MarkdownXaml.cs.
    /// </remarks>
    internal static class GMDCMarkdown
    {
        /// <summary>Converts a Markdown string to a FlowDocument.</summary>
        /// <param name="markdown">A Markdown text.</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <param name="baseUri">Base uri for images and links.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="System.ArgumentNullException">if markdown variable is null.</exception>
        public static FlowDocument ToFlowDocument(string markdown, MarkdownPipeline pipeline = null, Uri baseUri = null)
        {
            if (markdown == null)
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            if (pipeline == null)
            {
                pipeline = new MarkdownPipelineBuilder().Build();
            }

            using (var writer = new XamlObjectWriter(System.Windows.Markup.XamlReader.GetWpfSchemaContext()))
            {
                return (FlowDocument)ToXaml(markdown, writer, pipeline, baseUri);
            }
        }

        /// <summary>Converts a Markdown string to XAML.</summary>
        /// <param name="markdown">A Markdown text.</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <param name="baseUri">Base uri for images and links.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="ArgumentNullException">if markdown variable is null.</exception>
        public static string ToXaml(string markdown, MarkdownPipeline pipeline = null, Uri baseUri = null)
        {
            if (markdown == null)
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            using (var writer = new StringWriter())
            {
                ToXaml(markdown, writer, pipeline, baseUri);
                return writer.ToString();
            }
        } // func ToXaml

        /// <summary>Converts a Markdown string to XAML and output to the specified writer.</summary>
        /// <param name="markdown">A Markdown text.</param>
        /// <param name="writer">The destination <see cref="TextWriter"/> that will receive the result of the conversion.</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <param name="baseUri">Base uri for images and links.</param>
        public static void ToXaml(string markdown, TextWriter writer, MarkdownPipeline pipeline = null, Uri baseUri = null)
        {
            if (markdown == null)
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            using (var xamlWriter = new XamlXmlWriter(writer, System.Windows.Markup.XamlReader.GetWpfSchemaContext(), new XamlXmlWriterSettings() { CloseOutput = false }))
            {
                ToXaml(markdown, xamlWriter, pipeline, baseUri);
                xamlWriter.Flush();
            }
        }

        /// <summary>Converts a Markdown string to XAML and output to the specified writer.</summary>
        /// <param name="markdown">A Markdown text.</param>
        /// <param name="writer">The destination <see cref="TextWriter"/> that will receive the result of the conversion.</param>
        /// <param name="pipeline">The pipeline used for the conversion.</param>
        /// <param name="baseUri">Base uri for images and links.</param>
        /// <returns>Rendered XAML.</returns>
        public static object ToXaml(string markdown, XamlWriter writer, MarkdownPipeline pipeline = null, Uri baseUri = null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            pipeline = pipeline ?? new MarkdownPipelineBuilder().Build();

            var renderer = new GMDCXamlMarkdownWriter(writer) { BaseUri = baseUri };
            pipeline.Setup(renderer);
            var document = global::Markdig.Markdown.Parse(markdown, pipeline);
            return renderer.Render(document);
        }
    }
}
