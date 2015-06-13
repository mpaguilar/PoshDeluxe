using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshManager
{
    public interface IPoshStream
    {
        Action<String> VerboseWriter { get; set; }
        Action<String> DebugWriter { get; set; }
        Action<String> WarningWriter { get; set; }
        Action<String> ErrorWriter { get; set; }
    }
}
