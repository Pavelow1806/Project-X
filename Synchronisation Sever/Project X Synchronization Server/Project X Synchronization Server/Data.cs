using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
{
    public enum CachedTables
    {
        tbl_Characters
    }
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

    class Table
    {
        private Dictionary<string, List<Type>> _table = new Dictionary<string, List<Type>>();
        public Dictionary<string, List<Type>> table
        {
            set
            {
                _table = value;
            }
        }
        public List<string> GetFieldNames()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, List<Type>> col in _table)
            {
                result.Add(col.Key);
            }
            return result;
        }
        public int GetInt
    }
}
