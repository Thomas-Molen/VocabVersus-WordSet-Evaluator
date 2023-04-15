namespace vocabversus_wordset_evaluator.Models
{
    public class WordSet
    {
        /// <summary>
        /// Field names of data stored within Lucene
        /// </summary>
        public static readonly ISet<string> LuceneFields = new HashSet<string> { "Id", "Name", "Words" };
        /// <summary>
        /// Id of word set
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Name of word set
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Words contained within word set
        /// </summary>
        public IEnumerable<string> Words { get; set; } = new List<string>();
    }
}
