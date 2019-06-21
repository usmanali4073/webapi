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
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        public readonly IUrlHelper _urlHelper;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
            _libraryRepository = libraryRepository;
        }
        [HttpGet(Name = "GetAuthors")]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors(authorsResourceParameters paging)
        {
            var authorsFromRepo = _libraryRepository.GetAuthors(paging);
            var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorResourceUri(paging, ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ? CreateAuthorResourceUri(paging, ResourceUriType.NextPage) : null;
            var paginationMetaData = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPages,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };


            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetaData));


            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            if (authors == null)
            {
                return NotFound();
            }
            return Ok(authors);
        }

        private string CreateAuthorResourceUri(authorsResourceParameters paging, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        SearchQuery = paging.SearchQuery,
                        genre = paging.Genre,
                        pageNumber = paging.PageNumber - 1,
                        pageSize = paging.PageSize

                    });

                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        SearchQuery = paging.SearchQuery,
                        genre = paging.Genre,
                        pageNumber = paging.PageNumber + 1,
                        pageSize = paging.PageSize

                    });
                default:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        SearchQuery = paging.SearchQuery,
                        genre = paging.Genre,
                        pageNumber = paging.PageNumber ,
                        pageSize = paging.PageSize

                    });
            }
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