using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SammBotNET.Extensions
{
    public static class BoolExtensions
    {
        public static string ToYesNo(this bool boolean)
        {
            return boolean ? "Yes" : "No";
        }
    }
}
