namespace ClaimsManagement.API.Models;

public class Claim
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime FilingDate { get; set; }
    public string Status { get; set; }
    public int PolicyId { get; set; }
    public virtual Policy Policy { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; }
}
