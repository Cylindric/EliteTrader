using EliteTrader.Models;
using RaptorDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade.Views
{
    class EDSystemView : View<EDSystem>
    {
        public class RowSchema
        {
            public string SystemName;
        }

        public EDSystemView()
        {
            this.Schema = typeof(EDSystemView.RowSchema);

            this.Mapper = (api, docid, doc) =>
            {
                api.Emit(docid, doc.name);
            };
        }
    }
}
