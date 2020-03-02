using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YwfSimpleConsoleAppTerminal
{
    class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                ConsoleHelper.WriteLineByColor("Ywf简单控制台应用程序终端工具-shiyousan.com",ConsoleColor.Green);
                CommandCore command = new CommandCore();
                command.Run();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineByColor(ex.Message, ConsoleColor.Red);
                ConsoleHelper.WriteLineByColor(ex.StackTrace, ConsoleColor.Yellow);
            }
        }
    }
}
