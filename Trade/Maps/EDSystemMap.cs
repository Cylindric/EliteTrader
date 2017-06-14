using CsvHelper.Configuration;
using EliteTrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade.Maps
{
    public class EDSystemMap : CsvClassMap<EDSystem>
    {
        public EDSystemMap()
        {
            CsvHelper.TypeConversion.NullableConverter intNullableConverter = new CsvHelper.TypeConversion.NullableConverter(typeof(int?));

            Map(m => m.id).Name("id").TypeConverter(intNullableConverter);
            Map(m => m.edsm_id).Name("edsm_id").Default(0);
            Map(m => m.name).Name("name");
            Map(m => m.x).Name("x").Default(0);
            Map(m => m.y).Name("y").Default(0);
            Map(m => m.z).Name("z").Default(0);
            Map(m => m.population).Name("population").Default(0);
            Map(m => m.is_populated).Name("is_populated");
            Map(m => m.government_id).Name("government_id").Default(0);
            Map(m => m.allegiance_id).Name("allegiance_id").Default(0);
            Map(m => m.state_id).Name("state_id").Default(0);
            Map(m => m.security_id).Name("security_id").Default(0);
            Map(m => m.primary_economy_id).Name("primary_economy_id").Default(0);
            Map(m => m.power_state_id).Name("power_state_id").Default(0);
            Map(m => m.needs_permit).Name("needs_permit");
            Map(m => m.updated_at).Name("updated_at");
            Map(m => m.controlling_minor_faction_id).Default(0);
            Map(m => m.reserve_type_id).Name("reserve_type_id").Default(0);
        }
    }
}
