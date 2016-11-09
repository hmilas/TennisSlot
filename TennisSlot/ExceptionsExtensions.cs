using System;

namespace TennisSlot
{
    public static class ExceptionsExtensions
    {
        public static string GetFormatted(this Exception e)
        {
            return String.Format("Exception: {0}\nStackTrace: {1}\nInnerException: {2}", e, e.StackTrace, e.InnerException);
        }
    }
}