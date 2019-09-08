using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using AspectWeaver.Util;
using Test.Logging;

namespace AspectWeaver.Tests
{
    [TestClass]
    public class PrivateStringTests: PrivateStringTests.IHoHo
    {
        interface IHoHo
        {
            [return:Private]string HoHo([Private]string jo, Hoppa hoppa);
        }

        class Hoppa
        {
            public string UserName { get; set; }
            [Private]
            public string PersonalSecret { get; set; }
        }

        string IHoHo.HoHo(string jo, Hoppa hoppa) => "hmmm";

        ILoggerFactory _loggerFactory = new LoggerFactory();
        Queue<TestLogEntry> _log = new Queue<TestLogEntry>();

        public PrivateStringTests()
        {
            _loggerFactory.AddTestLogger(_log.Enqueue);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _log.Clear();
        }

        [TestMethod]
        public void TestJsonWrapper()
        {
            var obj = new Hoppa() { UserName = "me", PersonalSecret = "embarressment" };
            var serialized = JsonWrapper.Create(obj).ToString();
            Assert.AreEqual("{\"UserName\":\"me\",\"PersonalSecret\":\"***\"}", serialized);
        }

        [TestMethod]
        public void TestReflectionCaching()
        {
            var m1 = typeof(IHoHo).GetMethod("HoHo");
            var p1 = m1.GetParameters()[0];
            var m2 = typeof(IHoHo).GetMethod("HoHo");
            var p2 = m2.GetParameters()[0];

            Assert.AreSame(m1, m2);
            Assert.AreSame(p1, p2);
        }

        [TestMethod]
        public void TestCallLogWithPrivateData()
        {
            var wrapped = this.AddLoggingAspect<IHoHo>(_loggerFactory.CreateLogger<IHoHo>());
            var hmm = wrapped.HoHo("secret", new Hoppa() {
                UserName = "it is me",
                PersonalSecret = "it is personal"
            });
            Assert.AreEqual(2, _log.Count);
            var callMessage = _log.Dequeue().Message;
            var resultMessage = _log.Dequeue().Message;
            Assert.AreEqual(
                "Calling HoHo(***, {\"UserName\":\"it is me\",\"PersonalSecret\":\"***\"}).", 
                callMessage
            );
            Assert.AreEqual("HoHo returned *** in duration", resultMessage.Substring(0,29));
        }
    }
}
