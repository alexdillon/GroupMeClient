using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Caching.Models
{
    interface IDatabaseMessageWrapper
    {
        string GroupIdentifer { get; }

        string ChatIdentifier { get; }
    }
}
