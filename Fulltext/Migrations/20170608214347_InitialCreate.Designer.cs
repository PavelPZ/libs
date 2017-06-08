using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Fulltext;

namespace Fulltext.Migrations
{
    [DbContext(typeof(FulltextContext))]
    [Migration("20170608214347_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Fulltext.Dict", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Imported");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Dicts");
                });

            modelBuilder.Entity("Fulltext.dm_fts_parser", b =>
                {
                    b.Property<string>("display_term")
                        .ValueGeneratedOnAdd();

                    b.HasKey("display_term");

                    b.ToTable("dm_fts_parser");
                });

            modelBuilder.Entity("Fulltext.Phrase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Base")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<byte>("DestLang");

                    b.Property<int?>("DictId")
                        .IsRequired();

                    b.Property<byte>("SrcLang");

                    b.Property<int?>("SrcRef");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<byte[]>("TextIdxs")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Base");

                    b.HasIndex("DictId");

                    b.HasIndex("SrcRef");

                    b.ToTable("Phrases");
                });

            modelBuilder.Entity("Fulltext.PhraseWord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("DestLang");

                    b.Property<int?>("PhraseId")
                        .IsRequired();

                    b.Property<int>("PhraseRef");

                    b.Property<byte>("SrcLang");

                    b.Property<string>("Word")
                        .IsRequired()
                        .HasMaxLength(24);

                    b.HasKey("Id");

                    b.HasIndex("PhraseId");

                    b.HasIndex("SrcLang");

                    b.HasIndex("Word");

                    b.ToTable("PhraseWords");
                });

            modelBuilder.Entity("Fulltext.Phrase", b =>
                {
                    b.HasOne("Fulltext.Dict", "Dict")
                        .WithMany("Phrases")
                        .HasForeignKey("DictId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Fulltext.Phrase", "Src")
                        .WithMany("Dests")
                        .HasForeignKey("SrcRef");
                });

            modelBuilder.Entity("Fulltext.PhraseWord", b =>
                {
                    b.HasOne("Fulltext.Phrase", "Phrase")
                        .WithMany("Words")
                        .HasForeignKey("PhraseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
