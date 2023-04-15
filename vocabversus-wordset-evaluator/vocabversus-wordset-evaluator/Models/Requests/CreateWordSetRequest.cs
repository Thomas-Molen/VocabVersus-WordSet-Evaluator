namespace vocabversus_wordset_evaluator.Models.Requests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class CreateWordSetRequest
    {
        /// <summary>
        /// Word set name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Words to contain within word set
        /// </summary>
        public IEnumerable<string> Words { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
