namespace vocabversus_wordset_evaluator.Models.Responses
{
    public class GetWordSetListResponse
    {
        /// <summary>
        /// word sets stored
        /// </summary>
        public List<WordSet> WordSets { get; set; } = new();
    }
}
