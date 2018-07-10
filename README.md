# ADO.Extensions

<h2>For those who don't want to use EF, but still don't want to write every query, the ADO.Extensions helps you to convert models into queries.</h2>

<h3>This library is written in .NET Core 2 and currently only supports Oracle database.<h3>

<h4>How it works</h4>

In order to use the available methods, you need to create an intance of the DBHandler class, passing a model as type.
For example:
```cs
DBHandler<Person> dbHandler = new DBHandler<Person>();
```
Where Person is your class model.

So let's say you want to execute the following statement:
```sql
SELECT ID, FULL_NAME, BIRTHDAY FROM PERSON
```
All you have to do is this:
```cs
var dbResult = dbHandler.ExecuteReader() ;
```
In your class model, there are some attributes that should be used, so the library knows what table and columns that model represents.
For example:
```cs
    [TableName("PERSON")]
    public class Person
    {
        [ColumnName("ID"), IsPK(true)]
        public decimal Id { get; set; }

        [ColumnName("FULL_NAME")]
        public string FullName { get; set; }

        [ColumnName("BIRTHDAY")]
        public DateTime Birthday { get; set; }

        [ColumnName("AGE"), IsComputed(true)]
        public decimal Age { get; set; }

    }
```

<h4>The available attributes are:</h4>

| Attribute  | Description |
| ------------- | ------------- |
|TableName| Use it to inform the table name that the model represents |
|ColumnName| Use it to inform the column name that the property represents |
|IsPK| Mark it as true for properties that represent a Primary Key column |
|IsComputed| Mark it as true if you want this property to be ignored. If it dosen't represent any column of the given table |


<h4>Current methods avaliable are:</h4>

| Method  | Description | Parameters |
| ------------- | ------------- | ------------- |
|ExecuteReader| Returns all records from the table. Should be used when you don't have a where clause. | None |
|ExecuteReader| Returns all records from the table, given an where clause  | String where, String orderBy |
|ExecuteNonQuery|Executes a spefic query it receives by parameter and returns the amount of rows affected. Should be use for spefic updates, inserts, deletes, etc. |String command|
|ExecuteScalar| Executes a spefic query it receives by parameter and returns and object. Should be use when no other method fits your need. |String command|
|Insert|Receives and object and inserts it into the database. |Object obj|
|Store|Use it when you don't know if the object exists in the database. It will try to insert or update the given values.  |Object obj|
|Update|Receives and object and updates it into the database. |Object obj|
|Delete|Receives and object and deletes it into the database. |Object obj|

<h3>Examples:</h3>
In theses examples we are going to use the Person model you saw previously.

<h2>ExecuteReader</h2>
Gets the first 10 records from the PERSON table and prints the full name to console.

```cs
DBHandler<Person> dbHandler = new DBHandler<Person>();
List<Person> people = dbHandler.ExecuteReader(" ID <= 10 ", " ID ");

foreach(var person in people)
    Console.WriteLine(person.FullName);
```

<h2>Store</h2>
Create a new Person object and stores it in the database. 
The Update and Insert methods work the same. Just create an object and pass to it.

```cs
Person person = new Person()
{
    FullName = "John Doe",
    Birthday = new DateTime(1965, 05, 03),
    Age = 53
};

DBHandler<Person> dbHandler = new DBHandler<Person>();
dbHandler.Store(person);
```

<h2>Delete</h2>
To delete you have to pass at least the primary key properties.

```cs
Person person = new Person()
{
    Id = 11
};

DBHandler<Person> dbHandler = new DBHandler<Person>();
dbHandler.Delete(person);
```

<br><br><br><br>
Feel free to fork and help improve this library.
