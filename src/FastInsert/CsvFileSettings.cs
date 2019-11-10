namespace FastInsert
{
    public class CsvFileSettings
    {
        public string Path { get; set; }
        public string LineEnding { get; set; }
        public string Delimiter { get; set; }

        public string FieldEnclosedByChar { get; set; }
        public string FieldEscapedByChar { get; set; }
    }
}