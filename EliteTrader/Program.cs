using EliteTrader.Commands;
using System;
using System.IO;
using Trade;

namespace EliteTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            EDSystemManager.Instance.DataPath = Path.Combine(Properties.Settings.Default.DataPath, "systems.csv");
            EDSystemManager.Instance.RecentDataPath = Path.Combine(Properties.Settings.Default.DataPath, "systems_recently.csv");
            EDSystemManager.Instance.Update();

            var retval = 0;

            while (retval <= 0)
            {
                try
                {
                    Console.Write("> ");
                    var cmd = new ConsoleCommand(Console.ReadLine());
                    try
                    {
                        retval = Execute(cmd);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error executing command. {e.Message}");
                        retval = 0;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Invalid Command. {e.Message}");
                    retval = 0;
                }
            }
        }

        static int Execute(ConsoleCommand cmd)
        {
            ICommand command = new NullCommand();

            switch (cmd.Name)
            {
                case null:
                    return 0;

                case "exit":
                    command = new ExitCommand();
                    break;

                case "route":
                    command = new RouteCommand(cmd);
                    break;

                default:
                    Console.WriteLine("Unrecognised command.");
                    return 0;
            }

            return command.Execute();
        }

    }
}
