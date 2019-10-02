# C# library for super fast MySql bulk inserts (ADO.Net)

[![Build Status](https://travis-ci.org/klym1/FastInsert.svg?branch=master)](https://travis-ci.org/klym1/FastInsert)

[![Build status](https://ci.appveyor.com/api/projects/status/cdu5b6lis9ijs6gf?svg=true)](https://ci.appveyor.com/project/klym1/fastinsert)

## How does it work?

It is a common fact that the fastest way to insert many rows in MySql is by using `LOAD DATA INFILE` query which loads file contents and inserts the rows into a table with lightning fast speed.

But unfortunately it reads the data either from *.csv file or from *.xml which requires a lot of code to be written firstly to generate the file and then send it to MySql server.

This library tries to solve this tediuos task and make the entire process hidden behind the scenes. A client code just needs to call a single extension method:

```csharp

var connection = new MySqlConnection(conn);

var list = Enumerable.Range(1, 100000)
    .Select(id =>
        new Table
        {
            Id = id,
            Text = "text" + id
        });
            
await connection.FastInsertAsync(list, tableName);

```

The code above will insert 100000 rows into a table with a matter of seconds.

Under the hood the library utilizes **CsvHelper** (https://github.com/JoshClose/CsvHelper) to generate a csv file.
