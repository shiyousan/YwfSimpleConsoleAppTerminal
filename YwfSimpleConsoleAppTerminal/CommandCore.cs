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
        private IList<string> CommandList { get; set; }

        public CommandCore()
        {
            InitializeProperty();
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

                ConsoleHelper.WriteLineByColor(string.Format("Starting in {0},{1}", DateTime.Now, typeInfo.Name), ConsoleColor.Green);
                typeInfo.InvokeMember("Execute"
                    , BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance
                    , null
                    , instance
                    , null);
                ConsoleHelper.WriteLineByColor(string.Format("Ending in {0},{1}", DateTime.Now, typeInfo.Name), ConsoleColor.Green);
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

            if (CommandList == null || CommandList.Count() <= 0)
            {
                ConsoleHelper.WriteLineByColor("没有可执行的命令！！！", ConsoleColor.Red);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleHelper.DrawHalfSplitLine();
            Console.WriteLine("操作命令说明(输入命令名称以执行任务)：\n");
            Console.WriteLine($"序号\t|\t命令名称\t|\t描述\t");
            ConsoleHelper.DrawHalfSplitLine();
            foreach (var c in CommandList)
            {
                Console.WriteLine(c);
            }
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
            IList<string> _commandList = new List<string>();

            int counter = 1;
            IDictionary<string, Type> _typeMap = new Dictionary<string, Type>();
            foreach (var type in jobTypeList)
            {
                JobAttribute jobAttribute = type.GetCustomAttribute<JobAttribute>(false);
                _typeMap.Add(jobAttribute.CommandName, type);
                //获取操作类说明
                string counterString = $"[{counter}]";                
                //添加操作命令说明列表
                var commandDescription = $"[{counter}]\t|\t{jobAttribute.CommandName.PadRight(16)}|\t{jobAttribute.Description}";
                _commandList.Add(commandDescription);
                counter++;
            }            
            string tempCommand = "\t|\thelp\t\t|\t打印操作指令";
            _commandList.Add(tempCommand);
            tempCommand = "\t|\texit\t\t|\t退出终端循环";
            _commandList.Add(tempCommand);

            this.JobTypeMap = _typeMap;
            this.CommandList = _commandList;
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
