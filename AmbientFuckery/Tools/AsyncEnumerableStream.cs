using System;
using System.Collections.Generic;
using System.IO;

namespace AmbientFuckery.Tools
{
    public class AsyncEnumerableStream : Stream
    {
        private readonly IAsyncEnumerator<byte> enumerator;
        public long count = 0;

        public AsyncEnumerableStream(IAsyncEnumerable<byte> source)
        {
            enumerator = source.GetAsyncEnumerator();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            for (; i < count; i++)
            {
                var valueTask = enumerator.MoveNextAsync();
                var result = valueTask.IsCompleted ? valueTask.Result : valueTask.AsTask().Result;
                if (!result) break;

                buffer[offset++] = enumerator.Current;
            }

            this.count += i;
            return i;
        }

        protected override void Dispose(bool disposing)
        {
            var valueTask = enumerator.DisposeAsync();
            if (!valueTask.IsCompleted) valueTask.AsTask().Wait();

            base.Dispose(disposing);
        }

        #region boring implementations
        public override bool CanRead => true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        #endregion
    }
}
