using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PolyphasePreviewer
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Coeff : IEquatable<Coeff>, IEnumerable<short>
    {
        public short A;
        public short B;
        public short C;
        public short D;

        public Coeff(short a, short b, short c, short d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public bool Equals(Coeff other)
        {
            return
              A == other.A &&
              B == other.B &&
              C == other.C &&
              D == other.D;
        }

        public IEnumerator<short> GetEnumerator()
        {
            yield return A;
            yield return B;
            yield return C;
            yield return D;
        }

        public int Sum()
        {
            return A + B + C + D;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
