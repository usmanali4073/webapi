using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Library.Entities;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
                Console.WriteLine(ex.Message);
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
        [HttpPut("{id}")]
        public ActionResult UpdateBookforAuthor(Guid authorid,Guid id, [FromBody] BookforUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorid))
            {
                return NotFound();
            }

            var bookforUpdatefromRepo = _libraryRepository.GetBookForAuthor(authorid, id);
            if (bookforUpdatefromRepo == null)
            {
                var booktoAdd = Mapper.Map<Book>(book);
                if (booktoAdd != null)
                {
                    booktoAdd.Id = id;
                }
                _libraryRepository.AddBookForAuthor(authorid, booktoAdd);
                if (_libraryRepository.Save())
                {
                    return CreatedAtRoute("GetBookByAuthor", new { authorid = authorid, bookId = id }, booktoAdd);
                }
            }
            Mapper.Map(book, bookforUpdatefromRepo);

            _libraryRepository.UpdateBookForAuthor(bookforUpdatefromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Error while updating the Book");
            }

            return NoContent();

        }

        [HttpPatch("{id}")]
        public ActionResult PartiallyUpdateBookForAuthor(Guid authorid, Guid id, JsonPatchDocument<BookforUpdateDto> docPatch)
        {
            if (docPatch == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorid))
            {
                return NotFound();
            }

            //Find The Book by Author and BookId
            var BookforAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorid, id);
            if (BookforAuthorFromRepo == null)
            {
                var BookforUpdateDto = new BookforUpdateDto();
                docPatch.ApplyTo(BookforUpdateDto);
                var book = Mapper.Map<Book>(BookforUpdateDto);
                book.Id = id;
                _libraryRepository.AddBookForAuthor(authorid,book);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Error Creating the Book for {authorid}");
                }
                return CreatedAtRoute("GetBookByAuthor", new { authorid = authorid, bookId = id }, book);
            }
            //Convert the Book to BookforUpdateDto so JsonPatch can be applied
            var booktoPatch = Mapper.Map<BookforUpdateDto>(BookforAuthorFromRepo);
            //Apply The Patch
            docPatch.ApplyTo(booktoPatch);
            //Tranfer the Update booktoPatch to Book
            Mapper.Map(booktoPatch, BookforAuthorFromRepo);
            //Add the Patched Book to Repo
            _libraryRepository.UpdateBookForAuthor(BookforAuthorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Error while Patch the Book");
            }
            return NoContent();
        }
    }
}