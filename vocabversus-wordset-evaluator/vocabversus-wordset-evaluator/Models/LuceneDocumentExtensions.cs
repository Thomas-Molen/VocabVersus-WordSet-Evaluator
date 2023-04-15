using Lucene.Net.Documents;

namespace vocabversus_wordset_evaluator.Models
{
    static class LuceneDocumentExtensions
    {
        /// <summary>
        /// Convert Lucene <see cref="Document"> to VocabVersus <see cref="WordSet">
        /// </summary>
        /// <param name="wordSetDocument">Lucene <see cref="Document"> to convert, ensure wanted data is populated by Lucene before conversion</param>
        /// <returns>VocabVersus <see cref="WordSet"></returns>
        public static WordSet ToWordSet(this Document wordSetDocument)
        {
            return new WordSet
            {
                Id = Guid.Parse(wordSetDocument.Fields.FirstOrDefault(f => f.Name == "Id")?.GetStringValue() ?? Guid.Empty.ToString()),
                Name = wordSetDocument.Fields.FirstOrDefault(f => f.Name == "Name")?.GetStringValue() ?? string.Empty,
                Words = wordSetDocument.Fields.FirstOrDefault(f => f.Name == "Words")?.GetStringValue()?.Split() ?? Array.Empty<string>()
            };
        }
    }
}
