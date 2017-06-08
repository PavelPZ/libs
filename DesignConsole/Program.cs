using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesignConsole {
  class Program {
		static void Main(string[] args) {
			var th = new Thread(/*async*/ () => {
				//ImportAllLangs.Run();
				//SpellChecker.SpellLang.test().Wait();
				//StemmerBreaker.Runner.test();
				//Fulltext.FtxLib.test();
				ImportDicts.Import.importAll(ImportDicts.Import.ExtendCSVDict);
				//var res = await Fulltext.RunSpellCheck.SpellCheck(LangsLib.Langs.cs_cz, "ahoj (jak se) máš? {Já} docela [dobře] xxxx.)");
			});
			th.Start();
		}
  }
}
