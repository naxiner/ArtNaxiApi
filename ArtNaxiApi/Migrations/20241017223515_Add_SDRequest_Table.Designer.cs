﻿// <auto-generated />
using System;
using ArtNaxiApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ArtNaxiApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241017223515_Add_SDRequest_Table")]
    partial class Add_SDRequest_Table
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ArtNaxiApi.Models.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SDRequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SDRequestId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ArtNaxiApi.Models.SDRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CfgScale")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "cfg_scale");

                    b.Property<int>("Height")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "height");

                    b.Property<string>("NegativePrompt")
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "negative_prompt");

                    b.Property<string>("Prompt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "prompt");

                    b.Property<string>("SamplerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "sampler_name");

                    b.Property<string>("Scheduler")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "scheduler");

                    b.Property<int>("Steps")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "steps");

                    b.Property<string>("Styles")
                        .HasColumnType("nvarchar(max)")
                        .HasAnnotation("Relational:JsonPropertyName", "styles");

                    b.Property<int>("Width")
                        .HasColumnType("int")
                        .HasAnnotation("Relational:JsonPropertyName", "width");

                    b.HasKey("Id");

                    b.ToTable("SDRequests");
                });

            modelBuilder.Entity("ArtNaxiApi.Models.Image", b =>
                {
                    b.HasOne("ArtNaxiApi.Models.SDRequest", "Request")
                        .WithMany()
                        .HasForeignKey("SDRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Request");
                });
#pragma warning restore 612, 618
        }
    }
}
