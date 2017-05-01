using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace Fulltext
{
	public class PhraseWord
	{
		[Key]
		public int Id { get; set; } //internal unique ID
		[MaxLength(26)]
		public string Word { get; set; } // first two chars has ord value of 32 plus LANG code. First LANG code is DictLang, second is Phrase Lang. If DictLang==Lang => Phrase means source, otherwise translation.
		public int PhraseId { get; set; } //ID of phrase
	}

	public class Phrase
	{
		[Key]
		public int Id { get; set; } //unique ID
		public string Text { get; set; }
		public byte[] WordIdxs { get; set; } //pos a length of word in the Text. Word Breaking result.
	}

	public class BloggingContext : DbContext
	{
		public DbSet<PhraseWord> PhraseWords { get; set; }
		public DbSet<Phrase> Phrases { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(@"Data Source=PZ-W8VIRTUAL\SQLEXPRESS;Initial Catalog=test;Integrated Security=True;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PhraseWord>().HasIndex(p => p.Word);
		}

		public void recreate()
		{
			Database.EnsureDeleted();
			Database.EnsureCreated();
		}
	}

	public class Class1
	{
		protected void Page_Load()
		{
			var ctx = new BloggingContext();
			ctx.recreate();
			//var all = ctx.Blogs.ToArray();
		}
	}
}
