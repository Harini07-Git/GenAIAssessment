namespace ClaimsManagement.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public virtual ICollection<Claim> Claims { get; set; }
}
