using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace EfCore.NestedSets.Tests
{
    public class DbSql
    {
        public static void CreateDatabase()
        {
            RunServerSql(CreateDb);
        }

        public static void RunServerSql(string sql)
        {
            RunSql("Persist Security Info=False;Integrated Security=true;server=.", sql);
        }

        public static void RunDbSql(string sql)
        {
            RunSql(ConnectionString, sql);
        }

        public static string ConnectionString =>
            "Persist Security Info=False;Integrated Security=true;Initial Catalog=EfCore.NestedSet.Tests;server=.";

        public static void RunSql(string connectionString, string sql)
        {
            // See: https://stackoverflow.com/a/18597011/64519
            // Allows usage of "GO" in executed statements
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConn;

                    var scripts = Regex.Split(sql, @"^GO\r?$", RegexOptions.Multiline);
                    foreach (var splitScript in scripts)
                    {
                        if (!string.IsNullOrWhiteSpace(splitScript))
                        {
                            command.CommandText = splitScript;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Split by "GO" statements
            var statements = Regex.Split(
                sqlScript,
                @"^[\t ]*GO[\t ]*\d*[\t ]*(?:--.*)?$",
                RegexOptions.Multiline |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));
        }

        public static string CreateDb = @"USE [master]
GO
-- Create data database
IF db_id('EfCore.NestedSet.Tests') IS NOT NULL
BEGIN
	ALTER DATABASE [EfCore.NestedSet.Tests] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [EfCore.NestedSet.Tests]
END
/****** Object:  Database [EfCore.NestedSet.Tests]    Script Date: 12/06/2017 18:53:53 ******/
CREATE DATABASE [EfCore.NestedSet.Tests]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'EfCore.NestedSet.Tests', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\EfCore.NestedSet.Tests.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'EfCore.NestedSet.Tests_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\EfCore.NestedSet.Tests_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EfCore.NestedSet.Tests].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ARITHABORT OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET  DISABLE_BROKER 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET RECOVERY FULL 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET  MULTI_USER 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET DB_CHAINING OFF 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'EfCore.NestedSet.Tests', N'ON'
GO
USE [EfCore.NestedSet.Tests]
GO
/****** Object:  Table [dbo].[Nodes]    Script Date: 12/06/2017 18:53:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [master]
GO
ALTER DATABASE [EfCore.NestedSet.Tests] SET  READ_WRITE 
GO";
    }
}