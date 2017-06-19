using EliteTrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade;

namespace EliteTrader.Commands
{
    class MultiRouteCommand : ICommand
    {
        private string _start { get; set; }
        private List<List<string>> _routes;
        private float _range { get; set; }

        /*
         * multiroute jump=30 start=olgrea to="Te Kaha,Cao Junga" to="Carnsan,MCC 105"
         */

        public MultiRouteCommand(ConsoleCommand cmd)
        {
            _routes = new List<List<string>>();

            if (cmd.Arguments.Count() == 1)
            {
                if (cmd.Arguments.First().Value == "test")
                {
                    _start = "olgrea";
                    var r1 = "te kaha,cao junga".Split(',').ToList();
                    var r2 = "carnsan,mcc 105".Split(',').ToList();
                    _routes.Add(r1);
                    _routes.Add(r2);
                    _range = 30;
                }
            }
        }

        public int Execute()
        {
            var start = EDSystemManager.Instance.Find(_start);

            /* Systems that connect
             * 
             * Olgrea -> Te Kaha -> Cao Junga -> Carnsan -> MCC 105
             * Olgrea -> Carnsan -> MCC 105 -> Te Kaha -> Cao Junga
             */

            // Build a list of routes with actual systems in them.
            var routes = new List<Queue<EDSystem>>();
            foreach(var route in _routes)
            {
                var r = new Queue<EDSystem>();
                foreach(var sys in route)
                {
                    r.Enqueue(EDSystemManager.Instance.Find(sys));
                }
                routes.Add(r);
            }

            // Pick the starting route based on closest-first-stop
            var router = new RouteFinder();
            router.JumpRange = _range;

            Queue<EDSystem> first;
            int distance = int.MaxValue;
            foreach(var route in routes)
            {
                var p = router.Route(start, route.Peek());
                if(p.Count() < distance)
                {
                    distance = p.Count();
                    first = route;
                }
            }

            throw new NotImplementedException();
        }

        private void UpdateMap(EDSystem start, List<Queue<EDSystem>> routes)
        {
            var nodes = new List<Node>();

            var startNode = new Node();
            startNode.System = start;
        }

        private void UpdateConnections(Node node, List<Queue<EDSystem>> routes)
        {
            // create a copy of the queues
            var newRoutes = new List<Queue<EDSystem>>();
            foreach(var r in routes)
            {
                var q = new Queue<EDSystem>();
                while (r.Count() > 0)
                {
                    q.Enqueue(r.Dequeue());
                }
                newRoutes.Add(q);
            }

            foreach(var route in newRoutes)
            {
                var nextStop = route.Dequeue();
                node.ConnectedSystems.Add(nextStop, 0);
            }
        }

        private class Node
        {
            public EDSystem System { get; set; }
            public Dictionary<EDSystem, int> ConnectedSystems { get; set; }

            public Node()
            {
                ConnectedSystems = new Dictionary<EDSystem, int>();
            }
        }
    }

}
