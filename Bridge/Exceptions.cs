using System;

namespace FieldWorks.FieldReports
{
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