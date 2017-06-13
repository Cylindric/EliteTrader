using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade.Models
{
    class Settings
    {
        [Key]
        public int Id { get; set; }

        public DateTime? LastStationUpdate { get; set; }
        public DateTime? LastSystemUpdate { get; set; }
    }
}
