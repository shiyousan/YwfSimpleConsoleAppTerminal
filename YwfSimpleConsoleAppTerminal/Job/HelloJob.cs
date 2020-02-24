using System;

namespace YwfSimpleConsoleAppTerminal.Job
{
    [Job(CommandName = "hello", Description = "Output Hello World", Sort = 2)]
    public class HelloJob : IBaseJob
    {
        public void Execute()
        {
            Console.WriteLine("Hello World!!!");
        }
    }
}
