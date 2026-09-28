namespace CodeRoom.WebForms.Models
{
    /// <summary>A unlockable badge defined by the platform.</summary>
    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }
        public int XpReward { get; set; }

        public Achievement()
        {
            Name = string.Empty;
            Description = string.Empty;
            Code = string.Empty;
            Icon = "★";
        }
    }
}
