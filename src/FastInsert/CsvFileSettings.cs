namespace FastInsert
{
    public class CsvFileSettings
    {
        public string Path { get; }
        public string LineEnding { get; }
        public string Delimiter { get; }
        public string FieldEnclosedByChar { get; }
        public string FieldEscapedByChar { get; }

        public CsvFileSettings(string path, string lineEnding, string delimiter, string fieldEnclosedByChar, string fieldEscapedByChar)
        {
            Path = path;
            LineEnding = lineEnding;
            Delimiter = delimiter;
            FieldEnclosedByChar = fieldEnclosedByChar;
            FieldEscapedByChar = fieldEscapedByChar;
        }
    }
}
