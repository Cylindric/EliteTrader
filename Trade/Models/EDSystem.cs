using System.Diagnostics;
using Trade;

namespace EliteTrader.Models
{
    [DebuggerDisplay("{name}")]
    public class EDSystem
    {
        public int? id { get; set; }
        //public int? edsm_id { get; set; }
        public string key { get; private set; }

        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                key = _name.ToLower();
            }
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public long? population { get; set; }
        //public int? is_populated { get; set; }
        //public int? government_id { get; set; }
        ////public string government { get; set; }
        //public int? allegiance_id { get; set; }
        //public string allegiance { get; set; }
        //public int? state_id { get; set; }
        //public string state { get; set; }
        //public int? security_id { get; set; }
        //public string security { get; set; }
        //public int? primary_economy_id { get; set; }
        //public string primary_economy { get; set; }
        //public string power { get; set; }
        //public string power_state { get; set; }
        //public int? power_state_id { get; set; }
        public bool needs_permit { get; set; }
        public int updated_at { get; set; }
        //public string simbad_ref { get; set; }
        //public int? controlling_minor_faction_id { get; set; }
        //public string controlling_minor_faction { get; set; }
        //public int? reserve_type_id { get; set; }
        //public string reserve_type { get; set; }

        public BoxKey box { get; set; }
    }
}
