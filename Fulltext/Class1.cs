using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using LangsLib;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using SpellChecker;
using STALib;

namespace Fulltext {

	public class PhraseWord {

		public const int maxWordLen = 24;
		public const string PhraseIdName = "PhraseId";
		public const string DictIdName = "DictId";

		[Key]
		public int Id { get; set; } //internal unique ID
		[MaxLength(maxWordLen)]
		public string Word { get; set; }
		public Int64 PhraseId { get; set; } //ID of phrase, containing word. Could be Hash64 of string, identifying phrase in its source repository.
		[MaxLength(2)]
		public byte[] DictId { get; set; } //First byte is Dict Lang, second is Phrase Lang. If Phrase Lang==_ => Phrase means source, otherwise translation.
	}

	public class RunInsertPhrase : RunObject<PhraseWords> {

		public TaskCompletionSource<PhraseWords> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunInsertPhrase(Int64 phraseId, PhraseSide phraseSide, PhraseWords oldText, string newWords) {
			this.phraseId = phraseId; this.phraseSide = phraseSide; this.oldText = oldText; this.newWords = newWords;
		}
		Int64 phraseId; PhraseSide phraseSide; PhraseWords oldText; string newWords;

		public PhraseWords Run() {
			return FulltextContext.STAInsert(phraseId, phraseSide, oldText, newWords);
		}
	}

	public class RunSearchPhrase : RunObject<Int64[]> {

		public TaskCompletionSource<Int64[]> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunSearchPhrase(PhraseSide phraseSide, string text) {
			this.phraseSide = phraseSide; this.text = text;
		}
		PhraseSide phraseSide; string text;

		public Int64[] Run() {
			return FulltextContext.STASearchPhrase(phraseSide, text);
		}
	}

	public class FulltextContext : DbContext {
		public DbSet<PhraseWord> PhraseWords { get; set; }
		//public DbSet<Phrase> Phrases { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer(@"Data Source=PZ-W8VIRTUAL\SQLEXPRESS;Initial Catalog=test;Integrated Security=True;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<PhraseWord>().HasIndex(p => p.Word);
			modelBuilder.Entity<PhraseWord>().HasIndex(p => p.PhraseId);
		}

		public void recreate() {
			Database.EnsureDeleted();
			Database.EnsureCreated();
		}

		public static PhraseWords STAInsert(Int64 phraseId, PhraseSide phraseSide /*dict and its side, e.g. czech part of English-Czech dict*/, PhraseWords oldText /*null => insert, else update*/, string newWords /*null => delete, else update or insert*/) {
			var ctx = new FulltextContext(); var lang = phraseSide.langOfText(); var newText = new PhraseWords { Text = newWords };

			Action<WordIdx[]> addNews = nws => {
				//Spell check
				var errorIdxs = RunSpellCheckWords.STACheck(lang, nws);
				//update Len for wrong words
				if (errorIdxs != null) foreach (var errIdx in errorIdxs) newText.Idxs[errIdx] = new LangsLib.TPosLen() { Pos = newText.Idxs[errIdx].Pos, Len = -newText.Idxs[errIdx].Len };
				//
				for (var i = 0; i < nws.Length; i++) if (newText.Idxs[nws[i].idx].Len > 0)
						ctx.PhraseWords.Add(new PhraseWord() { DictId = phraseSide.getDictId(), Word = nws[i].word, PhraseId = phraseId });
			};

			if (newWords == null) { //DELETE
				ctx.Database.ExecuteSqlCommand(string.Format("delete PhraseWords where {0}={{0}} and {1}={{1}}", PhraseWord.PhraseIdName, PhraseWord.DictIdName), new object[] { phraseId, phraseSide.getDictId() });
				return null;
			}
			//Word breaking
			//var x = new StemmerBreaker.Runner(Langs.cs_cz).wordBreak("Ahoj");
			newText.Idxs = StemmerBreaker.RunBreaker.STAWordBreak(lang, newText.Text);
			//using (var st = new StemmerBreaker.Runner(lang)) newText.Idxs = st.wordBreak(newText.Text);

			var newWordIdx = getWordIdx(newText);
			if (oldText == null) { //insert
				addNews(newWordIdx); //Add news 
			} else { //update

				//Delete olds
				var olds = getWordIdx(oldText);
				var dict = phraseSide.getDictId();
				var oldsDB = ctx.PhraseWords.Where(pw => pw.PhraseId == phraseId && pw.DictId == dict).ToArray();
				foreach (var w in olds.Except(newWordIdx)) ctx.PhraseWords.Remove(oldsDB.First(db => db.Word == w.word)); //oldsDB.Where(ww => !boths.Contains(ww.Word))) ctx.PhraseWords.Remove(w);

				//Add news
				var news = newWordIdx.Except(olds).ToArray();
				addNews(news);
			}

			ctx.SaveChanges();
			return newText;
		}

		public static Task<PhraseWords> Insert(Int64 phraseId, PhraseSide phraseSide /*dict and its side, e.g. czech part of English-Czech dict*/, PhraseWords oldText /*null => insert, else update*/, string newWords /*null => delete, else update or insert*/) {
			return Lib.Run(new RunInsertPhrase(phraseId, phraseSide, oldText, newWords));
		}

		public static Int64[] STASearchPhrase(PhraseSide phraseSide, string text) {
			var ctx = new FulltextContext(); var lang = phraseSide.langOfText(); var txt = new PhraseWords { Text = text }; var dict = phraseSide.getDictId();
			txt.Idxs = StemmerBreaker.RunBreaker.STAWordBreak(lang, text);
			var words = getWordIdx(txt);
			List<string> res = new List<string>();
			foreach (var w in words) res.AddRange(StemmerBreaker.RunStemmer.STAStemm(lang, w.word));
			res = res.Distinct().ToList();
			var ids = ctx.PhraseWords.Where(w => w.DictId == dict && res.Contains(w.Word)).Select(w => w.PhraseId).Distinct().ToArray();
			return ids;
		}

		public static Task<Int64[]> SearchPhrase(PhraseSide phraseSide, string text) {
			return Lib.Run(new RunSearchPhrase(phraseSide, text));
		}

		static Func<PhraseWords, WordIdx[]> getWordIdx = phr => phr.Idxs.Where(idx => idx.Len > 0).Select((idx, i) => new WordIdx { idx = i, word = phr.Text.Substring(idx.Pos, Math.Min(idx.Len, PhraseWord.maxWordLen)).ToLower() }).ToArray();

		public static async void test() {

			var ctx = new FulltextContext();
			//ctx.recreate();
			ctx.Database.ExecuteSqlCommand("delete PhraseWords");
			for (var idx = 0; idx < 100; idx++) {
				var phrase = await Insert(123, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, null, "Ahoj, jak se máš?");
				var search = await SearchPhrase(new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, "měj");
				phrase = await Insert(123, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, phrase, "Ahoj, jak se máš? Asi dobře Kadle.");
				phrase = await Insert(123, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, phrase, "Asi dobře, Karle.");
				phrase = await Insert(123, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, null, null);
			}

			return;

			//Parallel.ForEach(Enumerable.Range(0, 1000), async idx => {
			//	var res = await RunSpellCheck.Check(Langs.de_de, new string[] {
			//			"einem", "Pferd", "die", "Sporen", "geben"
			//			//"einem", "pferd", "die", "sporen", "geben"
			//			//"Einem Pferd die Sporen geben."
			//			//"Klassenbuch"
			//		});
			//	res = null;
			//});


			//Parallel.ForEach(Enumerable.Range(0, 1000), async idx => {
			//	var s = "yyy"; for (var i = 0; i < idx; i++) s += "x";
			//	var res = await RunSpellCheckWords.Check(Langs.cs_cz, new string[] {
			//			//"einem", "Pferd", "die", "Sporen", "geben", s
			//			s, "ahoj", "jak", "se", "máš"
			//			//"einem", "pferd", "die", "sporen", "geben"
			//			//"Einem Pferd die Sporen geben."
			//			//"Klassenbuch"
			//		});
			//	if (idx >= 999)
			//		res = null;
			//});

			return;

			//public static async LangsLib.PhraseWord[] spellCheckedWordBreak(Langs lang, string text) {

			//for (var idx = 0; idx < 1000; idx++) {
			//	var res = await SpellChecker.SpellLang.Check(Langs.de_de, new string[] { });
			//}
			//var stemmer = new StemmerBreaker.Runner(lang);
			//var words = stemmer.wordBreak(text);
			//var errors = await RunSpellCheck.Check(lang, words.Select(idx => text.Substring(idx.srcPos, idx.srcLen))) as SpellLangResult;
			//if (errors != null) { };
			//return new LangsLib.PhraseWord[0];


			Langs[] langs = new Langs[] { Langs.cs_cz, Langs.de_de, Langs.ru_ru, Langs.pt_pt, Langs.sk_sk, Langs.fr_fr, Langs.it_it, Langs.es_es };
			//for (var idx = 0; idx < 1000; idx++) {
			//	var s = ""; for (var i = 0; i < idx; i++) s += "x";
			//	var res = await SpellChecker.SpellLang.Check(langs[idx % 8], new string[] {
			//		//s + " ahoj" + " jak" + " se" + " máš"
			//		s, "ahoj", "jak", "se", "máš"
			//	});
			//}
			//Parallel.ForEach(Enumerable.Range(0, 1000), async idx => {
			//	var s = ""; for (var i = 0; i < idx; i++) s += "x";
			//	var res = await SpellChecker.RunSpellCheck.Check(langs[idx % 8], new string[] {
			//		//s + " ahoj" + " jak" + " se" + " máš"
			//		s, "ahoj", "jak", "se", "máš"
			//	});
			//});
			//return null;
			//SpellChecker.RunSpellCheck.Check(lang, new string[] { null });
			//spellRes = SpellChecker.SpellLang.Check(lang, text);
			//using (var rn = new StemmerBreaker.Runner(lang))
			//	rn.wordBreak(text);
			//spellRes = SpellChecker.SpellLang.Check(lang, text);
			//using (var rn = new StemmerBreaker.Runner(lang))
			//	rn.wordBreak(text);
			//spellRes = SpellChecker.SpellLang.Check(lang, text);
			//IEnumerable<StemmerBreaker.Put> stBrRes = stBr.wordBreak(text);
			//if (spellRes != null) stBrRes = stBrRes.Where(wb => spellRes.All(br => br.pos!= wb.srcPos));
			//var words = stBrRes.Select(wb => text.Substring(wb.srcPos, wb.srcLen)).ToArray();
			//return null;
			//public static string[] toStrings(string text, List<Put> idxs)
			//{
			//	return idxs.Select(idx => text.Substring(idx.srcPos, idx.srcLen)).ToArray();
			//}
		}
	}

	public class Class1 {
		protected void Page_Load() {
			var ctx = new FulltextContext();
			ctx.recreate();
			//var all = ctx.Blogs.ToArray();
		}
	}
}
