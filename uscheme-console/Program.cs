using System;

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
            var environment = Env.Global;

            if (cmdOptions.LoadFilename != null)
                Parser.Load(cmdOptions.LoadFilename, environment);

            if (cmdOptions.StartRepl || cmdOptions.LoadFilename == null)
                new REPL(Console.In, Console.Out, environment).Start();
            else
                Console.In.ReadLine();
        }
    }
}
