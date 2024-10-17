using System.ComponentModel.DataAnnotations.Schema;

namespace ArtNaxiApi.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        
        [NotMapped]
        public SDRequest Request { get; set; }
    }
}
