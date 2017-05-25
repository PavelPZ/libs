using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using LangsLib;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Fulltext {

  public class PhraseWord {

		public const int maxWordLen = 24;

    [Key]
    public int Id { get; set; } //internal unique ID
    [MaxLength(maxWordLen)]
    public string Word { get; set; } 
		public Int64 PhraseId { get; set; } //ID of phrase, containing word. Could be Hash64 of string, identifying phrase in its source repository.
		[MaxLength(2)]
		public byte[] DictId; //First byte is Dict Lang, second is Phrase Lang. If Phrase Lang==_ => Phrase means source, otherwise translation.
	}

  public class BloggingContext : DbContext {
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

		public void insert(Int64 phraseId, PhraseSide phraseSide /*dict and its side, e.g. czech part of English-Czech dict*/, PhraseWords oldText /*null => insert, else update*/, ref PhraseWords newText /*null => delete, else update or insert. Returns newText.Idxs*/) {
			var ctx = new BloggingContext();
			if (newText == null) { //DELETE
				ctx.Database.ExecuteSqlCommand("delete PhraseWords where PhraseId = {0}", new object[] { phraseId });
				return;
		  }
			newText.Idxs = spellCheckedWordBreak(phraseSide.langOfText(), newText.Text);
			var news = newText.getWords();
			if (oldText == null) { //insert
				foreach (var w in news) ctx.PhraseWords.Add(new PhraseWord() { DictId = phraseSide.getDictId(), Word = w.Substring(0, PhraseWord.maxWordLen), PhraseId = phraseId });
			} else { //update
				var olds = oldText.getWords();
				var oldsDB = ctx.PhraseWords.Where(pw => pw.PhraseId == phraseId && pw.DictId.Equals(phraseSide.getDictId())).ToArray();
				HashSet<string> boths = new HashSet<string>(news.Intersect(news).Select(w => w.Substring(PhraseWord.maxWordLen)));
				foreach (var w in oldsDB.Where(ww => !boths.Contains(ww.Word))) ctx.PhraseWords.Remove(w);
				foreach (var w in news.Where(ww => !boths.Contains(ww))) ctx.PhraseWords.Add(new PhraseWord() { DictId = phraseSide.getDictId(), Word = w.Substring(0, 24), PhraseId = phraseId });
			}
			ctx.SaveChanges();
			//stemmer.wordBreak();
    }
    public string[] searchPhrase(PhraseSide dictSide, string text) {
      return null; //matching phrase ids
    }

		public static LangsLib.PhraseWord[] spellCheckedWordBreak(Langs lang, string text) {
			var stemmer = new StemmerBreaker.Runner(lang);
			Langs[] langs = new Langs[] { Langs.cs_cz, Langs.de_de, Langs.ru_ru, Langs.pt_pt, Langs.sk_sk, Langs.fr_fr, Langs.it_it, Langs.es_es};
			//for (var idx = 0; idx < 1000; idx++) {
			//	var s = ""; for (var i = 0; i < idx; i++) s += "x";
			//	var res = await SpellChecker.SpellLang.Check(langs[idx % 8], new string[] {
			//		//s + " ahoj" + " jak" + " se" + " máš"
			//		s, "ahoj", "jak", "se", "máš"
			//	});
			//}
			Parallel.ForEach(Enumerable.Range(0,1000), async idx => {
				var s = ""; for (var i = 0; i < idx; i++) s += "x";
				var res = await SpellChecker.RunSpellCheck.Check(langs[idx % 8], new string[] {
					//s + " ahoj" + " jak" + " se" + " máš"
					s, "ahoj", "jak", "se", "máš"
				});
			});
			return null;
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
      var ctx = new BloggingContext();
      ctx.recreate();
      //var all = ctx.Blogs.ToArray();
    }
  }
}
