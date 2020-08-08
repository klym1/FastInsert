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
            
await connection.FastInsertAsync(list, tableName: "table");

```

The code above will insert 100000 rows into a table with a matter of seconds.

Under the hood the library utilizes **CsvHelper** (https://github.com/JoshClose/CsvHelper) to generate a csv file. The file is automatically cleaned up after the request is completed.

## Prerequisites

The library is built on top of 'LOAD DATA INFILE' command, but the possibility to load data from *.csv files is disabled by default. To make it work set 'AllowLoadLocalInfile' parameter to 'true' in the mysql connection string:

```sql
;AllowLoadLocalInfile=true;
```

Also, if you intend to insert some binary data, 'AllowUserVariables' must be enabled too:

```sql
;AllowLoadLocalInfile=true;AllowUserVariables=true;
```

## Examples

The libary supports all built-in types (like `bool`, `int`, `string` etc) and some commonly used ones, like `Guid`, `byte[]`. More generic support (like Dapper has) might be added in the future.


## Configuration

Fluent configuration is available with the following options:


```csharp

connection.FastInsertAsync(list, 
     conf => conf
         //specify the target table name
         .ToTable(tableName)              

         //Set the maximum number of rows per single csv file (default: 100000)
         .BatchSize(100000)               
    
         //specify how binary data is saved to *.csv file, 
         //BinaryFormat.Hex (default) - e.g. 0xABC0123
         //BinaryFormat.Base64 - e.g. cnR5ZA== (more efficient than Hex)
         .BinaryFormat(BinaryFormat.Hex)  
    
         //When set the csv contents will be written to `writer`                  
         .Writer(writer)    

);
```


