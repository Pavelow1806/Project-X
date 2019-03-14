using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    class Data
    {
        protected static Data instance;

        protected static ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public static byte[] data = null;

        public static int Index = -1;

        public Data()
        {
            instance = this;
        }

        public static void Reset(bool DontResetIndex = false)
        {
            buffer = new ByteBuffer.ByteBuffer();
            data = null;
            if (!DontResetIndex) Index = -1;
        }
    }
}
