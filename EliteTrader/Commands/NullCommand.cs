using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteTrader.Commands
{
    public class NullCommand : ICommand
    {
        public int Execute()
        {
            return 0;
        }
    }
}
