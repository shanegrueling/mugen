namespace StructList
{
    using System;

    public static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("index", "The index was to big. Current count:{count}");
        }
    }
}