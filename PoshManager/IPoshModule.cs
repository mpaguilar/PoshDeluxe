using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshManager
{
    public interface IPoshModule
    {
        Task Refresh(IPoshStream stream);
    }
}
