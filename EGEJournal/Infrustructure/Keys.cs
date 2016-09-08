using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EGEJournal.Infrustructure
{
    public static class CKeys
    {
        public static byte[] key1
        {
            get
            {
                byte[] result = new byte[32] { 177, 85, 82, 178, 104, 232, 232, 238, 23, 30, 124, 73, 100, 25, 100, 244, 195, 198, 205, 179, 159, 106, 185, 15, 196, 152, 120, 133, 254, 217, 244, 93 };
                return result;
            }
        }

        public static byte[] key2
        {
            get
            {
                byte[] result = new byte[16] { 34, 106, 114, 112, 165, 37, 29, 194, 132, 135, 30, 60, 128, 194, 239, 106 };
                return result;
            }
        }
    }
}
