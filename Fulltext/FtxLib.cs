using LangsLib;
using Microsoft.EntityFrameworkCore;
using SpellChecker;
using STALib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fulltext {


	public class RunInsertPhrase : RunObject<Phrase> {

		public TaskCompletionSource<Phrase> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunInsertPhrase(string newWords, int? phraseId, PhraseSide? phraseSide, int? srcSideId) {
			this.phraseId = phraseId; this.phraseSide = phraseSide; this.newWords = newWords; this.srcSideId = srcSideId;
		}
		int? phraseId; PhraseSide? phraseSide; string newWords; int? srcSideId;

		public Phrase Run() {
			return FtxLib.STAInsert(newWords, phraseId, phraseSide, srcSideId);
		}
	}

	public class RunSearchPhrase : RunObject<int[]> {

		public TaskCompletionSource<int[]> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunSearchPhrase(PhraseSide phraseSide, string text, bool isDBStemming) {
			this.phraseSide = phraseSide; this.text = text; this.isDBStemming = isDBStemming;
		}
		PhraseSide phraseSide; string text; bool isDBStemming;

		public int[] Run() {
			return FtxLib.STASearchPhrase(phraseSide, text, isDBStemming);
		}
	}

	public class RunBreakAndCheck : RunObject<PhraseWords> {

		public TaskCompletionSource<PhraseWords> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunBreakAndCheck(Langs lang, string phrase) {
			this.lang = lang; this.phrase = phrase;
		}
		Langs lang; string phrase;

		public PhraseWords Run() {
			return FtxLib.STABreakAndCheck(lang, phrase);
		}

		public static Task<PhraseWords> SpellCheck(Langs lang, string phrase) {
			return Lib.Run(new RunBreakAndCheck(lang, phrase));
		}

	}

	public class FtxLib {

		public static PhraseWords STABreakAndCheck(Langs lang, string phrase) {
			var newText = new PhraseWords { Text = phrase };
			//WordBreaking without brackets
			STAWordBreak(lang, newText);
			//lowercased correct words of PhraseWord.maxWordLen
			var newWordIdx = getWordIdx(newText);
			STASpellCheck(lang, newWordIdx, newText); //low level spell check
			return newText;
		}

		//Bracket parsing
		public static IEnumerable<Bracket> BracketParse(string s) { foreach (Match match in brackets.Matches(s)) yield return new Bracket { Br = match.Value[0], Text = match.Value.Substring(1, match.Value.Length - 2) }; }
		public struct Bracket { public char Br; public string Text; }
		static Regex brackets = new Regex(@"\((.*?)\)|\{(.*?)\}|\[(.*?)\]");

		public static Phrase STAInsert(string newWords /*NullOrEmpty => delete, else update or insert*/, int? phraseId /*==null => insert else update or delete*/, PhraseSide? phraseSide /*for Insert: dict and its side, e.g. czech part of English-Czech dict*/, int? srcSideId /*for inserting Destination side*/) {
			var ctx = new FulltextContext();

			if (string.IsNullOrEmpty(newWords)) { //DELETE
				if (phraseId == null || phraseSide!=null) throw new Exception("phraseId == null || phraseSide!=null");
				var delPh = ctx.Phrases.Include(p => p.Dests).First(p => p.Id == phraseId);
				ctx.Phrases.RemoveRange(delPh.Dests);
				ctx.Phrases.Remove(delPh);
				ctx.SaveChanges();
				return null;
			}

			Phrase ph; PhraseWords oldText = null; PhraseSide ps;
			if (phraseId != null) { //UPDATE
				if (srcSideId != null) throw new Exception("srcSideId != null");
				ph = ctx.Phrases.Include(p => p.Words).First(p => p.Id == phraseId);
				oldText = new PhraseWords { Text = ph.Text, Idxs = TPosLen.fromBytes(ph.TextIdxs) };
				ps = new PhraseSide { src = (Langs)ph.SrcLang, dest = (Langs)ph.DestLang };
			} else { //INSERT
				if (phraseSide == null) throw new Exception("phraseSide == null");
				ps = (PhraseSide)phraseSide;
				if (ps.src!=ps.dest && srcSideId == null) throw new Exception("ps.src!=ps.dest && srcSideId == null");
				ctx.Phrases.Add(ph = new Phrase { SrcLang = (byte)ps.src, DestLang = (byte)ps.dest, SrcRef = srcSideId }); 
			}

			var lang = ps.langOfText(); var newText = new PhraseWords { Text = newWords };

			Action<WordIdx[]> addNews = wordIdxs => {
				STASpellCheck(lang, wordIdxs, newText); //low level spell check
				for (var i = 0; i < wordIdxs.Length; i++) if (newText.Idxs[wordIdxs[i].idx].Len > 0) //new correct words to fulltext DB
						ctx.PhraseWords.Add(new PhraseWord() { SrcLang = (byte)ps.src, DestLang = (byte)ps.dest, Word = wordIdxs[i].word, Phrase = ph });
			};

			//Word breaking
			STAWordBreak(lang, newText);

			var newWordIdx = getWordIdx(newText);
			if (oldText == null) { //insert
				addNews(newWordIdx); //Add news 
			} else { //update

				//Delete olds
				var olds = getWordIdx(oldText);
				var oldsDB = ph.Words;
				foreach (var w in olds.Except(newWordIdx)) ctx.PhraseWords.Remove(oldsDB.First(db => db.Word == w.word)); //oldsDB.Where(ww => !boths.Contains(ww.Word))) ctx.PhraseWords.Remove(w);

				//Add news
				var news = newWordIdx.Except(olds).ToArray();
				addNews(news);
			}

			ph.Text = newText.Text;
			ph.TextIdxs = TPosLen.toBytes(newText.Idxs);
			ph.Base = ""; 

			ctx.SaveChanges();
			return ph;
		}

		public static Task<Phrase> Insert(string newWords, int? phraseId, PhraseSide? phraseSide, int? srcSideId = null) {
			return Lib.Run(new RunInsertPhrase(newWords, phraseId, phraseSide, srcSideId));
		}

		public static int[] STASearchPhrase(PhraseSide phraseSide, string text, bool isDBStemming) {
			var ctx = new FulltextContext(); var lang = phraseSide.langOfText(); var txt = new PhraseWords { Text = text }; //var dict = phraseSide.getDictId();

			txt.Idxs = StemmerBreaker.RunBreaker.STAWordBreak(lang, text);
			var words = getWordIdx(txt);
			List<string> res = new List<string>();
			foreach (var w in words) {
				if (!StemmerBreaker.Lib.hasStemmer(lang)) res.Add(w.word); //stemmer does not exists => and single word (same as in the StemmerBreaker.Runner.stemm: if (stemmer == null) { onPutWord(PutTypes.put, word); return; })
				else {
					var st = isDBStemming ? (IEnumerable<string>)StemmingWithSQLServer(lang, w.word) : StemmerBreaker.RunStemmer.STAStemm(lang, w.word);
					//var st1 = StemmerBreaker.RunStemmer.STAStemm(lang, w.word);
					//var st2 = DBStemming(lang, w.word);
					res.AddRange(st);
				}
			}
			res = res.Distinct().ToList();
			var ids = ctx.PhraseWords.Where(w => w.SrcLang == (byte)phraseSide.src && w.DestLang == (byte)phraseSide.dest && res.Contains(w.Word)).Select(w => w.PhraseRef).Distinct().ToArray();
			return ids;
		}

		public static Task<int[]> SearchPhrase(PhraseSide phraseSide, string text, bool isDBStemming) {
			return Lib.Run(new RunSearchPhrase(phraseSide, text, isDBStemming));
		}

		static Func<PhraseWords, WordIdx[]> getWordIdx = phr => {
			string pomStr;
			return phr.Idxs.Where(idx => idx.Len > 0).Select((idx, i) => new WordIdx { idx = i, fullWord = pomStr = phr.Text.Substring(idx.Pos, idx.Len), word = pomStr.Substring(0, Math.Min(idx.Len, PhraseWord.maxWordLen)).ToLower() }).ToArray();
		};

		static void STAWordBreak(Langs lang, PhraseWords text) {
			var noBrackets = brackets.Replace(text.Text, match => new String(' ', match.Length));
			text.Idxs = StemmerBreaker.RunBreaker.STAWordBreak(lang, noBrackets);
		}

		static void STASpellCheck(Langs lang, WordIdx[] nws, PhraseWords newText) {
			//Spell check
			var errorIdxs = RunSpellCheckWords.STACheck(lang, nws);
			//update Len for wrong words
			if (errorIdxs != null) foreach (var errIdx in errorIdxs) newText.Idxs[errIdx] = new TPosLen() { Pos = newText.Idxs[errIdx].Pos, Len = (sbyte)-newText.Idxs[errIdx].Len };
		}


		public static string[] StemmingWithSQLServer(Langs lang, string phrase) {
			var ctx = new FulltextContext();
			var sql = string.Format("SELECT display_term FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, \"{0}\")', {1}, 0, 1)", phrase.Replace("'", "''")/*https://stackoverflow.com/questions/5528972/how-do-i-convert-a-string-into-safe-sql-string*/, Metas.lang2LCID(lang));
			return ctx.Set<dm_fts_parser>().FromSql(sql).Select(p => p.display_term).ToArray();
		}

		public static async void test() {

			var ctx = new FulltextContext();
			//ctx.recreate();
			ctx.Database.ExecuteSqlCommand("delete Phrases");
			for (var idx = 0; idx < 100; idx++) {
				var engPhrase = await Insert("Now are you?", null, new PhraseSide { src = Langs.en_gb, dest = Langs.en_gb });
				var phrase = await Insert("Ahoj, jak se máš?", null, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz}, engPhrase.Id);
				var search = await SearchPhrase(new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, "měj", true);
				search = await SearchPhrase(new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, "měj", false);
				await Insert("Ahoj, jak se máš? Asi dobře Kadle.", phrase.Id, null);
				await Insert("Asi dobře, Karle.", phrase.Id, null);
				await Insert(null, engPhrase.Id, null);
				//await Insert(null, phrase.Id, null);
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

}

