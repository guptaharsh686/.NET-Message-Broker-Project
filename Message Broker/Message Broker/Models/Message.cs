using System.ComponentModel.DataAnnotations;

namespace Message_Broker.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TopicMessage { get; set; }

        public int SubscriptionId { get; set; }

        public DateTime ExpiresAfter { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        public string MessageStatus { get; set; } = "New";


    }
}
