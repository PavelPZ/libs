﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesignConsole {
  class Program {
		static void Main(string[] args) {
			var th = new Thread(() => {
				//ImportAllLangs.Run();
				//SpellChecker.SpellLang.test().Wait();
				//StemmerBreaker.Runner.test();
				Fulltext.BloggingContext.test();
			});
			th.Start();
		}
  }
}
