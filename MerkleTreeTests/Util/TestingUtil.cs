using System;

namespace TestingUtil
{
    public static class TestingExtensions
    {
        public static string AsAlphaChar(this int i)
        {
            if (i < 10)
                return i.ToString();
            if (i < 36)
                return ((char)('A' + i - 10)).ToString();
            //if (i < 62)
                return ((char)('a' + i - 36)).ToString();
            //return "_";
        }
    }
}