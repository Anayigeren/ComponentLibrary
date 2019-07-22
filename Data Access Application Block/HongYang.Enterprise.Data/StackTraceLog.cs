using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
namespace HongYang.Enterprise.Data
{
    public class StackTraceLog
    {
        /// <summary>
        /// 获取调用堆栈的信息
        /// </summary>
        /// <returns></returns>
        public static StringBuilder GetStackTraceLog()
        {
            StringBuilder sb = new StringBuilder();
            StackTrace s = new StackTrace();
            StackFrame[] stack = s.GetFrames();
            for (int i = 2; i < stack.Length; i++)
            {
                sb.AppendLine($"调用堆栈{i.ToString()}:{stack[i].GetMethod().DeclaringType}中的{stack[i].GetMethod().Name}方法");
            }

            return sb;
        }
    }
}
