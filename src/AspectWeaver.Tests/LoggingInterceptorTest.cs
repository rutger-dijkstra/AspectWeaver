using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Test.Logging;

namespace AspectWeaver.Tests
{
    [TestClass]
    public class LoggingInterceptorTest {

        interface ITheInterface {
            void DoSomething(int fst, string scnd = "theDefault");
            Task DoSomethingAsync();
            Task<bool> FailGetSomethingAsync(int arg);
            void FailSomething(object arg);
            Task FailSomethingAsync();
            bool GetSomething(int arg);
            Task<bool> GetSomethingAsync(int arg);
        }

        class TheImplementation: ITheInterface {
            public void DoSomething(int fst, string scnd = "theDefault") { }

            public void FailSomething(object arg) {
                throw new FormatException("o o o");
            }

            public bool GetSomething(int arg) => arg == 42;

            public Task DoSomethingAsync() =>
                Task.Delay(2);

            public Task FailSomethingAsync() =>
               Task.Delay(5).ContinueWith(t => { throw new InvalidOperationException("i i i"); });

            public Task<bool> GetSomethingAsync(int arg) =>
              Task.Delay(5).ContinueWith(t => arg == 42);

            public Task<bool> FailGetSomethingAsync(int arg) =>
              Task.Delay(5).ContinueWith(t => (bool?)null ?? throw new InvalidOperationException("a a a"));
        }

        ILoggerFactory _loggerFactory = new LoggerFactory();
        Queue<TestLogEntry> _log = new Queue<TestLogEntry>();

        ITheInterface _decorated;

        public LoggingInterceptorTest() {
            _loggerFactory.AddTestLogger(_log.Enqueue);
        }

        private ITheInterface CreateDecoratedImplementation() => 
            (new TheImplementation() as ITheInterface)
            .AddLoggingAspect(
                _loggerFactory.CreateLogger<ITheInterface>(),
                new LoggingAspectConfiguration(logLevelOnError: LogLevel.Warning, includeExceptions: true)
             );

        private ITheInterface CreateErrorOnlyImplementation() =>
            (new TheImplementation() as ITheInterface)
            .AddLoggingAspect(
                _loggerFactory.CreateLogger<ITheInterface>(),
                new LoggingAspectConfiguration(
                    logLevelBefore: LogLevel.None, 
                    logLevelOnCompletion: LogLevel.None, 
                    logLevelOnError: LogLevel.Error
                )
             );

        [TestInitialize]
        public void TestInitialize() {
            _log.Clear();
            _decorated = CreateDecoratedImplementation();
        }

        private void AssertStartsWith(string expected, string actual)
        {
            if(expected.Length < (actual?.Length ?? 0))
            {
                actual = actual.Substring(0, expected.Length);
            }
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DoSomethingTest() {
            _decorated.DoSomething(5);
            Assert.AreEqual(2, _log.Count);
            var entry = _log.Dequeue();
            Assert.AreEqual(LogLevel.Information, entry.LogLevel);
            Assert.AreEqual("Calling DoSomething(5, theDefault).", entry.Message );
            AssertStartsWith("DoSomething completed in duration", _log.Dequeue().Message);
            _decorated.DoSomething(5, null);
            Assert.AreEqual("Calling DoSomething(5, (null)).", _log.Dequeue().Message);
        }

        [TestMethod]
        public void GetSomethingTest() {
            var result = _decorated.GetSomething(8);
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling GetSomething(8).", _log.Dequeue().Message);
            AssertStartsWith($"GetSomething returned false in duration", _log.Dequeue().Message);
        }

        [TestMethod]
        public void FailSomethingTest() {
            Exception exception = null;
            try {
                _decorated.FailSomething(new { aap = "noot" });
            } catch( Exception e) {
                exception = e;
            }
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling FailSomething({\"aap\":\"noot\"}).", _log.Dequeue().Message);
            var testLogEntry = _log.Dequeue();
            Assert.AreEqual(LogLevel.Warning, testLogEntry.LogLevel);
            Assert.AreEqual("Exception FormatException: o o o from FailSomething({\"aap\":\"noot\"}).", testLogEntry.Message);
        }

        [TestMethod]
        public async Task DoSomethingAsyncTest() {
            await _decorated.DoSomethingAsync();
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling DoSomethingAsync().", _log.Dequeue().Message);
            AssertStartsWith("DoSomethingAsync completed in duration", _log.Dequeue().Message);
        }

        [TestMethod]
        public async Task DoSomethingLogOnlyErrorAsyncTest()
        {
            var decorated = CreateErrorOnlyImplementation();
            await decorated.DoSomethingAsync();
            Assert.AreEqual(0, _log.Count);
        }

        [TestMethod]
        public async Task FailSomethingLogOnlyErrorAsyncTest()
        {
            var decorated = CreateErrorOnlyImplementation();
            try
            {
                await decorated.FailSomethingAsync();
            }
            catch { }
            Assert.AreEqual(1, _log.Count);
            Assert.IsNotNull(_log.Dequeue().Exception);
        }

        [TestMethod]
        public async Task FailSomethingAsyncTest() {
            Exception exception = null;
            try {
                await _decorated.FailSomethingAsync();
            } catch( Exception e ) {
                exception = e;
            }
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling FailSomethingAsync().", _log.Dequeue().Message);
            Assert.AreEqual("Exception InvalidOperationException: i i i from FailSomethingAsync().", _log.Dequeue().Message);
        }

        [TestMethod]
        public async Task GetSomethingAsyncTest() {
            var result = await _decorated.GetSomethingAsync(183);
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling GetSomethingAsync(183).", _log.Dequeue().Message);
            AssertStartsWith($"GetSomethingAsync returned false in duration", _log.Dequeue().Message);
        }

        [TestMethod]
        public async Task FailGetSomethingAsyncTest() {
            Exception exception = null;
            try {
                await _decorated.FailGetSomethingAsync(87);
            } catch( Exception e ) {
                exception = e;
            }
            Assert.AreEqual(2, _log.Count);
            Assert.AreEqual("Calling FailGetSomethingAsync(87).", _log.Dequeue().Message);
            var lastEntry = _log.Dequeue();
            Assert.AreEqual(LogLevel.Warning,lastEntry.LogLevel);
            Assert.IsNotNull(lastEntry.Exception);
            Assert.AreEqual("Exception InvalidOperationException: a a a from FailGetSomethingAsync(87).", lastEntry.Message);
        }
    }
}
