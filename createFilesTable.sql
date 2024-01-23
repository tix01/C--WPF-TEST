CREATE DATABASE IF NOT EXISTS DB_Files_geretions;
USE DB_Files_geretions;

CREATE TABLE IF NOT EXISTS Files (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Date DATE,
    LatinChars NVARCHAR(10),
    RussianChars NVARCHAR(10),
    IntegerNumber INT,
    DecimalNumber DECIMAL(18, 8)
);

