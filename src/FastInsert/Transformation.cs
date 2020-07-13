namespace FastInsert
{
    public static class Transformation
    {
        public static string FromHex(string varName) => $"UNHEX({varName})";
        public static string FromBase64(string varName) => $"FROM_BASE64({varName})";
    }
}
