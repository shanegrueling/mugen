namespace Mugen.Math
{
    public readonly struct int2
    {
        public readonly int X;
        public readonly int Y;

        public int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static int2 operator +(in int2 i1, in int2 i2)
        {
            return new int2(i1.X + i2.X, i1.Y + i2.Y);
        }

        /*public static int2 operator +(int2 i1, int2 i2)
        {
            return new int2(i1.X + i2.X, i1.Y + i2.Y);
        }*/

        public static int2 operator -(in int2 i1, in int2 i2)
        {
            return new int2(i1.X - i2.X, i1.Y - i2.Y);
        }

        public static float2 operator /(in int2 i1, in float divisor)
        {
            return new float2(i1.X / divisor, i1.Y / divisor);
        }

        public static float2 operator +(in int2 i, in float2 f)
        {
            return new float2(i.X + f.X, i.Y + f.Y);
        }
    }
}
