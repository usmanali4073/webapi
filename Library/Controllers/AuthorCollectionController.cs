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

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorCollectionController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        //[HttpGet("ids")]
        //public ActionResult GetAuthorCollection(IEnumerable<Guid> ids)
        //{

        //}
      

        [HttpPost]
        public ActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authors)
        {
            if (authors == null)
            {
                return BadRequest();
            }
           var authorentity =  Mapper.Map<IEnumerable<AuthorForCreationDto>, IEnumerable<Author>>(authors);
            foreach (var author in authorentity)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
            {
                 new Exception($"Failed to Add");
            }
            return Ok();

        }
    }
}