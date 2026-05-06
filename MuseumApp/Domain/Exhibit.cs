using System;

namespace MuseumApp.Domain
{
    public class Exhibit
    {
        public string InventoryNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AuthorOrEra { get; set; } = string.Empty;
        public DateTime AcquisitionDate { get; set; }
        public Category Category { get; set; }
        public ConditionStatus Status { get; set; }
    }
}