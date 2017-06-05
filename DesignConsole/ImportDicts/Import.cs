using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileHelpers;
using System.IO;
using Newtonsoft.Json;
using LangsLib;

namespace DesignConsole.ImportDicts {
	public static class Import {
		public static void importAll(Func<Meta, CVSDictItem[], Object, Task> run, Object data = null) {
			var metas = Metas.read();
			foreach (var meta in metas.Items) import(meta, run, data);
		}

		public static void import(Meta meta, Func<Meta, CVSDictItem[], Object, Task> run, Object data) {
			var engine = new FileHelperEngine(typeof(CVSDictItem));
			var res = (CVSDictItem[])engine.ReadFile(meta.fullPath());
			var task = run(meta, res, data); if (task!=null) task.Wait();
		}

		public static void exportLogs(Meta meta, CVSDictItem[] items) {
			var engine = new FileHelperEngine(typeof(CVSDictItem));
			engine.HeaderText = "Src;Dest;SrcLog;DestLog";
			var lp = meta.logPath();
			engine.WriteFile(meta.logPath(), items);
		}

		public static Task spellCheckAction(Meta meta, CVSDictItem[] items, Object data) {
			spellCheckResult res = (spellCheckResult)data;
			//Break and Spell check
			foreach (var row in items) {
				row.SrcLog = "srcLog"; row.DestLog = "destLog";
			}
			exportLogs(meta, items);
			return new Task(() => { });
		}

		public static CVSDictItem[] import(string fn) {
			Metas metas = new Metas {
				Items = new Meta[] {
				new Meta {src = Langs.ru_ru, dest = Langs.cs_cz, path=@"__Lingea\XX-CZ\Russian-Czech.csv" },
				new Meta {src = Langs.en_gb, dest = Langs.cs_cz, path=@"__Lingea\XX-CZ\English-Czech.csv" },
			}
			};

			File.WriteAllText(@"d:\temp\dictmetas.json", JsonConvert.SerializeObject(metas), Encoding.UTF8);
			return null;

			//create a CSV engine using FileHelpers for your CSV file
			var engine = new FileHelperEngine(typeof(CVSDictItem));
			//read the CSV file into your object Arrary
			return (CVSDictItem[])engine.ReadFile(fn);
		}
	}

	public class spellCheckResult {
		public Dictionary<Langs, HashSet<string>> wrongWords = new Dictionary<Langs, HashSet<string>>();
		public Dictionary<Langs, HashSet<string>> correctWords = new Dictionary<Langs, HashSet<string>>();
		public HashSet<string> getWords(Langs lang, bool isWrong) {
			HashSet<string> res;
			Dictionary<Langs, HashSet<string>> act = isWrong ? wrongWords : correctWords;
			if (act.TryGetValue(lang, out res)) act[lang] = res = new HashSet<string>();
			return res;
		}
	}

}

