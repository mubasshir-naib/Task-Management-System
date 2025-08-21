using System.ComponentModel.DataAnnotations;

namespace Task_Management.Models
{
    public class TasksEntity
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public string Status { get; set; } // e.g., "To Do", "In Progress", "Completed"

        public string UserId { get; set; } // Foreign key to associate with user
        //public Data.ApplicationUser User { get; set; } // Navigation property
    }
}
