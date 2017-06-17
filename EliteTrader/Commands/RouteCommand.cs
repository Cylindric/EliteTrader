using System;
using System.Diagnostics;
using Trade;

namespace EliteTrader.Commands
{
    class RouteCommand : ICommand
    {
        private RouteFinder router = new RouteFinder();

        private string start { get; set; }
        private string end { get; set; }
        private float range { get; set; }

        public RouteCommand(ConsoleCommand cmd)
        {
            foreach(var opt in cmd.NamedArguments)
            {
                switch (opt.Key)
                {
                    case "start":
                    case "from":
                        start = opt.Value;
                        break;

                    case "end":
                    case "to":
                        end = opt.Value;
                        break;

                    case "jump":
                    case "jmp":
                        range = (float)Convert.ToDouble(opt.Value);
                        break;
                }
            }

            if (string.IsNullOrEmpty(start))
            {
                throw new ArgumentException("Missing Start parameter!");
            }
            if (string.IsNullOrEmpty(end))
            {
                throw new ArgumentException("Missing End parameter!");
            }
            if (range ==  0)
            {
                throw new ArgumentException("Missing range parameter!");
            }
        }

        public int Execute()
        {
            router.JumpRange = range;
            var sw = new Stopwatch();
            sw.Start();
            var route = router.Route(start, end);
            sw.Stop();

            Console.WriteLine($"Path found, {start} to {end} in {route.Count - 1} jumps. Took {sw.Elapsed.ToString()}");
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
