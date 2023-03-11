using System;
using System.IO;

namespace GroupMeClient.Core.Utilities
{
    /// <summary>
    /// A read-only adapter between <see cref="byte[]"/> and <see cref="Stream"/>. Unlike <see cref="MemoryStream"/>,
    /// the byte content is not copied, and is not mutable through the <see cref="Stream"/>. Random read and seek operations
    /// on the stream is fully supported. This allows for creating multiple <see cref="ReadOnlyByteStream"/>s on the
    /// same underlying data with minimal overhead in a thread-safe way.
    /// This class implements <see cref="IDisposable"/>, but has no unmanaged resources to dispose.
    /// </summary>
    public class ReadOnlyByteStream : Stream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyByteStream"/> class.
        /// </summary>
        /// <param name="data">The raw byte data to wrap in this <see cref="Stream"/>.</param>
        public ReadOnlyByteStream(byte[] data)
        {
            this.Data = data;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => this.Data.Length;

        /// <inheritdoc/>
        public override long Position { get; set; }

        private byte[] Data { get; }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var actualCount = count;
            if (this.Position + count > this.Length)
            {
                actualCount = (int)(this.Length - this.Position);
            }

            Array.Copy(this.Data, this.Position, buffer, offset, actualCount);

            this.Position += actualCount;
            return actualCount;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition = this.Position;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;

                case SeekOrigin.Current:
                    newPosition += offset;
                    break;

                case SeekOrigin.End:
                    newPosition = this.Length + offset;
                    break;
            }

            if (newPosition < 0 || newPosition > this.Length)
            {
                throw new ArgumentException("Invalid position", nameof(this.Seek));
            }

            this.Position = newPosition;
            return this.Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
