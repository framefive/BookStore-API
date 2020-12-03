using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper)

        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }
        
        /// <summary>
        /// Get an Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns an Author's record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call for id: {id}");
                var author = await _authorRepository.FindById(id);
                var response = _mapper.Map<AuthorDTO>(author);
                if(author == null)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve Id: {id} ");
                    return NotFound();
                }
                _logger.LogInfo($"{location}: Successfully got record for id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        
        
        /// <summary>
        /// Create an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns>Author Object</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Create Attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was invomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var IsSuccess = await _authorRepository.Create(author);
                if (!IsSuccess)
                {
                    return InternalError($"{location}: Creation Failed");
                }
                _logger.LogInfo($"{location}: Author Created");
                return Created("Create", new { author });
                
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }


       /// <summary>
       /// Updates an Author
       /// </summary>
       /// <param name="Id"></param>
       /// <param name="authorDTO"></param>
       /// <returns></returns>
        [HttpPut("{Id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int Id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Update Attempted - id: {Id}");
                if (Id < 1 || authorDTO == null || Id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location}: Update Failed with Bad Data");
                    return BadRequest();
                }
                var isExists = await _authorRepository.IsExists(Id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve record with Id:{Id}");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was invomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var IsSuccess = await _authorRepository.Update(author);
                if (!IsSuccess)
                {
                    return InternalError($"{location}: Update Failed");
                }
                _logger.LogInfo($"{location}:Record with id: {Id} was successfully updated");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Removes an author by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("{Id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int Id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"Author Delete Attempted - id: {Id}");
                if (Id < 1)
                {
                    _logger.LogWarn("Author Delete with Bad Data");
                    return BadRequest();
                }
                var isExists = await _authorRepository.IsExists(Id);
                if (!isExists)
                {
                    _logger.LogWarn($"Author with Id:{Id} was not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(Id);
                var IsSuccess = await _authorRepository.Delete(author);
                if (!IsSuccess)
                {
                    return InternalError("Author Delete Failed");
                }
                _logger.LogInfo($"Author with Id:{Id} successfully deleted");
                return NoContent();
                
            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} = {action}";
        }

        private ObjectResult InternalError(string message)
        {
        _logger.LogError(message);
        return StatusCode(500, "Something went wrong. Check log and AuthorsController.cs");
        }
    }
}
