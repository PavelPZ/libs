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
				//ImportAllLangs.FrequencyDirs();
				//StemmerBreaker.Runner.test();
				//Fulltext.FtxLib.test();
				ImportDicts.Import.importAll(ImportDicts.Import.ExtendCSVDict);
				//var res = await Fulltext.RunBreakAndCheck.SpellCheck(Langs.en_gb, "automatization.)");
				//ImportDicts.Import.Duplicities();
				//new Fulltext.FulltextContext().Database.ExecuteSqlCommand("delete Dicts"); ImportDicts.Import.importAll();
				//if (SpellChecker.Frequency.forLangs == null) return;
			});
			th.Start();
		}
	}
}
