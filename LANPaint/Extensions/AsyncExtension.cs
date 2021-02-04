using System;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Extensions
{
    internal static class AsyncExtension
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None) return await task;
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return task.Result;
        }
    }
}
