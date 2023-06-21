﻿using System.ComponentModel.DataAnnotations.Schema;

namespace RestProject.Models
{
    public class BookToEdit
    {
        public string Title { get; set; } = null!;
        public string Isbn13 { get; set; } = null!;
        public int Num_pages { get; set; }
        public DateTime Publication_date { get; set; }

        public int Publisher_id { get; set; }

        public int Language_id { get; set; }

        [NotMapped]
        public IFormFile? FileUri { get; set; }

        public List<int> AuthorsId { get; set; } = new();
    }
}
