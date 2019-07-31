# C# library for super fast MySql bulk inserts

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
