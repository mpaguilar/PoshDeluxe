using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PoshManager
{
    public class ListWriter : IPoshStream
    {
        public void Write(PoshManager.PoshMessage message) {
            lock (PoshMessages)
            {
                PoshMessages.Add(message);
            }
        }

        public List<PoshManager.PoshMessage> PoshMessages
            = new List<PoshManager.PoshMessage>();

    }
}
