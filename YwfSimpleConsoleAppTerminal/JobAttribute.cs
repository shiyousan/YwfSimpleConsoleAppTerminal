using System;

namespace YwfSimpleConsoleAppTerminal
{
    public class JobAttribute : Attribute
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public string CommandName { get; set; }
        /// <summary>
        /// 描述/补充说明/额外说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 排序号
        /// 值越大排序越靠后
        /// </summary>
        public int Sort { get; set; } = 99999;
    }
}
