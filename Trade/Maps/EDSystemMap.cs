using CsvHelper.Configuration;
using EliteTrader.Models;

namespace Trade.Maps
{
    class EDSystemMap : CsvClassMap<EDSystem>
    {
        public EDSystemMap()
        {
            CsvHelper.TypeConversion.NullableConverter intNullableConverter = new CsvHelper.TypeConversion.NullableConverter(typeof(int?));

            Map(m => m.id).Name("id").TypeConverter(intNullableConverter);
            Map(m => m.name).Name("name");
            Map(m => m.x).Name("x").Default(0);
            Map(m => m.y).Name("y").Default(0);
            Map(m => m.z).Name("z").Default(0);
            Map(m => m.population).Name("population").TypeConverter(intNullableConverter);
            Map(m => m.needs_permit).Name("needs_permit").Default(false);
            Map(m => m.updated_at).Name("updated_at").Default(0);
        }
    }
}
