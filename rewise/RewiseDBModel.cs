using LangsLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace RewiseDBModel {

  public abstract class FactWord : FulltextDBModel.FtxWord {

    //*** relations
    public int FactRef { get; set; } //ID of PhraseLow, containing word
    public int DictRef { get; set; } //ID of dictionary

    public Fact Fact { get; set; }
    public Vocabulary Dict { get; set; }
  }

  public class PhraseWord: FactWord { }

  public class LocaleWord : FactWord { }

  public class WordError {
    [Key]
    public long Id { get; set; } 
    public byte Type { get; set; }
  }

  public class Fact  {
    [Key]
    public int Id { get; set; }

    public string PhraseText { get; set; }
    public byte[] PhraseTextIdxs { get; set; } 
    public string LocaleText { get; set; }
    public byte[] LocaleTextIdxs { get; set; } 

    public string Data { get; set; } //JSON data s faktem

    public int DictRef { get; set; }
    public Vocabulary Dict { get; set; }
    public ICollection<PhraseWord> PhraseWords { get; set; }
    public ICollection<LocaleWord> LocaleWords { get; set; }
  }

  public class Vocabulary {
    [Key]
    public int Id { get; set; } 
    public byte PhraseLang { get; set; }
    public byte LocaleLang { get; set; }

    public int UserRef { get; set; }
    public User User { get; set; }
    public ICollection<Fact> Facts { get; set; }
    public ICollection<PhraseWord> PhraseWords { get; set; }
    public ICollection<LocaleWord> LocaleWords { get; set; }
  }

  public class User {
    [Key]
    public int Id { get; set; } 
    public ICollection<Vocabulary> Dicts { get; set; }
  }

  public static class RewiseDBModelBuild {

    public static void OnModelCreating(ModelBuilder modelBuilder) {

      modelBuilder.Entity<PhraseWord>().HasIndex(p => new { p.Word, p.DictRef });
      modelBuilder.Entity<LocaleWord>().HasIndex(p => new { p.Word, p.DictRef });

      modelBuilder.Entity<Fact>()
        .HasMany(c => c.PhraseWords)
        .WithOne(e => e.Fact)
        .HasForeignKey(e => e.FactRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Fact>()
        .HasMany(c => c.LocaleWords)
        .WithOne(e => e.Fact)
        .HasForeignKey(e => e.FactRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Vocabulary>()
        .HasMany(c => c.Facts)
        .WithOne(e => e.Dict)
        .HasForeignKey(e => e.DictRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Vocabulary>()
        .HasMany(c => c.PhraseWords)
        .WithOne(e => e.Dict)
        .HasForeignKey(e => e.DictRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Vocabulary>()
        .HasMany(c => c.LocaleWords)
        .WithOne(e => e.Dict)
        .HasForeignKey(e => e.DictRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<User>()
        .HasMany(c => c.Dicts)
        .WithOne(e => e.User)
        .HasForeignKey(e => e.UserRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);
    }
  }

  ////Javascript definice faktu
  //public class Fact {
  //  //rewise

  //  //data
  //  public Side srcData;
  //  public Side destData;
  //}
  //public class Side {
  //  public Langs lang;
  //  public string text;
  //  public int sound;
  //}

}

