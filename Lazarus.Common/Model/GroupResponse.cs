using System;
using System.Collections.Generic;
using System.Text;

namespace Lazarus.Common.Model
{
   public  class GroupResponse<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
    }
}
