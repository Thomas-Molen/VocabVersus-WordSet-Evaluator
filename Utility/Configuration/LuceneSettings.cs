namespace vocabversus_wordset_evaluator.Utility.Configuration
{
    public class LuceneSettings : ISetting
    {
        public const string SectionName = "LuceneSettings";
        public string Path { get; set; } = string.Empty;
    }
}
