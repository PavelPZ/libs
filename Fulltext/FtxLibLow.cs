/*
 - Problémy: délka phrase max. 255 znaků (včetně závorek)
 - závorky uprostřed slova se pro fulltext odstraní (vy)konat = konat
 */
using LangsLib;
using Microsoft.EntityFrameworkCore;
using SpellChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fulltext {


	public static class FtxLibLow {
		//Doplni text.Idxs pro text, zbavený závorek
		public static void STAWordBreak(Langs lang, PhraseWords text) {
			text.Idxs = StemmerBreaker.RunBreaker.STAWordBreak(lang, noBrackets(text.Text));
		}

		//Pro SpellCheck error nastavi zaporne Len.
		public static void STASpellCheck(Langs lang, SelectedWords selected) {
			if (selected.selected == null || selected.selected.Length == 0) return;
			//Spell check
			var errorIdxs = RunSpellCheckWords.STACheck(lang, selected.selected);
			//update Len for wrong words
			if (errorIdxs != null) foreach (var errIdx in errorIdxs) selected.phrase.Idxs[errIdx] = new TPosLen() { Pos = selected.phrase.Idxs[errIdx].Pos, Len = (sbyte)-selected.phrase.Idxs[errIdx].Len };
		}

		public static string[] StemmingWithSQLServer(Langs lang, string phrase) {
			var ctx = new FulltextContext();
			var sql = string.Format("SELECT display_term FROM sys.dm_fts_parser('FormsOf(INFLECTIONAL, \"{0}\")', {1}, 0, 1)", phrase.Replace("'", "''")/*https://stackoverflow.com/questions/5528972/how-do-i-convert-a-string-into-safe-sql-string*/, Metas.lang2LCID(lang));
			return ctx.Set<dm_fts_parser>().FromSql(sql).Select(p => p.display_term).ToArray();
		}

		public static PhraseWords STABreakAndCheck(Langs lang, string phrase) {
			if (string.IsNullOrEmpty(phrase)) return null;
			var newText = new PhraseWords(phrase);
			//WordBreaking text (text in brackets excluded)
			STAWordBreak(lang, newText);
			//Spell check
			STASpellCheck(lang, new SelectedWords(newText)); //low level spell check
			return newText;
		}

		//Bracket parsing
		public static IEnumerable<Bracket> BracketParse(string s) {
			foreach (Match match in otherBrackets.Matches(s)) yield return new Bracket { Br = match.Value[0], Text = match.Value.Substring(1, match.Value.Length - 2) };
			foreach (Match match in roundBrackets.Matches(s)) yield return new Bracket { Br = match.Value[0], Text = match.Value.Substring(1, match.Value.Length - 2) };
		}
		static string noBrackets(string text) { return roundBrackets.Replace(otherBrackets.Replace(text, match => new String(' ', match.Length)), match => new String(' ', match.Length)); }

		public struct Bracket { public char Br; public string Text; }
		static Regex roundBrackets = new Regex(@"\((.*?)\)");
		static Regex otherBrackets = new Regex(@"\{(.*?)\}|\[(.*?)\]");

	}

}

