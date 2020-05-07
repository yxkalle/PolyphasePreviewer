using System.Runtime.InteropServices;

namespace PolyphasePreviewer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public Pixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 0xff;
        }
    }
}
