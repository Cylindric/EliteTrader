using System;
using System.Diagnostics;
using System.Linq;
using Trade;

namespace EliteTrader.Commands
{
    class RouteCommand : ICommand
    {
        private RouteFinder _router = new RouteFinder();
        private string _start { get; set; }
        private string _end { get; set; }
        private float _range { get; set; }

        public RouteCommand(ConsoleCommand cmd)
        {
            if(cmd.Arguments.Count() == 1)
            {
                if(cmd.Arguments.First().Value == "test")
                {
                    _start = "olgrea";
                    _end = "te kaha";
                    _range = 30;
                }
            }

            foreach(var opt in cmd.Arguments)
            {
                switch (opt.Key)
                {
                    case "start":
                    case "from":
                        _start = opt.Value;
                        break;

                    case "end":
                    case "to":
                        _end = opt.Value;
                        break;

                    case "jump":
                    case "jmp":
                        _range = (float)Convert.ToDouble(opt.Value);
                        break;
                }
            }

            if (string.IsNullOrEmpty(_start))
            {
                throw new ArgumentException("Missing Start parameter!");
            }
            if (string.IsNullOrEmpty(_end))
            {
                throw new ArgumentException("Missing End parameter!");
            }
            if (_range ==  0)
            {
                throw new ArgumentException("Missing range parameter!");
            }
        }

        public int Execute()
        {
            var start = EDSystemManager.Instance.Find(_start);
            var end = EDSystemManager.Instance.Find(_end);

            var distance = Astrogation.Distance(start, end);

            Console.WriteLine($"Looking for path between {start.name} and {end.name} ({distance:n2} Ly)...");

            _router.JumpRange = _range;
            var sw = new Stopwatch();
            sw.Start();
            var route = _router.Route(start, end);
            sw.Stop();

            Console.WriteLine($"Path found, {start.name} to {end.name} in {route.Count - 1} jumps. Took {sw.Elapsed.ToString()}");
            var i = 0;
            foreach (var n in route)
            {
                Console.WriteLine($"{i:n0} {n.name}");
                i++;
            }

            return 0;
        }
    }
}
