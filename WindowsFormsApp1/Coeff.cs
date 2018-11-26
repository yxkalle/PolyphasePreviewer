using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WindowsFormsApp1
{
  class Coeff : IEquatable<Coeff>
  {
    public Coeff(short a, short b, short c, short d)
    {
      A = a;
      B = b;
      C = c;
      D = d;

      this.a = A / 128f;
      this.b = B / 128f;
      this.c = C / 128f;
      this.d = D / 128f;
    }

    public Coeff(IEnumerable<short> e) : 
      this(e.ElementAtOrDefault(0), e.ElementAtOrDefault(1), e.ElementAtOrDefault(2), e.ElementAtOrDefault(3))
    {
    }

    public Coeff(float a, float b, float c, float d)
    {
      A = (short)(a * 128 + 0.5f);
      B = (short)(b * 128 + 0.5f);
      C = (short)(c * 128 + 0.5f);
      D = (short)(d * 128 + 0.5f);

      this.a = a;
      this.b = b;
      this.c = c;
      this.d = d;
    }

    public static Coeff operator *(Coeff c, float v)
    {
      return new Coeff(
        Clamp(c.a * v, -1f, 1f),
        Clamp(c.b * v, -1f, 1f),
        Clamp(c.c * v, -1f, 1f),
        Clamp(c.d * v, -1f, 1f));
    }

    public static int operator *(Coeff c, Color[] p)
    {
      var r = Clamp(p[0].R * c.A + p[1].R * c.B + p[2].R * c.C + p[3].R * c.D);
      var g = Clamp(p[0].G * c.A + p[1].G * c.B + p[2].G * c.C + p[3].G * c.D);
      var b = Clamp(p[0].B * c.A + p[1].B * c.B + p[2].B * c.C + p[3].B * c.D);

      return BitConverter.ToInt32(new byte[] { b, g, r, 0xff }, 0);
    }

    public static Coeff operator +(Coeff c1, Coeff c2)
    {
      return new Coeff(
        Clamp(c1.a + c2.a, -1f, 1f),
        Clamp(c1.b + c2.b, -1f, 1f),
        Clamp(c1.c + c2.c, -1f, 1f),
        Clamp(c1.d + c2.d, -1f, 1f));
    }

    public int Sum()
    {
      return A + B + C + D;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Clamp(int v)
    {
      if (v < 0)
        return 0;

      if (v >= 0x8000)
        return 0xff;

      return (byte)(v >> 7);
    }

    private static float Clamp(float v, float min, float max)
    {
      return Min(Max(v, min), max);
    }

    private static float Min(float v1, float v2)
    {
      return v1 < v2 ? v1 : v2;
    }

    private static float Max(float v1, float v2)
    {
      return v1 > v2 ? v1 : v2;
    }

    public bool Equals(Coeff other)
    {
      if (other == null)
        return false;

      return 
        A == other.A && 
        B == other.B && 
        C == other.C && 
        D == other.D;
    }

    public short A;
    public short B;
    public short C;
    public short D;

    private float a;
    private float b;
    private float c;
    private float d;
  }
}
