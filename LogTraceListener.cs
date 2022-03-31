using System;
using System.Diagnostics;

namespace SCW_APP
{
    /// <summary>
    /// This class reperesent argument for LogTraceListener class
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="message">Message text</param>
        public LogEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets or sets message text
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Trace listener that just fire LogWrite event
    /// </summary>
    public class LogTraceListener : TraceListener
    {
        ///<summary>
        /// Event that notify about TraceError, TraceWarning, TraceInformation and WriteLine methods
        ///</summary>
        public event EventHandler<LogEventArgs> LogWrite;

        /// <summary>
        /// Writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void Write(string message)
        {
            // Do nothing - skip proccess info stuff

            // Then we are using TraceError TraceWarning or TraceInformation methods 
            // Trace calls 2 methods Write(<process info>) and WriteLine(<our message>)
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void WriteLine(string message)
        {
            // fire event
            if (LogWrite != null)
            {
                LogWrite(this, new LogEventArgs(message));
            }
        }
    }
}
