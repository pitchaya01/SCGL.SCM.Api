using System;
using System.Collections.Generic;
using System.Text;

namespace Lazarus.Common.ExceptionHandling
{
    public class BusinessException : Exception
    {
        public BusinessException(string msg) : base(msg)
        {

        }
    }
}
