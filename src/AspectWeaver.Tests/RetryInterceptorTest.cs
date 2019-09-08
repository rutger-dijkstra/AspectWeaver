using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TLN.Platform.GeneralNonsense;

namespace AspectWeaver.Tests
{
    [TestClass]
    public class RetryInterceptorTest
    {
        IZoZo CreateZoZo(params int[] retries)
        {
            var retryStrategy = new RetryStrategy(
                retries.Select(_ => TimeSpan.FromMilliseconds(_))
            ).Handle<IndexOutOfRangeException>(e => e.Message == "not tried often enough");
            return CreateZoZo(retryStrategy);
        }

        private static IZoZo CreateZoZo(IRetryStrategy strategy) =>
           new ZoZo().AddRetryAspect<IZoZo>(strategy);

        [TestMethod]
        public void HopTest1Retry()
        {
            Assert.ThrowsException<IndexOutOfRangeException>(
                () => CreateZoZo(1).Hop()
            );
        }

        [TestMethod]
        public void HopTest2Retries()
        {
            var strategy = new RetryStrategy(0, 0);
            Assert.AreEqual(666, CreateZoZo(strategy).Hop());
        }

        [TestMethod]
        public void HopTest2RetriesWrongExceptionType()
        {
            var strategy = new RetryStrategy(0, 0).Handle<InvalidCastException>();
            Assert.ThrowsException<IndexOutOfRangeException>(
                () => CreateZoZo(strategy).Hop()
            );
        }

        [TestMethod]
        public void HopTest2RetriesWrongExceptionMessage()
        {
            var strategy = new RetryStrategy(0, 0).Handle<IndexOutOfRangeException>(e => e.Message == "not this");
            Assert.ThrowsException<IndexOutOfRangeException>(
                () => CreateZoZo(strategy).Hop()
            );
        }

        [TestMethod]
        public void HopTest2RetriesWithActualDelay()
        {
            var strategy = new RetryStrategy(50, 100);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Assert.AreEqual(666, CreateZoZo(strategy).Hop());
            stopWatch.Stop();
            Assert.IsTrue(150 <= stopWatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task AsyncTest2Retries()
        {
            Assert.AreEqual(667, await CreateZoZo(2, 50).Bof());
        }

        [TestMethod]
        public async Task AsyncTest2RetriesWithActualDelay()
        {
            var strategy = new RetryStrategy(50, 100, 300);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Assert.AreEqual(667, await CreateZoZo(strategy).Bof());
            stopWatch.Stop();
            Assert.IsTrue(150 <= stopWatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task AsyncVoidTest2OrMoreRetries()
        {
            for( var retries = 2; retries < 5; retries++ )
            {
                var delays = Enumerable.Repeat(20, retries).ToArray();
                try
                {
                    await CreateZoZo(delays).LaLa();
                }
                catch( Exception e )
                {
                    Assert.Fail(e.ToString());
                }
            }
        }

        [TestMethod]
        public async Task AsyncVoidTest1Retry()
        {
            Assert.AreEqual(TimeSpan.Zero, default(TimeSpan));
            await Assert.ThrowsExceptionAsync<IndexOutOfRangeException>(CreateZoZo(1).LaLa);

        }
    }
}
