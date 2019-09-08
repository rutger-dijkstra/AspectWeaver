using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspectWeaver.Tests
{
    [TestClass]
    public class ResultInterceptorTest
    {
        class Jo: Io
        {
            public int TheGood() => 42;
            public int TheBad() => 666;
        }

        interface Io
        {
            int TheGood();
            int TheBad();
        }

        [TestMethod]
        public void InterceptorTest()
        {
            var io = (new Jo() as Io)
                .AddResultAction((int result) => { if( result == 666 ) throw new BadImageFormatException("666"); });

            Assert.AreEqual(42, io.TheGood());
            Assert.ThrowsException<BadImageFormatException>(() => { io.TheBad(); });
        }
    }
}
