using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }
        [HttpGet]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors()
        {

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(_libraryRepository.GetAuthors());
            if (authors == null)
            {
                return NotFound();
            }
            return Ok(authors);


        }
        [HttpGet("{Id}", Name = "GetAuthor")]
        public ActionResult<AuthorDto> GetAuthor(Guid Id)
        {
            var author = _libraryRepository.GetAuthor(Id);

            if (author == null)
            {
                return NotFound(Id);
            }
            var authorM = Mapper.Map<AuthorDto>(author);
            return Ok(authorM);
        }
        [HttpPost]
        public ActionResult<AuthorForCreationDto> CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating author failed on save");
            }

            var authortoReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { Id = authortoReturn.Id }, authortoReturn);
        }



        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteAuthor(authorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting an Author has Error {id}");
            }
            return NoContent();
        }
    }
}