//Data from registry: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ContentIndex\Language
using LangsLib;
using STALib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace StemmerBreaker {

  public class RunStemmer : RunObject<List<string>> {

    public TaskCompletionSource<List<string>> tcs { get; set; }
    public void doRun() { tcs.TrySetResult(Run()); }

    public RunStemmer(Langs lng, string text) {
      this.lng = lng; this.text = text;
    }
    Langs lng; string text;

    public List<string> Run() {
      return STAStemm(lng, text);
    }

    public static Task<List<string>> Stemm(Langs lng, string text) {
      return STALib.Lib.Run(new RunStemmer(lng, text));
    }

    public static List<string> STAStemm(Langs lng, string text) {
      return Runner.getRunner(lng).stemm(text);
    }
  }

  public class RunBreaker : RunObject<List<TPosLen>> {

    public TaskCompletionSource<List<TPosLen>> tcs { get; set; }
    public void doRun() { tcs.TrySetResult(Run()); }

    public RunBreaker(Langs lng, string text) { this.lng = lng; this.text = text; }
    Langs lng; string text;

    public List<TPosLen> Run() { return STAWordBreak(lng, text); }

    public static Task<List<TPosLen>> WordBreak(Langs lng, string text) {
      return STALib.Lib.Run(new RunBreaker(lng, text));
    }

    public static List<TPosLen> STAWordBreak(Langs lng, string text) {
      return Runner.getRunner(lng).wordBreak(text);
    }

  }

  public enum PutTypes { put, alt, EOW, EOS, EOP, EOC, phraseStart, phraseEnd, phrase, phraseSmall, error }

  public struct StemItem { public PutTypes type; public string word; }

  public class Runner {

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

    public List<TPosLen> wordBreak(string text) {
      var res = new List<TPosLen>();
      wordBreak(text, (type, pos, len) => { if (type == PutTypes.put) res.Add(new TPosLen() { Pos = pos, Len = len }); });
      return res;
    }

    public void wordBreak(string text, Action<PutTypes, Int16, Int16> onPutWord) {
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

    public List<string> stemm(string word, List<string> res = null) {
      stemm(word, (type, w) => {
        //if (type != PutTypes.put) return;
        if (res == null) res = new List<string>();
        res.Add(w);
      });
      return res;
    }

    public void stemm(string word, Action<PutTypes, string> onPutWord) {
      if (stemmer == null && item != null) stemmer = item.getStemmer();
      if (stemmer == null) { onPutWord(PutTypes.put, word); return; }
      StemSink sink = new StemSink(onPutWord);
      stemmer.GenerateWordForms(word, word.Length, sink);
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

      var xxx = res.Select(w => w).Aggregate((r, i) => r + "\r\n" + i);
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

    //void IDisposable.Dispose() {
    //	if (breaker != null) Marshal.ReleaseComObject(breaker);
    //	if (stemmer != null) Marshal.ReleaseComObject(stemmer);
    //	breaker = null; stemmer = null;
    //}

    const uint WBREAK_E_END_OF_TEXT = 0x80041780;

    class StemSink : IWordFormSink {
      public StemSink(Action<PutTypes, string> onPutWord) { this.onPutWord = onPutWord; }
      Action<PutTypes, string> onPutWord;
      public void PutAltWord(string pwcInBuf, int cwc) { onPutWord(PutTypes.alt, pwcInBuf.Substring(0, cwc)); }
      public void PutWord(string pwcInBuf, int cwc) { onPutWord(PutTypes.put, pwcInBuf.Substring(0, cwc)); }
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
      public BreakSink(Action<PutTypes, Int16, Int16> onPutWord) { this.onPutWord = onPutWord; }
      Action<PutTypes, Int16, Int16> onPutWord;
      public void PutWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) {
        if (cwcSrcPos > 255 || cwcSrcLen > 127) throw new Exception("class BreakSink : IWordSink: cwcSrcPos > 255 || cwcSrcLen > 127");
        onPutWord(PutTypes.put, (byte)cwcSrcPos, (sbyte)cwcSrcLen);
      }
      //https://tlzprgmr.wordpress.com/2008/02/19/ifilters-part-2-using-word-breakers/: "Date: 2/19/2008" results in word: Date, alt: 2/19/2008, word: DD20080219
      public void PutAltWord(int cwc, string pwcInBuf, int cwcSrcLen, int cwcSrcPos) { onPutWord(PutTypes.alt, (byte)cwcSrcPos, (sbyte)cwcSrcLen); }
      public void StartAltPhrase() { onPutWord(PutTypes.phraseStart, 0, -1); }
      public void EndAltPhrase() { onPutWord(PutTypes.phraseEnd, 0, -1); }
      public void PutBreak(WORDREP_BREAK_TYPE breakType) {
        switch (breakType) {
          case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOC: onPutWord(PutTypes.EOC, 0, -1); break;
          case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOP: onPutWord(PutTypes.EOP, 0, -1); break;
          case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOS: onPutWord(PutTypes.EOS, 0, -1); break;
          case WORDREP_BREAK_TYPE.WORDREP_BREAK_EOW: onPutWord(PutTypes.EOW, 0, -1); break;
          default: onPutWord(PutTypes.error, 0, -1); break;
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
