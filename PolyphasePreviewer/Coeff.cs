using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PolyphasePreviewer
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Coeff : IEquatable<Coeff>, IEnumerable<int>
    {
        public int A;
        public int B;
        public int C;
        public int D;

        public Coeff(int a, int b, int c, int d)
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

        public IEnumerator<int> GetEnumerator()
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
