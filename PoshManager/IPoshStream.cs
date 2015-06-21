using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshManager
{
    public interface IPoshStream
    {
        Action<String> VerboseWriter { get;  }
        Action<String> DebugWriter { get;  }
        Action<String> WarningWriter { get; }
        Action<String> ErrorWriter { get; }
    }
}
