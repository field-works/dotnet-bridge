using System;

namespace FieldWorks.FieldReports
{
    /**
    * .NET Bridgeで発生する例外
    * 
    */
    public class ReportsException : Exception
    {
        public ReportsException(string message, Exception cause)
        : base(message, cause)
        {
        }

        public ReportsException(string message)
        : base(message)
        {
        }
    }
}