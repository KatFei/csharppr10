using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeProject.Library
{
    public enum Result { OK, Fail };
    public enum SendingType { Sys, Msg, File, FileInf};
    public class OperationResult
    {
        public Result Result;
        public string Message; // текст сообщения или расширение файла
        public SendingType Type;
        public OperationResult(Result result, string message, SendingType type = SendingType.Msg)
        {
            Result = result;
            Message = message;
            Type = type;
        }
    }
}
