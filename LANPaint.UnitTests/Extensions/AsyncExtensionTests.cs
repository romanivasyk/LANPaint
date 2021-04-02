using LANPaint.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LANPaint.UnitTests.Extensions
{
    public class AsyncExtensionTests
    {
        [Fact]
        public async void WithCancellationNormalComplete()
        {
            var taskToComplete = Task.FromResult(1);
            var tokenSource = new CancellationTokenSource();

            var result = await taskToComplete.WithCancellation(tokenSource.Token);

            Assert.Equal(1, result);
        }

        [Fact]
        public async void WithCancellationCancelTask()
        {
            var taskToCancel = new Task<int>(() =>
            {
                Task.Delay(1000).Wait();
                return 1;
            });
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => taskToCancel.WithCancellation(tokenSource.Token));
        }

        [Fact]
        public async void WithCancellationDefaultToken()
        {
            var taskToComplete = Task.FromResult(1);
            var result = await taskToComplete.WithCancellation(default);
            Assert.Equal(1, result);
        }
    }
}