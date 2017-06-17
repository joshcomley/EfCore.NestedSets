﻿// <auto-generated />
using EfCore.NestedSets.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EfCore.NestedSets.Tests.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-preview1-24937")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EfCore.NestedSets.Tests.Node", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Left");

                    b.Property<int>("Level");

                    b.Property<string>("Name");

                    b.Property<int?>("ParentId");

                    b.Property<int>("Right");

                    b.Property<int?>("RootId");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.HasIndex("RootId");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("EfCore.NestedSets.Tests.Node", b =>
                {
                    b.HasOne("EfCore.NestedSets.Tests.Node", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.HasOne("EfCore.NestedSets.Tests.Node", "Root")
                        .WithMany("Descendants")
                        .HasForeignKey("RootId");
                });
        }
    }
}
