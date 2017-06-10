using LangsLib;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

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
				//ImportDicts.Import.Duplicities();
				//new Fulltext.FulltextContext().Database.ExecuteSqlCommand("delete Dicts");
				ImportDicts.Import.importAll();
			});
			th.Start();
		}
	}
}
