namespace ClaimsManagement.API.Models;

public class Policy
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; }
    public string Type { get; set; }
    public decimal Coverage { get; set; }
    public virtual ICollection<Claim> Claims { get; set; }
}
