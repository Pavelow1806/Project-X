using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    class Data
    {
        protected static ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public static byte[] data = null;

        protected static int Index = -1;

        public static void Reset()
        {
            buffer = new ByteBuffer.ByteBuffer();
            data = null;
            Index = -1;
        }
    }
}
