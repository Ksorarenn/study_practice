namespace PC_Builder.Core.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public Category Parent { get; set; }
        public List<Component> Components { get; set; } = new List<Component>();
    }
}