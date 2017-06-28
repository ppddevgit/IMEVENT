using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public class GroupSharingEntry :  BaseSectionEntry
    {
        public string UserId { get; set; }
        public int Table { get; set; }
        public int Capacity { get; set; }
        public int RemainingSeats { get; set; }        
    }
}
