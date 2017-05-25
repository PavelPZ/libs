using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignConsole {
  class Program {
    static void Main(string[] args) {
			//ImportAllLangs.Run();
			//SpellChecker.SpellLang.test().Wait();
			//StemmerBreaker.Runner.test();
			//StemmerBreaker.Runner.test();
			Fulltext.BloggingContext.spellCheckedWordBreak(LangsLib.Langs.cs_cz, "Ahoj Verčo, jak, se, máš");
		}
  }
}
