using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteTrader
{
    public interface ICommand
    {
        int Execute();
    }
}
