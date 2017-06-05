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
//            UScheme.Loop(Console.In, Console.Out);
            Env env = UScheme.Load("../../uscheme/test.usc");
            Console.Out.WriteLine(env.Eval("results"));
//            Console.Out.WriteLine(env.Eval("(cons (= 1 0) 2)"));
            Console.In.ReadLine();
        }
    }
}
