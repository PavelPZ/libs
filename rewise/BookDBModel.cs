//https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell
//Add-Migration InitialCreate or v1 or v10 ...
//drop-database
//update-database 
//Inheritance: https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/inheritance
using LangsLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

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
    public string Text { get; set; }
    public byte[] TextIdxs { get; set; } //word breking result (zakodovane <pos, len> array). TODO: len musí být max. 127 :-(
    public byte Lang { get; set; }
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
    public bool isPhraseWord{ get; set; }

    //*** relations
    public int BookRef { get; set; }
    public Book Book { get; set; }
    public ICollection<Locale> Locales { get; set; } //my localizations
    public ICollection<PhraseWord> Words { get; set; } //breaked words
  }

  public class Locale : FulltextDBModel.FtxPhrase {

    //*** relations
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
    public ICollection<PhraseWord> Words { get; set; }

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

      modelBuilder.Entity<Book>()
        .HasMany(c => c.Phrases)
        .WithOne(e => e.Book)
        .HasForeignKey(e => e.BookRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

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

//namespace Fulltext2 {

//  public class PhraseWord {

//    [Key]
//    public int Id { get; set; }
//    [MaxLength(PhraseWords.maxWordLen)]
//    public string Word { get; set; } //fulltext word

//    //*** relations
//    public int PhraseRef { get; set; } //ID of phrase, containing word. Could be Hash64 of string, identifying phrase in its source repository.
//    public int DictRef { get; set; } //ID of dictionary

//    public Phrase Phrase { get; set; }
//    public Book Dict { get; set; }

//  }

//  public class Phrase {

//    //public const int maxPhraseBaseLen = 128;

//    [Key]
//    public int Id { get; set; }
//    public string Text { get; set; }
//    public byte[] TextIdxs { get; set; } //word breking result (zakodovane <pos, len> array). TODO: len musí být max. 127 :-(

//    //*** relations
//    public int? SrcRef { get; set; } //source phrase. ==null iff source part of phrase
//    public int DictRef { get; set; }

//    public ICollection<PhraseWord> Words { get; set; } //word breaked words
//    public Book Dict { get; set; }

//    public ICollection<Phrase> Dests { get; set; } //my localizations
//    public Phrase Src { get; set; } //my source


//  }

//  public class Book {
//    public int Id { get; set; }
//    public string Name { get; set; } //dict friendly name
//    public string Lessons { get; set; } //struktura lekci
//    public byte Lang { get; set; }
//    public DateTime Imported { get; set; }

//    //*** relations
//    public int? SrcRef { get; set; } //source dict. ==null iff source part of dictionary
//    public int? UserRef { get; set; }
//    public int? TextBookRef { get; set; }

//    public ICollection<Phrase> Phrases { get; set; }
//    public ICollection<PhraseWord> Words { get; set; }
//    public User User { get; set; }
//    public TextBook TextBook { get; set; }
//    public ICollection<Book> Dests { get; set; } //my localizations
//    public Book Src { get; set; } //my source
//  }

//  public class User {
//    public int Id { get; set; }

//    //*** relations
//    public ICollection<Book> Dicts { get; set; }
//  }

//  public class TextBook {
//    public int Id { get; set; }

//    //*** relations
//    public ICollection<Book> Dicts { get; set; }
//  }



//}
