using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UScheme {

    class CommandLineOptions {
        public readonly bool StartRepl;
        public readonly string LoadFilename;

        public CommandLineOptions(string[] args) {
            for (int i = 0 ; i < args.Length ; i++)
                switch (args[i]) {
                    case "-repl":
                        StartRepl = true;
                        break;
                    case "-load":
                        LoadFilename = args[i + 1];
                        i++;
                        break;
                }
        }
    }

    class Program {
        static void Main(string[] args) {
            var cmdOptions = new CommandLineOptions(args);
            Env environment = (cmdOptions.LoadFilename != null) ? UScheme.Load(cmdOptions.LoadFilename) : Env.InitialEnv();
            if (cmdOptions.StartRepl)
                UScheme.Loop(Console.In, Console.Out, environment);
            else
                Console.In.ReadLine();
        }
    }
}
