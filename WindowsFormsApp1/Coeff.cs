using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PolyphasePreviewer
{
    class Coeff : IEquatable<Coeff>
    {
        public Coeff(short a, short b, short c, short d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public Coeff(ICollection<short> e) :
          this(e.ElementAtOrDefault(0), e.ElementAtOrDefault(1), e.ElementAtOrDefault(2), e.ElementAtOrDefault(3))
        {
        }

        public Coeff(float a, float b, float c, float d)
        {
            A = (short)(a * 128 + 0.5f);
            B = (short)(b * 128 + 0.5f);
            C = (short)(c * 128 + 0.5f);
            D = (short)(d * 128 + 0.5f);
        }

        public static int operator *(Coeff c, Color[] p)
        {
            var r = Clamp(p[0].R * c.A + p[1].R * c.B + p[2].R * c.C + p[3].R * c.D);
            var g = Clamp(p[0].G * c.A + p[1].G * c.B + p[2].G * c.C + p[3].G * c.D);
            var b = Clamp(p[0].B * c.A + p[1].B * c.B + p[2].B * c.C + p[3].B * c.D);

            return BitConverter.ToInt32(new byte[] { b, g, r, 0xff }, 0);
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
    }
}
