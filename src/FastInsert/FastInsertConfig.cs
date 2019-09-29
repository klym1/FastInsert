using System;

namespace FastInsert
{
    public class FastInsertConfig
    {
        public ITableNameResolver TableNameResolver { get; set; }

        public FastInsertConfig(Type elemType)
        {
            TableNameResolver = new AutoTableNameResolver(elemType);
        }
    }

    public static class FastInsertConfigExtensions
    {
        public static FastInsertConfig ToTable(this FastInsertConfig conf, string tableName)
        {
            conf.TableNameResolver = new ManualTableNameResolver(tableName);
            return conf;
        }
    }

    public interface ITableNameResolver
    {
        string GetTableName();
    }

    public class ManualTableNameResolver : ITableNameResolver
    {
        private readonly string _tableName;

        public string GetTableName() => _tableName;
        
        public ManualTableNameResolver(string tableName)
        {
            _tableName = tableName;
        }
    }

    public class AutoTableNameResolver : ITableNameResolver
    {
        private readonly string _tableName;

        public string GetTableName() => _tableName;

        public AutoTableNameResolver(Type elemType)
        {
            _tableName = elemType.Name;
        }
    }
}