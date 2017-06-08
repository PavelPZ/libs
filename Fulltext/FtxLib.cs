﻿using LangsLib;
using Microsoft.EntityFrameworkCore;
using SpellChecker;
using STALib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fulltext {


	public class RunInsertPhrase : RunObject<PhraseWords> {

		public TaskCompletionSource<PhraseWords> tcs { get; set; }
		public void doRun() { tcs.TrySetResult(Run()); }

		public RunInsertPhrase(int phraseId, PhraseSide phraseSide, PhraseWords oldText, string newWords) {
			this.phraseId = phraseId; this.phraseSide = phraseSide; this.oldText = oldText; this.newWords = newWords;
		}
		int phraseId; PhraseSide phraseSide; PhraseWords oldText; string newWords;

		public PhraseWords Run() {
			return FtxLib.STAInsert(phraseId, phraseSide, oldText, newWords);
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

		public static PhraseWords STAInsert(int phraseId, PhraseSide phraseSide /*dict and its side, e.g. czech part of English-Czech dict*/, PhraseWords oldText /*null => insert, else update*/, string newWords /*null => delete, else update or insert*/) {
			var ctx = new FulltextContext(); var lang = phraseSide.langOfText(); var newText = new PhraseWords { Text = newWords };

			Action<WordIdx[]> addNews = wordIdxs => {
				STASpellCheck(lang, wordIdxs, newText); //low level spell check
				for (var i = 0; i < wordIdxs.Length; i++) if (newText.Idxs[wordIdxs[i].idx].Len > 0) //new correct words to fulltext DB
						ctx.PhraseWords.Add(new PhraseWord() { SrcLang = (byte)phraseSide.src, DestLang = (byte)phraseSide.dest, Word = wordIdxs[i].word, PhraseRef = phraseId });
			};

			if (newWords == null) { //DELETE
				ctx.Database.ExecuteSqlCommand(string.Format("delete PhraseWords where {0}={{0}} and {1}={{1}} and {2}={{2}}", PhraseWord.PhraseIdName, PhraseWord.SrcLangName, PhraseWord.DestLangName), new object[] { phraseId, (byte)phraseSide.src, (byte)phraseSide.dest });
				return null;
			}
			//Word breaking
			STAWordBreak(lang, newText);

			var newWordIdx = getWordIdx(newText);
			if (oldText == null) { //insert
				addNews(newWordIdx); //Add news 
			} else { //update

				//Delete olds
				var olds = getWordIdx(oldText);
				var dict = phraseSide.getDictId();
				var oldsDB = ctx.PhraseWords.Where(pw => pw.PhraseRef == phraseId && pw.SrcLang == (byte)phraseSide.src && pw.DestLang == (byte)phraseSide.dest).ToArray();
				foreach (var w in olds.Except(newWordIdx)) ctx.PhraseWords.Remove(oldsDB.First(db => db.Word == w.word)); //oldsDB.Where(ww => !boths.Contains(ww.Word))) ctx.PhraseWords.Remove(w);

				//Add news
				var news = newWordIdx.Except(olds).ToArray();
				addNews(news);
			}

			ctx.SaveChanges();
			return newText;
		}

		public static Task<PhraseWords> Insert(int phraseId, PhraseSide phraseSide /*dict and its side, e.g. czech part of English-Czech dict*/, PhraseWords oldText /*null => insert, else update*/, string newWords /*null => delete, else update or insert*/) {
			return Lib.Run(new RunInsertPhrase(phraseId, phraseSide, oldText, newWords));
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
			if (errorIdxs != null) foreach (var errIdx in errorIdxs) newText.Idxs[errIdx] = new TPosLen() { Pos = newText.Idxs[errIdx].Pos, Len = -newText.Idxs[errIdx].Len };
		}


		public static string[] StemmingWithSQLServer(Langs lang, string phrase) {
			var ctx = new FulltextContext();
			var sql = string.Format("SELECT display_term FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, \"{0}\")', {1}, 0, 1)", phrase.Replace("'", "''")/*https://stackoverflow.com/questions/5528972/how-do-i-convert-a-string-into-safe-sql-string*/, Metas.lang2LCID(lang));
			return ctx.Set<dm_fts_parser>().FromSql(sql).Select(p => p.display_term).ToArray();
		}

		public static async void test() {

			var ctx = new FulltextContext();
			ctx.recreate();
			ctx.Database.ExecuteSqlCommand("delete PhraseWords");
			for (var idx = 0; idx < 100; idx++) {
				var phrase = await Insert(123, new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, null, "Ahoj, jak se máš?");
				var search = await SearchPhrase(new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, "měj", true);
				search = await SearchPhrase(new PhraseSide { src = Langs.en_gb, dest = Langs.cs_cz }, "měj", false);
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

}
