using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

[Table("users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public User()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public User(string username, string email) : this()
    {
        Username = username;
        Email = email;
    }

    public void UpdateUsername(string username)
    {
        Username = username;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"User{{Id={Id}, Username='{Username}', Email='{Email}', CreatedAt={CreatedAt}, UpdatedAt={UpdatedAt}}}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not User other) return false;
        return Id != 0 && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}