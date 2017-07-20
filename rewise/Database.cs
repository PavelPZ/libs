/*
Add-Migration InitialCreate or v1 or v10 ...
drop-database
update-database 
 */
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database {

  public class RewiseContext : DbContext {
    public DbSet<BookDBModel.PhraseWord> BookPhraseWords { get; set; }
    public DbSet<BookDBModel.LocaleWord> BookLocaleWords { get; set; }
    public DbSet<BookDBModel.Phrase> BookPhrases { get; set; }
    public DbSet<BookDBModel.Locale> BookLocales { get; set; }
    public DbSet<BookDBModel.Book> Books { get; set; }

    //https://github.com/aspnet/EntityFramework/issues/245 register fake dm_fts_parser entity
    public DbSet<FulltextDBModel.dm_fts_parser> dm_fts_parsers { get; set; }

    public DbSet<RewiseDBModel.PhraseWord> UserPhraseWords { get; set; }
    public DbSet<RewiseDBModel.LocaleWord> UserLocaleWords { get; set; }
    public DbSet<RewiseDBModel.Fact> UserFacts { get; set; }
    public DbSet<RewiseDBModel.Vocabulary> UserDicts { get; set; }
    public DbSet<RewiseDBModel.User> Users { get; set; }

    public DbSet<SoundDBModel.Source> SoundSources { get; set; }
    public DbSet<SoundDBModel.File> SoundFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["RewiseDatabase"].ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      BookDBModel.BookDBModelBuild.OnModelCreating(modelBuilder);
      RewiseDBModel.RewiseDBModelBuild.OnModelCreating(modelBuilder);
      SoundDBModel.BookDBModelBuild.OnModelCreating(modelBuilder);
    }
  }
}
