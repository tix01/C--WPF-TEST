# C--WPF-TEST
Excel Parser and Database Loader - MySQL Edition

This is a C# WPF application designed to parse Excel files, store the extracted data into a MySQL database, and generate and merge multiple files into one before storing them in the database. The program utilizes the MySql.Data library for MySQL database interaction and the EPPlus library for Excel parsing.
Features

    Excel Parsing: The program can read Excel files (.xlsx) and extract data from specified sheets using the EPPlus library.

    Database Loading: Parsed data is stored in a MySQL database for easy retrieval and analysis using the using MySql.Data.MySqlClient library.

    File Generation and Merging: It can generate 100 Excel files, merge them into a single file, and then store the merged file in the MySQL database.

Requirements

    Visual Studio with C# support
    using MySql.Data.MySqlClient library: Install using NuGet Package Manager
    EPPlus library: Install using NuGet Package Manager
    EPPlus library: Install using NuGet Package Manager
    Microsoft.VisualBasic: Install using NuGet Package Manager
    MySQL Server

Usage

Parsing Excel Files and Loading into Database:

        Open the solution in Visual Studio.
        Modify the connection string in the ExcelParser.cs file to point to your MySQL database.
        Build and run the application.

Generating Files, Merging, and Loading into Database:

    Open the solution in Visual Studio.
    Modify the connection string in the FileGenerator.cs file to point to your MySQL database.
    Build and run the application.
