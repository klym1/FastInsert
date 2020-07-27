using System.Reflection;
using Dapper;
using Xunit.Sdk;

namespace FastInsert.Integration.Tests
{
    public class WithTableAttribute : BeforeAfterTestAttribute
    {
        private readonly string _ddl;

        public WithTableAttribute(string ddl)
        {
            _ddl = ddl;
        }
        
        public override void After(MethodInfo methodUnderTest)
        {
            base.After(methodUnderTest);
            using var connection = BaseTests.GetConnection();
            var tableName = methodUnderTest.Name;
            connection.Execute($"DROP TABLE IF EXISTS {tableName}");
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            base.Before(methodUnderTest);
            using var connection = BaseTests.GetConnection();
            var tableName = methodUnderTest.Name;
            connection.Execute($"DROP TABLE IF EXISTS {tableName}");
            connection.Execute($"CREATE TABLE `{tableName}` ({_ddl})");
        }
    }
}
