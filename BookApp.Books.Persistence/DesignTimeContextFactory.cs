﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookApp.Books.Persistence
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<BookDbContext>          
    {
        private const string connectionString =               
            "Server=(localdb)\\mssqllocaldb;Database=ModMon.BooksDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        public BookDbContext CreateDbContext(string[] args)   
        {
            var optionsBuilder =                              
                new DbContextOptionsBuilder<BookDbContext>(); 
            optionsBuilder.UseSqlServer(connectionString, dbOptions =>
                dbOptions.MigrationsHistoryTable("BookMigrationHistoryName"));    

            return new BookDbContext(optionsBuilder.Options); 
        }
    }
}