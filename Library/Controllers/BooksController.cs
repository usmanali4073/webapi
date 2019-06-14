using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorid}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }


        [HttpGet]
        public ActionResult<IEnumerable<BooksDto>> GetBooksForAuthor(Guid authorid)
        {
            var AuthorExists = this._libraryRepository.AuthorExists(authorid);
            if (!AuthorExists)
            {
                return NotFound(authorid);
            }
            var books = Mapper.Map<IEnumerable<Book>,IEnumerable<BooksDto>>(_libraryRepository.GetBooksForAuthor(authorid));
            return Ok(books);
        }

        [HttpGet("{bookId}", Name = "GetBookByAuthor")]
       public ActionResult<BooksDto> GetBookByAuthor(Guid authorid, Guid bookId)
        {
            var book = _libraryRepository.GetBookForAuthor(authorid, bookId);
            if (book == null)
            {
                return NotFound();
            };
            return Ok(Mapper.Map<Book, BooksDto>(book));
        }

        [HttpPost]
        public ActionResult CreateBookForAuthor(Guid authorid, [FromBody]  BookforCreationDto book)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!_libraryRepository.AuthorExists(authorid))
                {
                    return NotFound();
                }
                var booketitiy = Mapper.Map<BookforCreationDto, Book>(book);
                _libraryRepository.AddBookForAuthor(authorid, booketitiy);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Failed to Add the Book{book.Title} for {authorid}");
                }
                return CreatedAtRoute("GetBookByAuthor", new { authorid = authorid, bookId = booketitiy.Id }, booketitiy);
            }
            catch (Exception ex)
            {

                throw;
            }
           
           
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
 

        [HttpDelete("{bookId}")]
        public ActionResult DeleteBookForAuthor(Guid authorid, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorid))
            {
                return NotFound();
            }

            var bookforAuthor = _libraryRepository.GetBookForAuthor(authorid, bookId);

            if (bookforAuthor == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteBook(bookforAuthor);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Delete Book {bookId} for Author {authorid} failed");
            }
            return NoContent();
        }
    }
}