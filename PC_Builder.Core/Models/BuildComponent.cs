namespace PC_Builder.Core.Models
{
    public class BuildComponent
    {
        public int Id { get; set; }
        public int BuildId { get; set; }
        public Build Build { get; set; }
        public int ComponentId { get; set; }
        public Component Component { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
