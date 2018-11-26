using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    public Coeff(IEnumerable<short> s) : 
      this(s.ElementAtOrDefault(0), s.ElementAtOrDefault(1), s.ElementAtOrDefault(2), s.ElementAtOrDefault(3))
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
        Clip(c.a * v, -1f, 1f),
        Clip(c.b * v, -1f, 1f),
        Clip(c.c * v, -1f, 1f),
        Clip(c.d * v, -1f, 1f));
    }

    public static int operator *(Coeff c, Color[] p)
    {
      byte r, g, b;

      var rr = p[0].R * c.A + p[1].R * c.B + p[2].R * c.C + p[3].R * c.D;
      var gg = p[0].G * c.A + p[1].G * c.B + p[2].G * c.C + p[3].G * c.D;
      var bb = p[0].B * c.A + p[1].B * c.B + p[2].B * c.C + p[3].B * c.D;

      if (rr < 0)
        r = 0;
      else if (rr >= 0x8000)
        r = 0xff;
      else
        r = (byte)(rr >> 7);

      if (gg < 0)
        g = 0;
      else if (gg >= 0x8000)
        g = 0xff;
      else
        g = (byte)(gg >> 7);

      if (bb < 0)
        b = 0;
      else if (bb >= 0x8000)
        b = 0xff;
      else
        b = (byte)(bb >> 7);

      return BitConverter.ToInt32(new[] { b, g, r, (byte)0xff }, 0);
    }

    public static Coeff operator +(Coeff c1, Coeff c2)
    {
      return new Coeff(
        Clip(c1.a + c2.a, -1f, 1f),
        Clip(c1.b + c2.b, -1f, 1f),
        Clip(c1.c + c2.c, -1f, 1f),
        Clip(c1.d + c2.d, -1f, 1f));
    }

    public int Sum()
    {
      return A + B + C + D;
    }

    private static float Clip(float v, float min, float max)
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
