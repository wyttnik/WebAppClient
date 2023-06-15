﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksApp.Models
{
    public class Author
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Author_id { get; set; }
        public string Author_name { get; set; } = null!;

        public List<Book> Books { get; set; } = new();
    }
}
