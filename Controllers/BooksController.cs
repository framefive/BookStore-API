using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts with Books Table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)

        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>A List of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Get an Book by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>Returns an Book's record</returns>
        [HttpGet("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int Id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call for id: {Id}");
                var book = await _bookRepository.FindById(Id);
                var response = _mapper.Map<BookDTO>(book);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve Id: {Id} ");
                    return NotFound();
                }
                _logger.LogInfo($"{location}: Successfully got record for id: {Id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create an Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Create Attempted");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was invomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var IsSuccess = await _bookRepository.Create(book);
                if (!IsSuccess)
                {
                    return InternalError($"{location}: Creation Failed");
                }
                _logger.LogInfo($"{location}: Create Successful");
                return Created("Create", new { book });

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }



        }

        /// <summary>
        /// Updates a book
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{Id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int Id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Update Attempted - id: {Id}");
                if (Id < 1 || bookDTO == null || Id != bookDTO.Id)
                {
                    _logger.LogWarn($"{location}: Update Failed with Bad Data");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(Id);
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
                var book = _mapper.Map<Book>(bookDTO);
                var IsSuccess = await _bookRepository.Update(book);
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
        /// Removes an book by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("{Id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int Id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"Book Delete Attempted - id: {Id}");
                if (Id < 1)
                {
                    _logger.LogWarn("Book Delete with Bad Data");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(Id);
                if (!isExists)
                {
                    _logger.LogWarn($"Book with Id:{Id} was not found");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(Id);
                var IsSuccess = await _bookRepository.Delete(book);
                if (!IsSuccess)
                {
                    return InternalError("Book Delete Failed");
                }
                _logger.LogInfo($"Book with Id:{Id} successfully deleted");
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
