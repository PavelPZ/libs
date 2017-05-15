using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using Langs;



namespace Fulltext {
  public class PhraseWord {
    [Key]
    public int Id { get; set; } //internal unique ID
    [MaxLength(26)]
    public string Word { get; set; } // first two chars has ord value of 32 plus LANG code. First LANG code is DictLang, second is Phrase Lang. If DictLang==Lang => Phrase means source, otherwise translation.
    public string PhraseId { get; set; } //ID of phrase
  }

  //public class Phrase {
  //  [Key]
  //  public int Id { get; set; } //unique ID
  //  public string Text { get; set; }
  //  public byte[] WordIdxs { get; set; } //pos a length of word in the Text. Word Breaking result.
  //}
  
  public class BloggingContext : DbContext {
    public DbSet<PhraseWord> PhraseWords { get; set; }
    //public DbSet<Phrase> Phrases { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(@"Data Source=PZ-W8VIRTUAL\SQLEXPRESS;Initial Catalog=test;Integrated Security=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.Entity<PhraseWord>().HasIndex(p => p.Word);
    }

    public void recreate() {
      Database.EnsureDeleted();
      Database.EnsureCreated();
    }
    public void insert(string phraseId, PhraseSide dictSide /*dict and its side, e.g. czech part of English-Czech dict*/, string oldText /*null => insert, else update*/, byte[] oldIdxs, string newText /*null => delete else update or insert*/, out byte[] newIdxs) {
      newIdxs = null;
    }
    public string[] searchPhrase(PhraseSide dictSide, string text) {
      return null; //matching phrase ids
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
