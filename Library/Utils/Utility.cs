using System;

namespace Library.Utils
{
    public static class Utility
    {
        public static string GetLastChars(Guid token)
        {
            return token.ToString().Substring(
                token.ToString().Length - 3);
        }
    }
}
