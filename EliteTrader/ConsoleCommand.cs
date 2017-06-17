using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EliteTrader
{
    public class ConsoleCommand
    {
        public string Name { get; set; }

        public IEnumerable<string> Arguments
        {
            get
            {
                return _arguments;
            }
        }

        public IDictionary<string, string> NamedArguments
        {
            get
            {
                return _namedArgs;
            }
        }

        private List<string> _arguments;
        private Dictionary<string, string> _namedArgs;

        public ConsoleCommand(string input)
        {
            _arguments = new List<string>();
            _namedArgs = new Dictionary<string, string>();

            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            string token = string.Empty;
            bool inQuoted = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuoted = !inQuoted;
                }
                else if (inQuoted == false && c == ' ')
                {
                    // A space usually delimits tokens, unless we're in the middle of a quoted string
                    _arguments.Add(token);
                    token = string.Empty;
                }
                else
                {
                    token += c;
                }
            }

            // There will probably be a trailing token, add that too.
            if (!string.IsNullOrEmpty(token))
            {
                _arguments.Add(token);
            }

            // The first token is the command name.
            Name = _arguments[0];
            _arguments.RemoveAt(0);

            // See if any of the arguments are name/value pairs, and if so store them in the NamedArgs.
            foreach(var arg in _arguments)
            {
                if (arg.Contains("="))
                {
                    var parts = arg.Split('=');
                    _namedArgs.Add(parts[0], parts[1]);
                }
            }
        }
    }
}
