using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YwfSimpleConsoleAppTerminal
{
    public class CommandCore
    {
        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsRun { get; set; } = true;
        /// <summary>
        /// 任务操作类地图字典
        /// </summary>
        private IDictionary<string, Type> JobTypeMap { get; set; }

        /// <summary>
        /// 命令信息
        /// </summary>
        private IDictionary<string, string> CommandInfoList { get; set; }


        public CommandCore()
        {
            InitializeProperty();
        }

        /// <summary>
        /// 运行入口,接收输入命令
        /// </summary>
        public void Run()
        {
            ShowCommand();
            while (IsRun)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write("console> ");
                    var consoleInput = Console.ReadLine();
                    ExecuteCommand(consoleInput);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="input"></param>
        public void ExecuteCommand(string input)
        {
            if (input == "exit")
            {
                IsRun = false;
            }
            else if (input == "help")
            {
                ShowCommand();
            }
            else if (JobTypeMap.ContainsKey(input))
            {
                Type typeInfo = JobTypeMap[input];
                var instance = Assembly.GetAssembly(typeInfo).CreateInstance(typeInfo.FullName);

                ConsoleHelper.WriteLineByColor($"Starting in {DateTime.Now},command by [{input}]", ConsoleColor.DarkGray);
                typeInfo.InvokeMember("Execute"
                    , BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance
                    , null
                    , instance
                    , null);
                ConsoleHelper.WriteLineByColor($"Ending in {DateTime.Now},command by [{input}]", ConsoleColor.DarkGray);
            }
            else
            {
                PrintErrorCommandMessage();
            }
        }

        /// <summary>
        /// 显示命令操作说明
        /// </summary>
        public void ShowCommand()
        {

            if (JobTypeMap == null || JobTypeMap.Count() <= 0)
            {
                ConsoleHelper.WriteLineByColor("没有可执行的命令！！！", ConsoleColor.Red);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleHelper.DrawHalfSplitLine();
            Console.WriteLine("操作命令说明(输入命令名称以执行任务，区分大小写)：\n");

            /*
             * 使用ConsoleTable类库绘制表格
             * https://github.com/khalidabuhakmeh/ConsoleTables
             */
            ConsoleTable table = new ConsoleTable("No.", "CommandName", "Description");
            int counter = 0;
            foreach (var c in CommandInfoList)
            {
                table.AddRow(counter, c.Key, c.Value);
                counter++;
            }
            table.Write();

            ConsoleHelper.DrawHalfSplitLine();
            Console.ResetColor();
        }

        #region 内部私有方法
        /// <summary>
        /// 打印错误命令信息提示
        /// </summary>
        private void PrintErrorCommandMessage()
        {
            ConsoleHelper.WriteLineByColor("请输入正确的命令！当前命令无效！输入help查看命令说明", ConsoleColor.Red);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>        
        /// <returns></returns>
        private void InitializeProperty()
        {
            IList<Type> jobTypeList = GetJobTypeList();
            IDictionary<string, string> _commandInfoList = new Dictionary<string, string>();

            IDictionary<string, Type> _typeMap = new Dictionary<string, Type>();
            foreach (var type in jobTypeList)
            {
                JobAttribute jobAttribute = type.GetCustomAttribute<JobAttribute>(false);
                _typeMap.Add(jobAttribute.CommandName, type);
                _commandInfoList.Add(jobAttribute.CommandName, jobAttribute.Description);
            }
            _commandInfoList.Add("help", "Show operation command.");
            _commandInfoList.Add("exit", "Exit terminal loop.");

            JobTypeMap = _typeMap;
            CommandInfoList = _commandInfoList;
        }

        /// <summary>
        /// 获取操作任务类的Type集合
        /// </summary>
        /// <returns></returns>
        private IList<Type> GetJobTypeList()
        {

            string jobNamespace = ConfigurationManager.AppSettings["JobNamespace"];
            //获取当前程序集下所有Type,根据需求进行过滤            
            var queryList = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.IsClass && t.Namespace == jobNamespace
                            select t;
            var tempTypeList = queryList.ToList();
            var jopTypeList = new List<Type>();
            //获取只包含IBaseJob接口，并且有声明JobAttribute特性的类             
            foreach (var type in tempTypeList)
            {
                bool hasInterface = new List<Type>(type.GetInterfaces()).Contains(typeof(IBaseJob));
                bool hasAttribute = type.IsDefined(typeof(JobAttribute), false);

                if (hasInterface && hasAttribute)
                {
                    jopTypeList.Add(type);
                }
            }
            jopTypeList.Sort(new JobAttributeSortComparer());
            return jopTypeList;
        }

        #endregion

        private class JobAttributeSortComparer : IComparer<Type>
        {
            public int Compare(Type t1, Type t2)
            {
                var att1 = t1.GetCustomAttributes(typeof(JobAttribute), false);
                var att2 = t2.GetCustomAttributes(typeof(JobAttribute), false);
                if (att1 == null || att2 == null || att1.Count() <= 0 || att2.Count() <= 0)
                {
                    return 0;
                }
                int sort1 = ((JobAttribute)att1[0]).Sort;
                int sort2 = ((JobAttribute)att2[0]).Sort;

                if (sort1 > sort2)
                {
                    return 1;
                }
                else if (sort1 < sort2)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
