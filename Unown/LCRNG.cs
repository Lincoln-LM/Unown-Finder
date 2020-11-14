using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unown
{
    internal class LCRNG
    {
        public uint seed;

        public LCRNG(uint seed)
        {
            this.seed = seed;
        }

        public uint nextUInt()
        {
            this.seed = this.seed * 0x41c64e6d + 0x6073;
            return this.seed;
        }
        public uint nextUShort()
        {
            return this.nextUInt() >> 16;
        }
    }
}


