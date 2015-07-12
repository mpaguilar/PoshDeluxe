using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshManager
{
    public interface IPoshStream
    {
        void Write(PoshMessage message);
    }
}
