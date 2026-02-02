using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AP.Common.Data.Identity.Entities;

[Table("Contacts")]
public class Contact
{
    [Key]
    public int ContactId { get; set; }

    [MaxLength(128)]
    public required string ContactName { get; set; }

    [EmailAddress]
    [MaxLength(128)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(64)]
    public string? Title { get; set; }

    [MaxLength(128)]
    public string? Address { get; set; }

    public required DateTime CreatedOn { get; set; }
}