namespace Mugen.Math
{
    public struct float2
    {
        public float X { get; }
        public float Y { get; }

        public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y);

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

        public static float2 operator *(in float2 f, in float s)
        {
            return new float2(f.X * s, f.Y * s);
        }

        public static explicit operator int2(in float2 f)
        {
            return new int2((int)f.X, (int)f.Y);
        }

        public static float Distance(float2 f1, float2 f2) =>
            (float)System.Math.Sqrt(System.Math.Pow(f2.X - f1.X, 2) + System.Math.Pow(f2.Y - f1.Y, 2));

        public static float2 Normalize(ref float2 f)
        {
            var m = f.Magnitude;
            return new float2(f.X/m, f.Y/m);
        }

        public static float2 Normalize(float2 f)
        {
            var m = f.Magnitude;
            return new float2(f.X/m, f.Y/m);
        }
    }
}