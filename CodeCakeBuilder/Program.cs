﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCake
{
    class Program
    {
        /// <summary>
        /// CodeCakeBuilder entry point. This is a default, simple, implementation that can 
        /// be extended as needed.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>An error code (typically -1), 0 on success.</returns>
        static int Main(string[] args)
        {
            var app = new CodeCakeApplication();
            bool interactive = !args.Contains('-' + InteractiveAliases.NoInteractionArgument, StringComparer.OrdinalIgnoreCase);
            int result = app.Run(args);
            Console.WriteLine();
            if (interactive)
            {
                Console.WriteLine("Hit any key to exit. (Use -{0} parameter to exit immediately)", InteractiveAliases.NoInteractionArgument);
                Console.ReadKey();
            }
            return result;
        }
    }
}
