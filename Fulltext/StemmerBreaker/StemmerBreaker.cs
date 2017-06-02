//Data from registry: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ContentIndex\Language
using LangsLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using STALib;
using System.Threading.Tasks;

namespace StemmerBreaker {

	public class RunStemmer : RunObject {

		public RunStemmer(Langs lng) {
		}

		public override object Run() {
			return null;
		}
	}

	public class RunBreaker : RunObject {

		public RunBreaker(Langs lng, string text) {
			this.lng = lng; this.text = text;
		}
		Langs lng; string text;

		public override object Run() { return STAWordBreak(lng,text); }

		public static Task<Object> WordBreak(Langs lng, string text) {
			var res = STALib.Lib.Run(new RunBreaker(lng, text)) as Task<Object>;
			return res;
		}

		public static List<TPosLen> STAWordBreak(Langs lng, string text) {
			return Runner.getRunner(lng).wordBreak(text);
		}
	}

	public enum PutTypes { put, alt, EOW, EOS, EOP, EOC, phraseStart, phraseEnd, phrase, phraseSmall, error }

	public struct Put {
		public Put(PutTypes type, string word) {
			this.type = type;
			this.word = word;
			srcPos = -1; srcLen = -1;
		}
		public Put(PutTypes type, int srcPos = -1, int srcLen = -1) {
			this.type = type;
			this.srcLen = srcLen;
			this.srcPos = srcPos;
			word = null;
		}
		public PutTypes type; public string word; public int srcPos; public int srcLen;
	}

	public class Runner : IDisposable {

		static bool initiated;
		static Dictionary<Langs, Runner> runners = new Dictionary<Langs, Runner>();
		public static Runner getRunner(Langs lng) {
			Runner rn;
			if (!Runner.runners.TryGetValue(lng, out rn)) runners[lng] = rn = new Runner(lng);
			return rn;
		}

		public static void init() {
			init(ConfigurationManager.AppSettings["FulltextDlls"]);
		}
		public static void init(string libBasicPath) {
			if (initiated) return;
			initiated = true;
			Lib.initFactories(libBasicPath);
		}

		public Runner(Langs lang) {
			if (!initiated) init();
			this.lang = lang;
			Lib.items.TryGetValue(lang, out item);
		}

		IStemmer stemmer; IWordBreaker breaker; Langs lang; Lib item;

		//public List<Put> wordBreak(string text, bool wordsOnly = true, List<Put> res = null) {
		//	if (string.IsNullOrEmpty(text)) return res;
		//	if (res == null) res = new List<Put>();
		//	if (breaker == null && item != null) breaker = item.getWordBreaker();
		//	if (breaker == null) breaker = Lib.items[Langs._].getWordBreaker(); //neutral word breaker
		//	BreakSink cws = new BreakSink(res, wordsOnly);
		//	//CPhraseSink cps = new CPhraseSink(res);
		//	TEXT_SOURCE pTextSource = new TEXT_SOURCE();
		//	pTextSource.pfnFillTextBuffer += fillTextBuffer;
		//	pTextSource.awcBuffer = text;
		//	pTextSource.iCur = 0;
		//	pTextSource.iEnd = text.Length;
		//	breaker.BreakText(ref pTextSource, cws, /*cps*/null);
		//	return res;
		//}

		public List<TPosLen> wordBreak(string text) {
			var res = new List<TPosLen>();
			wordBreak(text, (type, pos, len) => { if (type == PutTypes.put) res.Add(new TPosLen() { Pos = pos, Len = len }); });
			return res;
		}

		public void wordBreak(string text, Action<PutTypes, int, int> onPutWord) {
			if (string.IsNullOrEmpty(text)) return;
			if (breaker == null && item != null) breaker = item.getWordBreaker();
			if (breaker == null) breaker = Lib.items[Langs._].getWordBreaker(); //neutral word breaker
			if (breaker == null)
				throw new Exception("breaker == null");
			BreakSink cws = new BreakSink(onPutWord);
			//CPhraseSink cps = new CPhraseSink(res);
			TEXT_SOURCE pTextSource = new TEXT_SOURCE();
			pTextSource.pfnFillTextBuffer += fillTextBuffer;
			pTextSource.awcBuffer = text;
			pTextSource.iCur = 0;
			pTextSource.iEnd = text.Length;
			breaker.BreakText(ref pTextSource, cws, /*cps*/null);
		}

		public List<Put> stemm(string word, List<Put> res = null) {
			if (res == null) res = new List<Put>();
			if (stemmer == null && item != null) stemmer = item.getStemmer();
			if (stemmer == null) { res.Add(new Put(PutTypes.put, word)); return res; }
			StemSink sink = new StemSink(res);
			stemmer.GenerateWordForms(word, word.Length, sink);
			return res;
		}

		public static void test() {
			init();

			//foreach (var kv in Enum.GetValues(typeof(Langs))) {
			//	var st = new Runner((Langs)kv);
			//	var r = st.stemm("xxx");
			//	r = st.wordBreak("xxx yyy");
			//}
			var stemm = new Runner(Langs.de_de);
			var txt = "Einem Pferd die Sporen geben?";
			stemm.wordBreak(txt, (type, start, len) => {
				if (/*type != PutTypes.alt &&*/ type != PutTypes.put) return;
				var w = txt.Substring(start, len);
				w = null;
			});
			var res = stemm.stemm("Pferd");
			res = stemm.stemm("Sporen");
			res = stemm.stemm("Bücherregal");
			res = stemm.stemm("bucherregal").Concat(stemm.stemm("buch")).Concat(stemm.stemm("regal")).ToList();

			var xxx = res.Select(w => w.word).Aggregate((r, i) => r + "\r\n" + i);
			res = null;


			//var stemm = new Runner(Langs.cs_cz);
			//var res = stemm.wordBreak("Jak se u vás mají?");
			//res = stemm.stemm("udělají");

			//stemm = new Runner(Langs.ar_sa);
			//res = stemm.stemm("XXX");
			//res = stemm.wordBreak("XXX YYY ZZZ HHH");

			//stemm = new Runner(Langs._);
			//res = stemm.stemm("XXX");
			//res = stemm.wordBreak("XXX YYY ZZZ HHH");
		}

		//**************** private
		static uint fillTextBuffer(ref TEXT_SOURCE ts) { return WBREAK_E_END_OF_TEXT; }

		void IDisposable.Dispose() {
			if (breaker != null) Marshal.ReleaseComObject(breaker);
			if (stemmer != null) Marshal.ReleaseComObject(stemmer);
			breaker = null; stemmer = null;
		}

		const uint WBREAK_E_END_OF_TEXT = 0x80041780;

		class StemSink : IWordFormSink {
			public StemSink(List<Put> data) { this.data = data; }
			List<Put> data;
			public void PutAltWord(string pwcInBuf, int cwc) { data.Add(new Put(PutTypes.alt, pwcInBuf.Substring(0, cwc))); }
			public void PutWord(string pwcInBuf, int cwc) { data.Add(new Put(PutTypes.put, pwcInBuf.Substring(0, cwc))); }
		}

		//class BreakSink : IWordSink {
		//	public BreakSink(List<Put> data, bool wordsOnly) { this.data = data; this.wordsOnly = wordsOnly; }
		//	List<Put> data;
		//	bool wordsOnly;
		//	public void PutWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) { data.Add(new Put(PutTypes.put, cwcSrcPos, cwcSrcLen)); }
		//	public void PutAltWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) { data.Add(new Put(PutTypes.alt, cwcSrcPos, cwcSrcLen)); }
		//	public void StartAltPhrase() { if (wordsOnly) return; data.Add(new Put(PutTypes.phraseStart, null)); }
		//	public void EndAltPhrase() { if (wordsOnly) return; data.Add(new Put(PutTypes.phraseEnd, null)); }
		//	public void PutBreak(WORDREP_BREAK_TYPE breakType) {
		//		if (wordsOnly) return;
		//		switch (breakType) {
		//			case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOC: data.Add(new Put(PutTypes.EOC, null)); break;
		//			case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOP: data.Add(new Put(PutTypes.EOP, null)); break;
		//			case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOS: data.Add(new Put(PutTypes.EOS, null)); break;
		//			case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOW: data.Add(new Put(PutTypes.EOW, null)); break;
		//			default: data.Add(new Put(PutTypes.error, null)); break;
		//		}
		//	}
		//}

		class BreakSink : IWordSink {
			public BreakSink(Action<PutTypes, int, int> onPutWord) { this.onPutWord = onPutWord; }
			Action<PutTypes, int, int> onPutWord;
			public void PutWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) { onPutWord(PutTypes.put, cwcSrcPos, cwcSrcLen); }
			public void PutAltWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) { onPutWord(PutTypes.alt, cwcSrcPos, cwcSrcLen); }
			public void StartAltPhrase() { onPutWord(PutTypes.phraseStart, -1, -1); }
			public void EndAltPhrase() { onPutWord(PutTypes.phraseEnd, -1, -1); }
			public void PutBreak(WORDREP_BREAK_TYPE breakType) {
				switch (breakType) {
					case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOC: onPutWord(PutTypes.EOC, -1, -1); break;
					case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOP: onPutWord(PutTypes.EOP, -1, -1); break;
					case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOS: onPutWord(PutTypes.EOS, -1, -1); break;
					case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOW: onPutWord(PutTypes.EOW, -1, -1); break;
					default: onPutWord(PutTypes.error, -1, -1); break;
				}
			}
		}
		//class CPhraseSink : IPhraseSink {
		//	public CPhraseSink(List<Put> data) { this.data = data; }
		//	List<Put> data;
		//	public void PutSmallPhrase(string pwcNoun, int cwcNoun, string pwcModifier, int cwcModifier, int ulAttachmentType) { data.Add(new Put(PutTypes.phrase, pwcNoun.Substring(0, cwcNoun), -1, pwcModifier.Substring(0, cwcModifier))); }
		//	public void PutPhrase(string pwcPhrase, int cwcPhrase) { data.Add(new Put(PutTypes.phrase, pwcPhrase.Substring(0, cwcPhrase))); }
		//}

	}
}
