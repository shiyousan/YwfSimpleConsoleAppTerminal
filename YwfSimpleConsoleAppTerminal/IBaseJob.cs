using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YwfSimpleConsoleAppTerminal
{
    public interface IBaseJob
    {
        /// <summary>
        /// 运行任务，执行相关业务逻辑
        /// </summary>
        /// <returns></returns>
        void Execute();
    }
}
