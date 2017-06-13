//https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell
//Add-Migration InitialCreate or v1 or v10 ...
//drop-database
//update-database 
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using LangsLib;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using SpellChecker;
using STALib;
using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Fulltext {

	public class PhraseWord {

		public const string PhraseIdName = "PhraseId";
		public const string SrcLangName = "SrcLang";
		public const string DestLangName = "DestLang";

		[Key]
		public int Id { get; set; } //internal unique ID
		[MaxLength(maxWordLen), Required]
		public string Word { get; set; }
		public int PhraseRef { get; set; } //ID of phrase, containing word. Could be Hash64 of string, identifying phrase in its source repository.
		public byte SrcLang { get; set; }
		public byte DestLang { get; set; }

		public Phrase Phrase { get; set; }
	}

	public class Phrase {

		public const int maxPhraseBaseLen = 128;

		[Key]
		public int Id { get; set; } //internal unique ID
		[Required]
		public string Text { get; set; }
		[Required]
		public byte[] TextIdxs { get; set; } //word breking and stemming result (<pos, len> array)
		public byte SrcLang { get; set; }
		public byte DestLang { get; set; }
		public int? SrcRef { get; set; }

		[MaxLength(maxPhraseBaseLen), Required]
		public string Base { get; set; } //normalized text for duplicity search

		public ICollection<PhraseWord> Words { get; set; } //stemmed words
		public ICollection<Phrase> Dests { get; set; }
		public Phrase Src { get; set; }
		public Dict Dict { get; set; }
	}

	public class Dict {
		public int Id { get; set; }
		public string Name { get; set; }
		public byte SrcLang { get; set; }
		public DateTime Imported { get; set; }
		public ICollection<Phrase> Phrases { get; set; } 
	}



	//fake entity for dm_fts_parser result, see //https://github.com/aspnet/EntityFramework/issues/245 register entity
	public class dm_fts_parser {
		[Key]
		public string display_term { get; set; }
	}

	public class FulltextContext : DbContext {
		public DbSet<PhraseWord> PhraseWords { get; set; }
		public DbSet<Phrase> Phrases { get; set; }
		public DbSet<Dict> Dicts { get; set; }
		//https://github.com/aspnet/EntityFramework/issues/245 register fake dm_fts_parser entity
		public DbSet<dm_fts_parser> dm_fts_parser { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["FulltextDatabase"].ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			//http://www.learnentityframeworkcore.com/configuration/one-to-many-relationship-configuration
			//modelBuilder.Entity<PhraseWord>().HasIndex(p => p.Word);
			modelBuilder.Entity<PhraseWord>().HasIndex(p => new { p.Word, p.SrcLang, p.DestLang });

			modelBuilder.Entity<Phrase>()
				.HasMany(c => c.Words)
				.WithOne(e => e.Phrase)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Phrase>()
				.HasMany(c => c.Dests)
				.WithOne(e => e.Src)
				.HasForeignKey(b => b.SrcRef)
				.OnDelete(DeleteBehavior.Restrict); //https://stackoverflow.com/questions/22681352/entity-framework-6-code-first-cascade-delete-on-self-referencing-entity
			modelBuilder.Entity<Phrase>().HasIndex(p => new { p.Base, p.SrcLang, p.DestLang });

			modelBuilder.Entity<Dict>().HasIndex(p => p.Name);
			modelBuilder.Entity<Dict>()
				.HasMany(c => c.Phrases)
				.WithOne(e => e.Dict)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		}

		public void recreate() {
			Database.EnsureDeleted();
			Database.EnsureCreated();
		}


	}

}
