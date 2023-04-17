namespace vocabversus_wordset_evaluator.Models.Responses
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class GetWordSetResponse
    {
        /// <summary>
        /// Id of word set
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of word set
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Words contained within word set
        /// </summary>
        public IEnumerable<string> Words { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
