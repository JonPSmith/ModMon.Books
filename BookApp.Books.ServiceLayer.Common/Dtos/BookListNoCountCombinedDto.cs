﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace BookApp.Books.ServiceLayer.Common.Dtos
{
    public class BookListNoCountCombinedDto
    {
        public BookListNoCountCombinedDto(SortFilterPageOptionsNoCount sortFilterPageData, IEnumerable<BookListDto> booksList)
        {
            SortFilterPageData = sortFilterPageData;
            BooksList = booksList;
        }

        public SortFilterPageOptionsNoCount SortFilterPageData { get; private set; }

        public IEnumerable<BookListDto> BooksList { get; private set; }
    }
}