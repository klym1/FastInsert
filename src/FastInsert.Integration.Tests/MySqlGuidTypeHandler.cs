using System;
using System.Data;
using Dapper;

namespace FastInsert.Integration.Tests
{
    //taken from here: https://github.com/StackExchange/Dapper/issues/447#issuecomment-277589028
    public class MySqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = FlipEndian(value.ToByteArray());
        }

        public override Guid Parse(object value)
        {
            return new Guid(FlipEndian((byte[])value));
        }

        internal static byte[] FlipEndian(byte[] oldBytes)
        {
            var newBytes = new byte[16];
            for (var i = 8; i < 16; i++)
                newBytes[i] = oldBytes[i];

            newBytes[3] = oldBytes[0];
            newBytes[2] = oldBytes[1];
            newBytes[1] = oldBytes[2];
            newBytes[0] = oldBytes[3];
            newBytes[5] = oldBytes[4];
            newBytes[4] = oldBytes[5];
            newBytes[6] = oldBytes[7];
            newBytes[7] = oldBytes[6];

            return newBytes;
        }
    }
}
