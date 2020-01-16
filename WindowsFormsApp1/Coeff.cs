using System;
using System.Collections.Generic;
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

        public static int operator *(Coeff c, int[] p)
        {
            var bytes = new byte[4][]
            {
                BitConverter.GetBytes(p[0]),
                BitConverter.GetBytes(p[1]),
                BitConverter.GetBytes(p[2]),
                BitConverter.GetBytes(p[3])
            };

            var r = Clamp(bytes[0][2] * c.A + bytes[1][2] * c.B + bytes[2][2] * c.C + bytes[3][2] * c.D);
            var g = Clamp(bytes[0][1] * c.A + bytes[1][1] * c.B + bytes[2][1] * c.C + bytes[3][1] * c.D);
            var b = Clamp(bytes[0][0] * c.A + bytes[1][0] * c.B + bytes[2][0] * c.C + bytes[3][0] * c.D);

            return 0xff << 24 | r << 16 | g << 8 | b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Clamp(int val)
        {
            if (val < 0)
                return 0;

            if (val >= 0x8000)
                return 0xff;

            return (byte)(val >> 7);
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
