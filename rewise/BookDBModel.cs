//https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell
//Add-Migration InitialCreate or v1 or v10 ...
//drop-database
//update-database 
//Inheritance: https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/inheritance
using LangsLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace FulltextDBModel {
  public class dm_fts_parser {
    [Key]
    public string display_term { get; set; }
  }

  public abstract class FtxWord {

    [Key]
    public long Id { get; set; }
    [MaxLength(PhraseWords.maxWordLen)]
    public string Word { get; set; } //fulltext word
    public byte Lang { get; set; } //jazyk knihy
  }

  public abstract class FtxPhrase {
    [Key]
    public int Id { get; set; }
    public string TextJSON { get; set; } //phrase JSON (PhraseLib.PhraseText)
    public byte Lang { get; set; }

    //design time
    public string Text;
  }
}

namespace SoundDBModel {
  public class Source {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<File> Sounds { get; set; } //breaked words
  }

  public class File {
    [Key]
    public int Id { get; set; }
    public byte Lang { get; set; }
    [MaxLength(PhraseWords.maxWordLen)]
    public string Text { get; set; } //text zvuku
    public string Path { get; set; }

    //*** relations
    public int SourceRef { get; set; }
    public Source Source { get; set; }
  }

  public static class BookDBModelBuild {
    public static void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.Entity<File>().HasIndex(p => new { p.SourceRef, p.Text, p.Lang });

      modelBuilder.Entity<Source>()
             .HasMany(c => c.Sounds)
             .WithOne(e => e.Source)
             .HasForeignKey(e => e.SourceRef)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
    }
  }
}

namespace BookDBModel {

  public class PhraseWord : FulltextDBModel.FtxWord {

    //*** relations
    public int PhraseRef { get; set; }
    public int BookRef { get; set; }

    public Phrase Phrase { get; set; }
    public Book Book { get; set; }
  }

  public class LocaleWord : FulltextDBModel.FtxWord {

    public byte BookSrcLang { get; set; } //jazyk knihy

    //*** relations
    public int PhraseRef { get; set; }
    public int BookRef { get; set; }

    public Locale Locale { get; set; }
    public Book Book { get; set; }

  }

  public class Phrase : FulltextDBModel.FtxPhrase {
    public byte LessonId { get; set; }
    public bool isPhraseWord { get; set; }

    //*** relations
    public int BookRef { get; set; }
    public Book Book { get; set; }
    public ICollection<Locale> Locales { get; set; } //my localizations
    public ICollection<PhraseWord> Words { get; set; } //breaked words
  }

  public class Locale : FulltextDBModel.FtxPhrase {

    //*** relations
    public int BookRef { get; set; }
    public Book Book { get; set; }
    public int PhraseRef { get; set; }
    public Phrase Phrase { get; set; } //my localizations
    public ICollection<LocaleWord> Words { get; set; } //breaked words
  }

  public class Book {
    public int Id { get; set; }
    public string Name { get; set; } //dict friendly name
    public byte Lang { get; set; }
    public string MetaData { get; set; } //BookMeta JSON: metadata, vcetne sturktury lekci
    public DateTime Imported { get; set; }
    //public string Lessons { get; set; } 

    public ICollection<Phrase> Phrases { get; set; }
    public ICollection<Locale> Locales { get; set; }
    public ICollection<PhraseWord> PhraseWords { get; set; }
    public ICollection<LocaleWord> LocaleWords { get; set; }

    //IGNORE
    public BookMeta Meta;
  }

  public static class BookDBModelBuild {

    public static void OnModelCreating(ModelBuilder modelBuilder) {

      modelBuilder.Entity<PhraseWord>().HasIndex(p => new { p.Word, p.Lang, p.BookRef });
      modelBuilder.Entity<LocaleWord>().HasIndex(p => new { p.Word, p.Lang, p.BookSrcLang, p.BookRef });

      modelBuilder.Entity<Phrase>()
        .HasMany(c => c.Words)
        .WithOne(e => e.Phrase)
        .HasForeignKey(e => e.PhraseRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Locale>().HasIndex(p => new { p.BookRef, p.Lang });
      modelBuilder.Entity<Locale>()
        .HasMany(c => c.Words)
        .WithOne(e => e.Locale)
        .HasForeignKey(e => e.PhraseRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Phrase>()
        .HasMany(c => c.Locales)
        .WithOne(e => e.Phrase)
        .HasForeignKey(e => e.PhraseRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Book>().HasIndex(p => new { p.Name, p.Lang });
      modelBuilder.Entity<Book>()
        .HasMany(c => c.Phrases)
        .WithOne(e => e.Book)
        .HasForeignKey(e => e.BookRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);
      modelBuilder.Entity<Book>()
        .HasMany(c => c.Locales)
        .WithOne(e => e.Book)
        .HasForeignKey(e => e.BookRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);
      modelBuilder.Entity<Book>()
        .HasMany(c => c.PhraseWords)
        .WithOne(e => e.Book)
        .HasForeignKey(e => e.BookRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);
      modelBuilder.Entity<Book>()
        .HasMany(c => c.LocaleWords)
        .WithOne(e => e.Book)
        .HasForeignKey(e => e.BookRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

    }
  }

  public class BookMeta {
    public string Publisher;
    public string Title;
    public string BookGroup;
    public string _path;
    public LessonMeta[] Lessons;
  }

  public class LessonMeta {
    public int Id;
    public string Title;
  }
}

namespace PhraseLib {

  public class PhraseText {
    public PhraseText() { }
    public PhraseText(StemmerBreaker.Runner runner, string text) {
      items = text.Split('|').Select(it => new PhraseTextItem(runner, it.Trim())).ToArray();
    }
    public PhraseTextItem[] items;

    //object array JSON code x encode
    public static PhraseText decode(string jsonArray) {
      //return parse(jsonArray);
      var objs  = JsonConvert.DeserializeObject<JArray>(jsonArray);
      return new PhraseText {
        items = objs.Select(v => PhraseTextItem.decode(v.ToArray())).ToArray()
      };
    }
    public string encode(bool formated = false) {
      //return stringify(formated);
      return JsonConvert.SerializeObject(items.Select(it => it.encode()).ToArray());
    }

    public IEnumerable<string> getFtxWords() {
      return items.SelectMany(it => it.getFtxWords()).Distinct();
    }

    //simply JSON code x encode for debugging
    public string stringify(bool formated = false) { return JsonConvert.SerializeObject(this, new JsonSerializerSettings { Formatting = formated ? Formatting.Indented : Formatting.None, DefaultValueHandling = DefaultValueHandling.Ignore }); }
    public static PhraseText parse(string json) { return JsonConvert.DeserializeObject<PhraseText>(json); }

  }

  public class PhraseTextItem {
    public PhraseTextItem() { }
    internal PhraseTextItem(StemmerBreaker.Runner runner, string _text) {
      text = _text;
      var st = 0; //0..in ftxText, 1..in bracket, 2..in sound text, 3..after sound text
      short idx = 0 /*act char*/; short brStart = -1; /*bracket start*/ short textStart = 0; /*normal text start*/
      var posLens = new List<TPosLen>();
      Action addBeforeBracket = () => {
        if (textStart == idx) return;
        var str = _text.Substring(textStart, idx - textStart);
        runner.wordBreak(str, (type, pos, len) => { if (type == StemmerBreaker.PutTypes.put) posLens.Add(new TPosLen() { pos = (short)(pos + textStart), len = len }); });
      };
      while (idx < _text.Length) {
        var ch = _text[idx];
        switch (ch) {
          case '(': addBeforeBracket(); brStart = idx; st = 1; break;
          case ')': if (st != 1) break; st = 0; posLens.Add(new TPosLen { pos = brStart, len = (short)(idx - brStart), type = ItemType.bracket }); textStart = (short)(idx + 1); break;
          case '{': addBeforeBracket(); text = text.Substring(0, idx); brStart = idx; st = 2; break;
          case '}': if (st != 2) break; soundText = _text.Substring(brStart + 1, idx - brStart - 1).Trim(); st = 3; break;
        }
        idx++;
      }
      if (st != 3) addBeforeBracket();
      wordIdxs = posLens.ToArray();
    }

    public string text;
    public string soundText;
    public TPosLen[] wordIdxs;

    public string getSoundText() {
      if (soundText!=null) return soundText;
      return rxBraket.Replace(text,"").Trim();
    }
    static Regex rxBraket = new Regex("\\(.*?\\)");

    internal IEnumerable<string> getFtxWords() {
      return wordIdxs.Where(pl => pl.type == ItemType.ok).Select(pl => text.Substring(pl.pos, pl.len).ToLower());
    }

    //object array JSON code x encode
    internal static PhraseTextItem decode(JToken[] v) {
      return new PhraseTextItem {
        text = v[0].ToString(),
        soundText = v[1].ToString(),
        wordIdxs = v.Skip(2).OfType<JArray>().Select(vv => TPosLen.decode(vv.ToArray())).ToArray()
      };
    }
    internal IEnumerable<Object> encode() {
      return new object[] { text, soundText ?? "" }.Concat(wordIdxs.Select(pl => pl.encode()));
    }
  }

  public enum ItemType { ok, wrong, bracket }

  public class TPosLen {
    public TPosLen() { }
    public short pos;
    public short len;
    public ItemType type; //word type

    //object array JSON code x encode
    internal static TPosLen decode(JToken[] v) {
      return new TPosLen {
        pos = v[0].Value<short>(),
        len = v[1].Value<short>(),
        type = (ItemType)(byte)v[2].Value<byte>()
      };
    }
    internal IEnumerable<Object> encode() {
      return new object[] { pos, len, (byte)type };
    }
  }

  public static class Test {
    public static void Run() {
      var text = new PhraseText(new StemmerBreaker.Runner(Langs.cs_cz), @"
        Ahoj, jak (ako) se máš (máte)? {ahoj máš} ignored | Ahoj, jak (ako) se máš (máte)?  | já dobře 
");
      var str = text.encode(true);
      var str2 = PhraseText.decode(str).encode();
      var sounds = text.items.Select(it => it.getSoundText()).Aggregate((r, i) => r + "|" + i);
      str = null;
    }
  }

}

