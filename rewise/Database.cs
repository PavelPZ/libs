﻿/*
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

  public class FulltextContext : DbContext {
    public DbSet<BookDBModel.PhraseSrcWord> PhraseSrcWords { get; set; }
    public DbSet<BookDBModel.PhraseDestWord> PhraseDestWords { get; set; }
    public DbSet<BookDBModel.PhraseSrc> PhraseSrcs { get; set; }
    public DbSet<BookDBModel.PhraseDest> PhraseDests { get; set; }
    public DbSet<BookDBModel.Book> Books { get; set; }
    //https://github.com/aspnet/EntityFramework/issues/245 register fake dm_fts_parser entity
    public DbSet<FulltextDBModel.dm_fts_parser> dm_fts_parsers { get; set; }
    public DbSet<RewiseDBModel.SrcFactWord> SrcFactWords { get; set; }
    public DbSet<RewiseDBModel.DestFactWord> DestFactWords { get; set; }
    public DbSet<RewiseDBModel.UserFact> UserFacts { get; set; }
    public DbSet<RewiseDBModel.UserDict> UserDicts { get; set; }
    public DbSet<RewiseDBModel.User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["RewiseDatabase"].ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      BookDBModel.BookDBModelBuild.OnModelCreating(modelBuilder);
      RewiseDBModel.RewiseDBModelBuild.OnModelCreating(modelBuilder);
    }
  }
}
