using System;

namespace YwfSimpleConsoleAppTerminal.Job
{
    [Job(CommandName = "test", Description = "test command", Sort = 1)]
    public class TestJob : IBaseJob
    {
        public void Execute()
        {
            Console.WriteLine(" 这是一个测试任务！");
        }
    }
}
