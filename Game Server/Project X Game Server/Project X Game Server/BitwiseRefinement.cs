using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class BitwiseRefinement
    {
        public static byte BoolsToByte(bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7)
        {
            int a = 1;
            int b = 2;
            int c = 4;
            int d = 8;
            int e = 16;
            int f = 32;
            int g = 64;
            int h = 128;

            int result = 0;

            if (b0) result = result | a;
            if (b1) result = result | b;
            if (b2) result = result | c;
            if (b3) result = result | d;
            if (b4) result = result | e;
            if (b5) result = result | f;
            if (b6) result = result | g;
            if (b7) result = result | h;

            return (byte)result;
        }
        public static void ByteToBools(byte input, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
        {
            int a = 1;
            int b = 2;
            int c = 4;
            int d = 8;
            int e = 16;
            int f = 32;
            int g = 64;
            int h = 128;

            int trojan = (int)input;
            int op = 0;

            op = trojan & a;
            if (op == a) b0 = true; else b0 = false;
            op = trojan & b;
            if (op == b) b1 = true; else b1 = false;
            op = trojan & c;
            if (op == c) b2 = true; else b2 = false;
            op = trojan & d;
            if (op == d) b3 = true; else b3 = false;
            op = trojan & e;
            if (op == e) b4 = true; else b4 = false;
            op = trojan & f;
            if (op == f) b5 = true; else b5 = false;
            op = trojan & g;
            if (op == g) b6 = true; else b6 = false;
            op = trojan & h;
            if (op == h) b7 = true; else b7 = false;
        }
    }
}
