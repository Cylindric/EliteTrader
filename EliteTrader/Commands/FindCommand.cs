using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade;

namespace EliteTrader.Commands
{
    class FindCommand : ICommand
    {
        private string _start { get; set; }
        private float _range { get; set; }
        private float _distance { get; set; }

        public FindCommand(ConsoleCommand cmd)
        {
            if (cmd.Arguments.Count() == 1)
            {
                if (cmd.Arguments.First().Value == "test")
                {
                    _start = "olgrea";
                    _range = 30;
                    _distance = 300;
                }
            }

            foreach (var opt in cmd.Arguments)
            {
                switch (opt.Key)
                {
                    case "start":
                    case "from":
                        _start = opt.Value;
                        break;

                    case "distance":
                        _distance = (float)Convert.ToDouble(opt.Value);
                        break;

                    case "jump":
                    case "jmp":
                        _range = (float)Convert.ToDouble(opt.Value);
                        break;
                }
            }
        }
        public int Execute()
        {
            var start = EDSystemManager.Instance.Find(_start);

            Console.WriteLine($"Looking for systems within {_distance:n1} Ly of {start.name}...");
            var systems = EDSystemManager.Instance.FindInRange(start, _distance);
            Console.WriteLine($"Found {systems.Count()} systems:");
            foreach(var sys in systems.OrderBy(s => s.Value.name))
            {
                Console.WriteLine($"{sys.Value.name}");
            }
            return 0;
        }
    }
}
