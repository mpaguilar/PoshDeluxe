using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshManager
{
    public class PoshMessage
    {
        public MessageType Type { get; set; }
        public String Message { get; set; }

        public enum MessageType
        {
            Verbose,
            Debug,
            Warning,
            Error
        }
    }
}
