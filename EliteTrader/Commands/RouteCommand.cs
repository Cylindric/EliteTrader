using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using Trade;

namespace EliteTrader.Commands
{
    class RouteCommand : ICommand
    {
        private RouteFinder router = new RouteFinder();

        private string start { get; set; }
        private string end { get; set; }
        private float range { get; set; }

        public RouteCommand(IEnumerable<string> args)
        {
            if(args.Count() < 3)
            {
                throw new ArgumentException();
            }

            start = args.Skip(0).Take(1).First();
            end = args.Skip(1).Take(1).First();
            range = Convert.ToInt32(args.Skip(2).Take(1).First());
        }


        public int Execute()
        {
            router.JumpRange = range;
            var route = router.Route(start, end);

            Console.WriteLine($"Path found, {start} to {end} in {route.Count - 1} jumps.");
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
