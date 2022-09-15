using System.ComponentModel.DataAnnotations;
namespace TestFinal.Models;
#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations.Schema;


public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public string UserName { get; set; }
    
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

 [InverseProperty("Reciver")]
    public List<Request> RequestsReciver {get;set;} = new List<Request>();

     [InverseProperty("Sender")]
    public List<Request> RequestsSender { get; set; } = new List<Request>();
    public List<Like> Likes {get;set;} = new List<Like>();
    public List<Comment> Comments {get;set;} = new List<Comment>();

    public List<Post> CreatedPost { get; set; } = new List<Post>(); 

      public string Myimage { get; set; } = string.Empty;

    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string Confirm { get; set; }
    [NotMapped]
    public IFormFile Image { get; set; }
}
public class LoginUser
{
    // No other fields!
    [Required]
    public string UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}


