using Microsoft.Extensions.Logging;
using System;

namespace Test.Logging
{

    /// <summary>TestLogEntry</summary>
    public class TestLogEntry {

        /// <summary>LogLevel</summary>
        public LogLevel LogLevel { get; internal set; }

        /// <summary>EventId</summary>
        public EventId EventId { get; internal set; }

        /// <summary>Exception</summary>
        public Exception Exception { get; internal set; }

        /// <summary>Message</summary>
        public string Message { get; internal set; }
    }
}
