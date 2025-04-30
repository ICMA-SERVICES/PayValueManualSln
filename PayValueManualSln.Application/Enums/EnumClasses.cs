using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Enums
{
    public enum PayerType
    {
        Ind = 1,
        Agen = 2
    }
    public enum InputDefinitionEnum
    {
        [Description("Pages")]
        Page = 1,
        [Description("Value")]
        Value = 2,
        [Description("Land Size")]
        LandSize = 3,
    }
}
