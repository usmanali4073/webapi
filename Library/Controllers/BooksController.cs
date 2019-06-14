﻿using System;
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
        [HttpGet("{bookId}")]
       public ActionResult<BooksDto> GetBookByAuthor(Guid authorid, Guid bookId)
        {
            var book = _libraryRepository.GetBookForAuthor(authorid, bookId);
            if (book == null)
            {
                return NotFound();
            };
            return Ok(Mapper.Map<Book, BooksDto>(book));
        }
    }
}