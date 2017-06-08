using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileHelpers;
using System.IO;
using Newtonsoft.Json;
using LangsLib;
using Microsoft.EntityFrameworkCore;

namespace DesignConsole.ImportDicts {
	public static class Import {
		public static void importAll() {
			var ctx = new Fulltext.FulltextContext();
			//ctx.recreate();
			ctx.Database.ExecuteSqlCommand("ALTER DATABASE test SET RECOVERY SIMPLE");
			//ctx.Database.ExecuteSqlCommand("delete Dicts");
			ctx.Database.ExecuteSqlCommand("DBCC SHRINKDATABASE (test, 0);");
			ctx.Database.ExecuteSqlCommand("DBCC SHRINKFILE (test_log, 0);");
			importAll(ImportCSVDict);
		}
		public static void importAll(Func<Meta, CVSDictItem[], Task> run) {
			var metas = Metas.read();
			foreach (var meta in metas.Items)
				import(meta, run);
		}

		public static void import(Meta meta, Func<Meta, CVSDictItem[], Task> run) {
			var engine = new FileHelperEngine(typeof(CVSDictItem));
			var res = (CVSDictItem[])engine.ReadFile(meta.fullPath());
			var task = run(meta, res); if (task != null) task.Wait();
		}

		public static void exportLogs(Meta meta, CVSDictItem[] items) {
			var engine = new FileHelperEngine(typeof(CVSDictItem));
			engine.HeaderText = "Src;Dest;SrcLog;DestLog";
			engine.Encoding = Encoding.UTF8;
			engine.WriteFile(meta.logPath(), items);
		}

		public static List<int> STAImportCSVDict(Meta meta, CVSDictItem[] items) {
			var ids = new List<int>();
			var ctx = new Fulltext.FulltextContext(); int count = 0;
			var dict = ctx.Dicts.Include(d => d.Phrases).FirstOrDefault(d => d.Name == meta.path);
			if (dict != null) { ctx.Dicts.Remove(dict); ctx.SaveChanges(); ctx = new Fulltext.FulltextContext(); }
			ctx.Dicts.Add(dict = new Fulltext.Dict { Name = meta.path, Imported = DateTime.UtcNow, SrcLang = (byte) meta.src });
			foreach (var row in items) {
				var srcPhrase = Fulltext.FtxLib.STAInsert(ctx, row.Src, null, dict, new PhraseSide { src = meta.src, dest = meta.src }, null);
				var destPhrase = Fulltext.FtxLib.STAInsert(ctx, row.Dest, null, dict, new PhraseSide { src = meta.src, dest = meta.dest }, srcPhrase.Id);
				ids.Add(destPhrase.Id);
				if (count++ > 1000) { ctx.SaveChanges(); ctx = new Fulltext.FulltextContext(); ctx.Dicts.Attach(dict); count = 0; }
			}
			ctx.SaveChanges();
			new Fulltext.FulltextContext().Database.ExecuteSqlCommand("DBCC SHRINKFILE (test_log, 0);");
			return ids;
		}

		public static Task<List<int>> ImportCSVDict(Meta meta, CVSDictItem[] items) {
			return STALib.Lib.Run(new RunImportCSVDict(meta, items));
		}

		public static spellCheckResult STAExtendCSVDict(Meta meta, CVSDictItem[] items) {
			Action<PhraseWords, HashSet<string>, bool> addWords = (phr, res, isWrong) => { foreach (var w in phr.Idxs.Where(idx => isWrong == idx.Len < 0).Select(idx => phr.Text.Substring(idx.Pos, Math.Abs(idx.Len)))) res.Add(w); };
			spellCheckResult data = new spellCheckResult();
			//Break and Spell check
			foreach (var row in items) {
				var txt = Fulltext.FtxLib.STABreakAndCheck(meta.src, row.Src); row.SrcLog = TPosLen.encode(txt.Idxs);
				addWords(txt, data.getWords(true, true), true); addWords(txt, data.getWords(true, false), false);
				foreach (var bp in Fulltext.FtxLib.BracketParse(row.Src)) data.getBracketsWords(true, bp.Br).Add(bp.Text);
				txt = Fulltext.FtxLib.STABreakAndCheck(meta.dest, row.Dest); row.DestLog = TPosLen.encode(txt.Idxs);
				addWords(txt, data.getWords(false, true), true); addWords(txt, data.getWords(false, false), false);
				foreach (var bp in Fulltext.FtxLib.BracketParse(row.Dest)) data.getBracketsWords(false, bp.Br).Add(bp.Text);
			}
			exportLogs(meta, items);
			data.save(meta);
			return data;
		}

		public static Task<spellCheckResult> ExtendCSVDict(Meta meta, CVSDictItem[] items) {
			return STALib.Lib.Run(new RunExtendCSVDict(meta, items));
		}

		public static CVSDictItem[] import(string fn) {
			return null;
			//Metas metas = new Metas {
			//	Items = new Meta[] {
			//	new Meta {src = Langs.ru_ru, dest = Langs.cs_cz, path=@"__Lingea\XX-CZ\Russian-Czech.csv" },
			//	new Meta {src = Langs.en_gb, dest = Langs.cs_cz, path=@"__Lingea\XX-CZ\English-Czech.csv" },
			//}
			//};

			//File.WriteAllText(@"d:\temp\dictmetas.json", JsonConvert.SerializeObject(metas), Encoding.UTF8);
			//return null;

			////create a CSV engine using FileHelpers for your CSV file
			//var engine = new FileHelperEngine(typeof(CVSDictItem));
			////read the CSV file into your object Arrary
			//return (CVSDictItem[])engine.ReadFile(fn);
		}
	}

	public class spellCheckResult {
		public spellCheckResult() { }
		HashSet<string>[] wordLists = new HashSet<string>[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
		public HashSet<string> getWords(bool isSrc, bool isWrong) { return wordLists[isSrc ? (isWrong ? 1 : 0) : (isWrong ? 3 : 2)]; }
		public HashSet<string> getBracketsWords(bool isSrc, char br) { return wordLists[(isSrc ? 0 : 3) + (br == '(' ? 4 : (br == '{' ? 5 : 6))]; } //0 for (, 1 for {, 2 for [
		[DelimitedRecord(";"), IgnoreEmptyLines(), IgnoreFirst()]
		public class TRow { public string SrcOK; public string SrcWrong; public string DestOK; public string DestWrong; public string SrcRoundBR; public string SrcCurlyBR; public string SrcSquareBR; public string DestRoundBR; public string DestCurlyBR; public string DestSquareBR; }
		IEnumerable<object> getRows() {
			var wls = wordLists.Select(w => w.Select(ww => ww.Replace(';', '|')).OrderBy(ww => ww).ToArray()).ToArray();
			Func<int, int, string> val = (idx, i) => { return i < wls[idx].Length ? wls[idx][i] : ""; };
			for (var i = 0; i < wordLists.Max(w => w.Count); i++)
				yield return new TRow { SrcOK = val(0, i), SrcWrong = val(1, i), DestOK = val(2, i), DestWrong = val(3, i), SrcRoundBR = val(4, i), SrcCurlyBR = val(5, i), SrcSquareBR = val(6, i), DestRoundBR = val(7, i), DestCurlyBR = val(8, i), DestSquareBR = val(9, i) };
		}
		public void save(Meta meta) {
			var engine = new FileHelperEngine(typeof(TRow));
			engine.HeaderText = "SrcOK;SrcWrong;DestOK;DestWrong;SrcRoundBR;SrcCurlyBR;SrcSquareBR;DestRoundBR;DestCurlyBR;DestSquareBR";
			engine.Encoding = Encoding.UTF8;
			engine.WriteFile(meta.logPath().Replace(@"\ex-", @"\br-"), getRows()); //, Encoding.UTF8);
		}
	}

	public class RunExtendCSVDict : STALib.RunObject<spellCheckResult> {

		public TaskCompletionSource<spellCheckResult> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunExtendCSVDict(Meta meta, CVSDictItem[] items) {
			this.meta = meta; this.items = items;
		}
		Meta meta; CVSDictItem[] items;

		public spellCheckResult Run() {
			return Import.STAExtendCSVDict(meta, items);
		}

	}

	public class RunImportCSVDict : STALib.RunObject<List<int>> {

		public TaskCompletionSource<List<int>> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunImportCSVDict(Meta meta, CVSDictItem[] items) {
			this.meta = meta; this.items = items;
		}
		Meta meta; CVSDictItem[] items;

		public List<int> Run() {
			return Import.STAImportCSVDict(meta, items);
		}

	}



}

