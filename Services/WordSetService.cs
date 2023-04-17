using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Text;
using vocabversus_wordset_evaluator.Models;

namespace vocabversus_wordset_evaluator.Services
{
    public class WordSetService
    {
        private readonly LuceneVersion _version;
        private readonly StandardAnalyzer _analyzer;
        private readonly FSDirectory _directory;
        private readonly IndexWriter _writer;

        public WordSetService()
        {
            _version = LuceneVersion.LUCENE_48;
            _analyzer = new StandardAnalyzer(_version);
            _directory = FSDirectory.Open("lucenedb");
            var config = new IndexWriterConfig(_version, _analyzer);
            _writer = new IndexWriter(_directory, config);
        }

        /// <summary>
        /// Saves words and name as a <see cref="WordSet"> using Lucene
        /// </summary>
        /// <param name="name">Name of word set</param>
        /// <param name="words">All words contained within the word set</param>
        /// <returns>reference <see cref="Guid"> of the stored word set</returns>
        /// <exception cref="ArgumentException">duplicate name was given</exception>
        public Guid CreateWordSet(string name, params string[] words)
        {
            if (WordSetNameExists(name)) throw new ArgumentException($"Word set with given name already exists: {name}");
            Guid wordSetId = Guid.NewGuid();

            // Add wordSet to the Lucene Index
            var document = new Document
            {
                // Reference fields
                new StringField("Id", wordSetId.ToString(), Field.Store.YES),
                new StringField("Name", name, Field.Store.YES),
            };
            // add word search field
            foreach (var word in words)
            {
                document.AddStringField("Words", word.ToLower(), Field.Store.YES);
            }
            _writer.AddDocument(document);
            _writer.Commit();

            return wordSetId;
        }

        /// <summary>
        /// Checks if a word set with given name already exists within Lucene
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>true if word set with name was found, otherwise false</returns>
        private bool WordSetNameExists(string name)
        {
            IndexSearcher searcher;
            try
            {
                searcher = CreateSearcher();
            }
            catch (ArgumentException)
            {
                // Searcher could not be created, so no wordSets exist yet
                return false;
            }

            // Create search query
            Query NameQuery = new TermQuery(new Term("Name", name));

            // Find stored document
            return searcher.Search(NameQuery, 1).TotalHits > 0;
        }

        /// <summary>
        /// Checks if given word exists within word set
        /// </summary>
        /// <param name="wordSetId">Word set to check within</param>
        /// <param name="word">Word to look for</param>
        /// <param name="incorrectCharMargin">amount of characters that the given word can be wrong by while still resulting in true result, default = 0</param>
        /// <returns>true if word was found within word set, otherwise false</returns>
        public bool HasWordMatch(Guid wordSetId, string word, int incorrectCharMargin = 0)
        {
            IndexSearcher searcher = CreateSearcher();

            // Create search queries
            Query wordSetQuery = new TermQuery(new Term("Id", wordSetId.ToString()));
            Query wordQuery = new FuzzyQuery(new Term("Words", word), incorrectCharMargin);

            BooleanQuery query = new BooleanQuery();
            query.Add(wordSetQuery, Occur.MUST);
            query.Add(wordQuery, Occur.MUST);

            // if any match was found, return true
            return searcher.Search(query, 1).TotalHits > 0;
        }

        /// <summary>
        /// finds <see cref="WordSet"> data in Lucene
        /// </summary>
        /// <param name="wordSetId">Word set to find</param>
        /// <param name="fields">Stored Lucene fields to return</param>
        /// <returns><see cref="WordSet"> of found wordSetId with given fields populated</returns>
        /// <exception cref="ArgumentException">given wordset is not stored within Lucene</exception>
        public WordSet GetWordSet(Guid wordSetId, string[] fields)
        {
            IndexSearcher searcher = CreateSearcher();

            // Create search query
            Query WordSetQuery = new TermQuery(new Term("Id", wordSetId.ToString()));
            ISet<string> queryFields = (fields.Any()) ? fields.ToHashSet() : WordSet.LuceneFields;

            // Find stored document
            var searchResult = searcher.Search(WordSetQuery, 1).ScoreDocs.FirstOrDefault() ?? throw new ArgumentException($"No WordSet found for Id: {wordSetId}");
            var wordSet = searcher.Doc(searchResult.Doc, queryFields);

            return wordSet.ToWordSet();
        }

        /// <summary>
        /// finds all word sets stored within Lucene
        /// </summary>
        /// <param name="fields">fields stored in Lucene to populate</param>
        /// <param name="limit">amount of word sets to return</param>
        /// <remarks>default/null limit will return all word sets</remarks>
        /// <returns>stored <see cref="WordSet"> in Lucene</returns>
        public List<WordSet> GetWordSets(string[] fields, int? limit)
        {
            // If no fields where specified, return all available
            ISet<string> queryFields = (fields.Any()) ? fields.ToHashSet() : WordSet.LuceneFields;

            IndexSearcher searcher;
            try
            {
                searcher = CreateSearcher();
            }
            catch (ArgumentException)
            {
                // No searcher could be created as there is no index to search for, so return empty list
                return new List<WordSet>();
            }

            List<WordSet> wordSets = new();
            Query query = new MatchAllDocsQuery();

            // Get first 100 wordSet references
            var searchResult = searcher.Search(query, 100);
            // Go through all wordSet references and add results to list, untill either all references are added or list limit is reached
            while (searchResult.ScoreDocs.Length > 0)
            {
                // Go through all found references and add the wordSet info to list
                ScoreDoc[] results = searchResult.ScoreDocs;
                foreach (var result in results)
                {
                    wordSets.Add(searcher.Doc(result.Doc, queryFields).ToWordSet());
                    if (limit is not null && wordSets.Count >= limit) return wordSets;
                }
                // Get the next 10 wordSet references
                ScoreDoc lastDoc = results[results.Length - 1];
                searchResult = searcher.SearchAfter(lastDoc, query, 100);
            }

            return wordSets;
        }

        /// <summary>
        /// Creates an <see cref="IndexSearcher"> to use for querying Lucene
        /// </summary>
        /// <returns><see cref="IndexSearcher"> to use for querying Lucene</returns>
        /// <exception cref="ArgumentException">Lucene storage is empty</exception>
        private IndexSearcher CreateSearcher()
        {
            try
            {
                var directoryReader = DirectoryReader.Open(_directory);
                var indexSearcher = new IndexSearcher(directoryReader);
                return indexSearcher;
            }
            catch (IndexNotFoundException)
            {
                throw new ArgumentException("No WordSets exist yet to search for");
            }
        }
    }
}
