﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SammBotNET.Database;

namespace SammBotNET.Migrations.EmotionalSupportDBMigrations
{
    [DbContext(typeof(EmotionalSupportDB))]
    [Migration("20211108194245_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("SammBotNET.Database.EmotionalSupport", b =>
                {
                    b.Property<int>("SupportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("SupportMessage")
                        .HasColumnType("TEXT");

                    b.HasKey("SupportId");

                    b.ToTable("EmotionalSupport");
                });
#pragma warning restore 612, 618
        }
    }
}