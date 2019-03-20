using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    class Data
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        private static Data _instance = null;
        public static Data instance
        {
            get
            {
                lock (lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new Data();
                    }
                    return _instance;
                }
            }
        }

        protected static ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public static byte[] data = null;

        public static int Index = -1;

        public static void Reset(bool DontResetIndex = false)
        {
            buffer = new ByteBuffer.ByteBuffer();
            data = null;
            if (!DontResetIndex) Index = -1;
        }
    }
}
