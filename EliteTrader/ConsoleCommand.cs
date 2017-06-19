using System.Collections.Generic;

namespace EliteTrader
{
    public class ConsoleCommand
    {
        public struct Arg
        {
            public string Key;
            public string Value;
        }

        public string Name { get; set; }

        public IEnumerable<Arg> Arguments
        {
            get
            {
                return _arguments;
            }
        }

        private List<Arg> _arguments;

        public ConsoleCommand(string input)
        {
            var arguments = new List<string>();

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
                    arguments.Add(token);
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
                arguments.Add(token);
            }

            // The first token is the command name.
            Name = arguments[0];
            arguments.RemoveAt(0);

            // See if any of the arguments are name/value pairs, and if so store them in the NamedArgs.
            _arguments = new List<Arg>();
            foreach(var arg in arguments)
            {
                if (arg.Contains("="))
                {
                    var parts = arg.Split('=');
                    _arguments.Add(new Arg() { Key = parts[0], Value = parts[1] });
                }
                else
                {
                    _arguments.Add(new Arg() { Value = arg });
                }
            }
        }
    }
}
