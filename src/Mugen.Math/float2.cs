namespace Mugen.Math
{
    public struct float2
    {
        public float X { get; }
        public float Y { get; }

        public float2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static float2 operator +(in float2 f1, in float2 f2)
        {
            return new float2(f1.X + f2.X, f1.Y + f2.Y);
        }

        public static float2 operator -(in float2 f1, in float2 f2)
        {
            return new float2(f1.X - f2.X, f1.Y - f2.Y);
        }

        public static explicit operator int2(in float2 f)
        {
            return new int2((int)f.X, (int)f.Y);
        }
    }
}