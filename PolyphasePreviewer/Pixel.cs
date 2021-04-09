using System.Runtime.InteropServices;

namespace PolyphasePreviewer
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel
    {
        [FieldOffset(0)]
        public byte B;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(2)]
        public byte R;

        [FieldOffset(3)]
        public byte A;

        [FieldOffset(0)]
        public int Argb;
    }
}
