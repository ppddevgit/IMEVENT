using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class BaseFreeItem
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        public int ResourceId { get; set; }
        public int PlaceNbr { get; set; }
    }
}
