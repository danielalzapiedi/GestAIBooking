using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestAI.Domain.Common
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
