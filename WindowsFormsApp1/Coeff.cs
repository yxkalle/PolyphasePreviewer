using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PolyphasePreviewer
{
    internal class Coeff : IEquatable<Coeff>
    {
        public short A;
        public short B;
        public short C;
        public short D;

        public Coeff() : 
            this(0, 0, 0, 0)
        {
        }

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

        public static int operator *(Coeff c, Color[] p)
        {
            if (c == null)
                return 0;

            var r = Clamp(p[0].R * c.A + p[1].R * c.B + p[2].R * c.C + p[3].R * c.D);
            var g = Clamp(p[0].G * c.A + p[1].G * c.B + p[2].G * c.C + p[3].G * c.D);
            var b = Clamp(p[0].B * c.A + p[1].B * c.B + p[2].B * c.C + p[3].B * c.D);

            return BitConverter.ToInt32(new[] { b, g, r, (byte)0xff }, 0);
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

        public int Sum()
        {
            return A + B + C + D;
        }
    }
}
