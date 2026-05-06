using System;
using System.Collections.Generic;

namespace MuseumApp.Domain
{
    public class ExhibitionHall
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<Exhibit> Items { get; set; } = new List<Exhibit>();
    }
}