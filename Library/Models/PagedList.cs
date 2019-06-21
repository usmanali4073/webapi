﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPages { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious
        {
            get
            {
                return (CurrentPages > 1);
            }
        }
        public bool HasNext
        {
            get
            {
                return (CurrentPages < TotalPages);
            }
        }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            this.TotalCount = items.Count;
            this.TotalPages = items.Count;
            this.PageSize = pageSize;
            this.CurrentPages = pageNumber;
            this.TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            AddRange(items);

        }
        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
