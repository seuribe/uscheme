using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var environment = Env.InitialEnv();

            if (cmdOptions.LoadFilename != null)
                UReader.Load(cmdOptions.LoadFilename, environment);

            if (cmdOptions.StartRepl || cmdOptions.LoadFilename == null)
                Repl(Console.In, Console.Out, environment);
            else
                Console.In.ReadLine();
        }

        static void Repl(TextReader textIn, TextWriter textOut, Env environment) {
            while (true) {
                try {
                    textOut.Write("uscheme > ");

                    var line = textIn.ReadLine();
                    if (line.Equals("!quit"))
                        break;

                    using (var lineStream = new StringReader(line)) {
                        var form = UReader.ReadForm(lineStream);
                        var expression = UScheme.Eval(form, environment);
                        textOut.WriteLine(expression.ToString());
                    }
                } catch (IOException) {
                    break;
                } catch (Exception e) {
                    textOut.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}
