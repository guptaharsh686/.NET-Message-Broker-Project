using System.ComponentModel.DataAnnotations;

namespace Message_Broker.Models
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
