using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unown
{
    internal class LCRNG
    {
        public int seed;

        public LCRNG(int seed)
        {
            this.seed = seed;
        }

        public int nextUInt()
        {
            this.seed = (int)((this.seed * 0x41c64e6d + 0x6073) & 0xffffffff);
            return this.seed;
        }
        public int nextUShort()
        {
            return (int)((uint)this.nextUInt() >> 16);
        }
    }
}


