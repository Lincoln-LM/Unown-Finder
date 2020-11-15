using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unown
{
    internal class PokeRNGR
    {
        public uint seed;

        public PokeRNGR(uint seed)
        {
            this.seed = seed;
        }

        public uint nextUInt()
        {
            this.seed = this.seed * 0xEEB9EB65 + 0xA3561A1;
            return this.seed;
        }
        public uint nextUShort()
        {
            return this.nextUInt() >> 16;
        }
    }
}
