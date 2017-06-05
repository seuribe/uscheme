using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UScheme
{
    class Program
    {
        static void Main(string[] args)
        {
            Env env = UScheme.Load("../../uscheme/r4rstest.scm");
            Console.In.ReadLine();
        }
    }
}
