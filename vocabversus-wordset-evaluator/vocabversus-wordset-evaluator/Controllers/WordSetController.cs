using Microsoft.AspNetCore.Mvc;
using vocabversus_wordset_evaluator.Models.Requests;
using vocabversus_wordset_evaluator.Models.Responses;
using vocabversus_wordset_evaluator.Services;

namespace vocabversus_wordset_evaluator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordSetController : ControllerBase
    {
        private readonly WordSetService _wordSetService;
        public WordSetController(WordSetService wordSetService)
        {
            _wordSetService = wordSetService;
        }

        /// <summary>
        /// Stores name and list of words as word set
        /// </summary>
        /// <param name="request">Request data to store as word set</param>
        /// <returns>Reference ID of word set, used for later querying on word set</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateWordSet([FromBody] CreateWordSetRequest request)
        {
            try
            {
                Guid wordSetId = _wordSetService.CreateWordSet(request.Name, request.Words.ToArray());
                return Ok(wordSetId);
            }
            catch (ArgumentException)
            {
                return BadRequest($"Can not create WordSet with Duplicate name: {request.Name}");
            }
        }

        /// <summary>
        /// Finds data of word set
        /// </summary>
        /// <param name="wordSetId">Word set to find</param>
        /// <param name="fields">Data to return from word set</param>
        /// <returns>Word set data</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetWordSetResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetWordSet([FromQuery] Guid wordSetId, [FromQuery] string[] fields)
        {
            try
            {
                var wordSet = _wordSetService.GetWordSet(wordSetId, fields);
                return Ok(new GetWordSetResponse
                {
                    Id = wordSet.Id,
                    Name = wordSet.Name,
                    Words = wordSet.Words
                });
            }
            catch (ArgumentException)
            {
                return NotFound($"Could not find a WordSet with ID: {wordSetId}");
            }
        }

        /// <summary>
        /// Finds all word sets
        /// </summary>
        /// <param name="fields">Data to return of word sets</param>
        /// <param name="count">Amount of word sets to return</param>
        /// <returns>List of word sets</returns>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetWordSetListResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetWordSets([FromQuery] string[] fields, [FromQuery] int? count)
        {
            if (count == 0) return BadRequest("count can not be 0");

            var wordSets = _wordSetService.GetWordSets(fields, count);
            return Ok(new GetWordSetListResponse
            {
                WordSets = wordSets
            });
        }

        /// <summary>
        /// Evaluate a given word against specified requirements in a word set
        /// </summary>
        /// <param name="wordSetId">Word set to evaluate within</param>
        /// <param name="word">Word to evaluate</param>
        /// <param name="fuzzyChars">Amount of incorrect characters within word that still result in positive evaluation</param>
        /// <returns>evaluation results</returns>
        [HttpGet("evaluate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EvaluateWordSetResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult WordExistsInWordSet([FromQuery] Guid wordSetId, [FromQuery] string word, [FromQuery] int fuzzyChars = 0)
        {
            try
            {
                bool hasMatch = _wordSetService.HasWordMatch(wordSetId, word.ToLower(), fuzzyChars);
                return Ok(new EvaluateWordSetResponse
                {
                    HasMatch = hasMatch,
                });
            }
            catch (ArgumentOutOfRangeException)
            {
                // Fuzzy search was set to a value higher than allowed
                return BadRequest($"fuzzyChars value has to be between 0 and 2, given value was: {fuzzyChars}");
            }
            catch (ArgumentException)
            {
                // WordSet could not be found
                return NotFound($"Could not find a WordSet with ID: {wordSetId}");
            }
        }
    }
}