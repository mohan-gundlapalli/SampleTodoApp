using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp
{
    [Flags]
    public enum UserAction: short
    {
        [Description("List")]
        List = 0,

        [Description("Ldd")]
        Add,

        [Description("Remove")]
        Remove,

        [Description("Exit")]
        Exit
    }
}
