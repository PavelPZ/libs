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
				//while (true) {
				//	var txt = await ImportDicts.Import.ExtendCSVDict(new ImportDicts.Meta {
				//		src = Langs.bg_bg, dest = Langs.en_gb,
				//		path = @"__Lingea\EN-XX\English-Bulgarian.csv"
				//	}, new ImportDicts.CVSDictItem[] {new ImportDicts.CVSDictItem {
				//	Dest = "go",
				//	Src = "добивам, извлека {lfw нещо от нещо} {lfd (газ, нефт и т.н.)}, добия {lfd (природни ресурси)}^{styl (форм.)} извадя {lfw нещо от нещо} {lfd (кърпичка от джоба си и т.н.)}, измъкна {lfw нещо от нещо} {lfd (книга от шкаф и т.н.)}^извадя {lfw какво} {lfd (зъб)}, изтръгна {lfd (със сила)}^облагодетелствам се {lfw от нещо} {lfd (извлека полза, облага и т.н.)}^{prag (хим.)} екстрахирам {lfd (подлагам на екстракция)}",
				//	SrcLog = null, DestLog = null
				//} });
				//}
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
