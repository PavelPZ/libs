using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Database;

namespace rewise.Migrations
{
    [DbContext(typeof(RewiseContext))]
    partial class RewiseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BookDBModel.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Imported");

                    b.Property<byte>("Lang");

                    b.Property<string>("MetaData");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("Name", "Lang");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("BookDBModel.Locale", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookRef");

                    b.Property<byte>("Lang");

                    b.Property<int>("PhraseRef");

                    b.Property<string>("TextJSON");

                    b.HasKey("Id");

                    b.HasIndex("PhraseRef");

                    b.HasIndex("BookRef", "Lang");

                    b.ToTable("BookLocales");
                });

            modelBuilder.Entity("BookDBModel.LocaleWord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookRef");

                    b.Property<byte>("BookSrcLang");

                    b.Property<byte>("Lang");

                    b.Property<int>("PhraseRef");

                    b.Property<string>("Word")
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("BookRef");

                    b.HasIndex("PhraseRef");

                    b.HasIndex("Word", "Lang", "BookSrcLang", "BookRef");

                    b.ToTable("BookLocaleWords");
                });

            modelBuilder.Entity("BookDBModel.Phrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookRef");

                    b.Property<byte>("Lang");

                    b.Property<byte>("LessonId");

                    b.Property<string>("TextJSON");

                    b.Property<bool>("isPhraseWord");

                    b.HasKey("Id");

                    b.HasIndex("BookRef");

                    b.ToTable("BookPhrases");
                });

            modelBuilder.Entity("BookDBModel.PhraseWord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookRef");

                    b.Property<byte>("Lang");

                    b.Property<int>("PhraseRef");

                    b.Property<string>("Word")
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("BookRef");

                    b.HasIndex("PhraseRef");

                    b.HasIndex("Word", "Lang", "BookRef");

                    b.ToTable("BookPhraseWords");
                });

            modelBuilder.Entity("FulltextDBModel.dm_fts_parser", b =>
                {
                    b.Property<string>("display_term")
                        .ValueGeneratedOnAdd();

                    b.HasKey("display_term");

                    b.ToTable("dm_fts_parsers");
                });

            modelBuilder.Entity("RewiseDBModel.Fact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Data");

                    b.Property<int>("DictRef");

                    b.Property<string>("LocaleText");

                    b.Property<byte[]>("LocaleTextIdxs");

                    b.Property<string>("PhraseText");

                    b.Property<byte[]>("PhraseTextIdxs");

                    b.HasKey("Id");

                    b.HasIndex("DictRef");

                    b.ToTable("UserFacts");
                });

            modelBuilder.Entity("RewiseDBModel.LocaleWord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DictRef");

                    b.Property<int>("FactRef");

                    b.Property<byte>("Lang");

                    b.Property<string>("Word")
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("DictRef");

                    b.HasIndex("FactRef");

                    b.HasIndex("Word", "DictRef");

                    b.ToTable("UserLocaleWords");
                });

            modelBuilder.Entity("RewiseDBModel.PhraseWord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DictRef");

                    b.Property<int>("FactRef");

                    b.Property<byte>("Lang");

                    b.Property<string>("Word")
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("DictRef");

                    b.HasIndex("FactRef");

                    b.HasIndex("Word", "DictRef");

                    b.ToTable("UserPhraseWords");
                });

            modelBuilder.Entity("RewiseDBModel.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RewiseDBModel.Vocabulary", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("LocaleLang");

                    b.Property<byte>("PhraseLang");

                    b.Property<int>("UserRef");

                    b.HasKey("Id");

                    b.HasIndex("UserRef");

                    b.ToTable("UserDicts");
                });

            modelBuilder.Entity("SoundDBModel.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("Lang");

                    b.Property<string>("Path");

                    b.Property<int>("SourceRef");

                    b.Property<string>("Text")
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("SourceRef", "Text", "Lang");

                    b.ToTable("SoundFiles");
                });

            modelBuilder.Entity("SoundDBModel.Source", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("SoundSources");
                });

            modelBuilder.Entity("BookDBModel.Locale", b =>
                {
                    b.HasOne("BookDBModel.Book", "Book")
                        .WithMany("Locales")
                        .HasForeignKey("BookRef");

                    b.HasOne("BookDBModel.Phrase", "Phrase")
                        .WithMany("Locales")
                        .HasForeignKey("PhraseRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BookDBModel.LocaleWord", b =>
                {
                    b.HasOne("BookDBModel.Book", "Book")
                        .WithMany("LocaleWords")
                        .HasForeignKey("BookRef");

                    b.HasOne("BookDBModel.Locale", "Locale")
                        .WithMany("Words")
                        .HasForeignKey("PhraseRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BookDBModel.Phrase", b =>
                {
                    b.HasOne("BookDBModel.Book", "Book")
                        .WithMany("Phrases")
                        .HasForeignKey("BookRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BookDBModel.PhraseWord", b =>
                {
                    b.HasOne("BookDBModel.Book", "Book")
                        .WithMany("PhraseWords")
                        .HasForeignKey("BookRef");

                    b.HasOne("BookDBModel.Phrase", "Phrase")
                        .WithMany("Words")
                        .HasForeignKey("PhraseRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RewiseDBModel.Fact", b =>
                {
                    b.HasOne("RewiseDBModel.Vocabulary", "Dict")
                        .WithMany("Facts")
                        .HasForeignKey("DictRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RewiseDBModel.LocaleWord", b =>
                {
                    b.HasOne("RewiseDBModel.Vocabulary", "Dict")
                        .WithMany("LocaleWords")
                        .HasForeignKey("DictRef");

                    b.HasOne("RewiseDBModel.Fact", "Fact")
                        .WithMany("LocaleWords")
                        .HasForeignKey("FactRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RewiseDBModel.PhraseWord", b =>
                {
                    b.HasOne("RewiseDBModel.Vocabulary", "Dict")
                        .WithMany("PhraseWords")
                        .HasForeignKey("DictRef");

                    b.HasOne("RewiseDBModel.Fact", "Fact")
                        .WithMany("PhraseWords")
                        .HasForeignKey("FactRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RewiseDBModel.Vocabulary", b =>
                {
                    b.HasOne("RewiseDBModel.User", "User")
                        .WithMany("Dicts")
                        .HasForeignKey("UserRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SoundDBModel.File", b =>
                {
                    b.HasOne("SoundDBModel.Source", "Source")
                        .WithMany("Sounds")
                        .HasForeignKey("SourceRef")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
