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

    public UserFact Fact { get; set; }
    public UserDict Dict { get; set; }
  }

  public class SrcFactWord: FactWord { }

  public class DestFactWord : FactWord { }

  public class UserFact {
    [Key]
    public int Id { get; set; } //internal unique ID
    public string Data { get; set; } //JSON data s faktem

    public int DictRef { get; set; }
    public UserDict Dict { get; set; }
    public ICollection<SrcFactWord> SrcWords { get; set; }
    public ICollection<DestFactWord> DestWords { get; set; }
  }

  public class UserDict {
    [Key]
    public int Id { get; set; } //internal unique ID
    public byte SrcLang { get; set; }
    public byte DestLang { get; set; }

    public int UserRef { get; set; }
    public User User { get; set; }
    public ICollection<UserFact> Facts { get; set; }
    public ICollection<SrcFactWord> SrcWords { get; set; }
    public ICollection<DestFactWord> DestWords { get; set; }
  }

  public class User {
    [Key]
    public int Id { get; set; } //internal unique ID
    public ICollection<UserDict> Dicts { get; set; }
  }

  public static class RewiseDBModelBuild {

    public static void OnModelCreating(ModelBuilder modelBuilder) {

      modelBuilder.Entity<SrcFactWord>().HasIndex(p => new { p.Word, p.DictRef });
      modelBuilder.Entity<DestFactWord>().HasIndex(p => new { p.Word, p.DictRef });

      modelBuilder.Entity<UserFact>()
        .HasMany(c => c.SrcWords)
        .WithOne(e => e.Fact)
        .HasForeignKey(e => e.FactRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<UserFact>()
        .HasMany(c => c.DestWords)
        .WithOne(e => e.Fact)
        .HasForeignKey(e => e.FactRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<UserDict>()
        .HasMany(c => c.Facts)
        .WithOne(e => e.Dict)
        .HasForeignKey(e => e.DictRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<UserDict>()
        .HasMany(c => c.SrcWords)
        .WithOne(e => e.Dict)
        .HasForeignKey(e => e.DictRef)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<UserDict>()
        .HasMany(c => c.DestWords)
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

  //Javascript definice faktu
  public class Fact {
    //rewise

    //data
    public Side srcData;
    public Side destData;
  }
  public class Side {
    public Langs lang;
    public string text;
    public int sound;
  }

}