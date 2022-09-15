using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace TestFinal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment WebHostEnvironment;

    private MyContext _context;

    // here we can "inject" our context service into the constructor
    public HomeController(ILogger<HomeController> logger, MyContext context,IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _context = context;
        WebHostEnvironment = webHostEnvironment;

    }
  public async Task<IActionResult> Index(string searchString)

    {
        // Pjesa me request

        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");
        User useriLoguar =_context.Users.FirstOrDefault(e => e.UserId == id);

        ViewBag.iLoguari = useriLoguar;

                    ViewBag.Allusers = _context.Users
                        .Include(e => e.RequestsReciver)
                        .Include(e => e.RequestsSender)
                        .Where(e => e.UserId != id)
                        .Where(e =>
                                     (e.RequestsSender.Any(f => f.ReciverId == id) == false)
                                    && (e.RequestsReciver.Any(f => f.SenderId == id) == false)
                        ).Take(3)
                        .ToList();

                    ViewBag.request = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                                        .Where(e => e.ReciverId == id)
                                        .Where(e => e.Accepted == false)
                                        .ToList();

                    ViewBag.friends = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender)
                                        .Where(e => (e.ReciverId == id ) || (e.SenderId == id ))
                                        .Where(e => e.Accepted == true)
                                        .ToList();

                    ViewBag.posts = _context.Posts.Include(e => e.Creator).Include(e=>e.Likes).Include(e=>e.Comments)
                                                    .ThenInclude(e=>e.UseriQekomenton).ThenInclude(e=>e.RequestsReciver)
                                                    .OrderByDescending(e=>e.CreatedAt)
                                                    
                                                    .Where(e=>(e.Creator.RequestsSender.Where(e=> e.Accepted==true ).Any(e => e.ReciverId == id) == true) 
                                                    || (e.Creator.RequestsReciver.Where(e=> e.Accepted==true).Any(e => e.SenderId == id) == true) 
                                                    || e.Creator.UserId == id  )                                                
                                                    .ToList();

                                                 

    var searchfrineds = from m in _context.Users 
                 select m;
    ViewBag.searchfrineds =  searchfrineds.Take(3);
    
    if (!String.IsNullOrEmpty(searchString))
    {
       ViewBag.searchfrineds  = searchfrineds.Where(s => s.FirstName!.Contains(searchString));
    }
                                                    


        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {


        if (HttpContext.Session.GetInt32("userId") == null)
        {

            return View();
        }

        return RedirectToAction("Index");

    }
    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
      
            // If a User exists with provided email
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                // Manually add a ModelState error to the Email field, with provided
                // error message
                ModelState.AddModelError("UserName", "UserName already in use!");

                return View();
                // You may consider returning to the View at this point
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("userId", user.UserId);

            return RedirectToAction("Index");
        
    }

    [HttpPost("Login")]
    public IActionResult LoginSubmit(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.UserName == userSubmission.UserName);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("Register");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("Register");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId);

            return RedirectToAction("Index");
        }

        return View("Register");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {

        HttpContext.Session.Clear();
        return RedirectToAction("register");
    }


    // Pjesa me request


    [HttpGet("SendR/{id}")]
    public IActionResult SendR(int id)
    {  
        int idFromSession = (int)HttpContext.Session.GetInt32("userId");
        if (_context.Requests.Any(u => (u.SenderId == idFromSession) && (u.ReciverId == id)))
            {
      
         return RedirectToAction("index");
            }
            else{

        Request newRequest = new Models.Request()
        {
            SenderId = idFromSession,
            ReciverId = id,

        };
        _context.Requests.Add(newRequest);
        _context.SaveChanges();
       
                return RedirectToAction("index"); 
            }
           

    }
    [HttpGet("AcceptR/{id}")]
    public IActionResult AcceptR(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        requestii.Accepted = true;
        
        _context.SaveChanges();
        return RedirectToAction("index");
    }
    [HttpGet("DeclineR/{id}")]
    public IActionResult Decline(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requestii);
        _context.SaveChanges();
        return RedirectToAction("index");
    }
    [HttpGet("RemoveF/{id}")]
    public IActionResult RemoveF(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requestii);
        _context.SaveChanges();
        return RedirectToAction("index");
    }

    //Perfundon Request

    //pjesa me poste

    public IActionResult PostAdd(int id)
    {
        ViewBag.id = id;
        return View();
    }

    [HttpPost]
    public IActionResult PostCreate(Post marrNgaView)
    {
        string StringFileName = UploadFile(marrNgaView);
        var post = new Post( ){
            Description = marrNgaView.Description,
            Myimage = StringFileName
        };


        // if (ModelState.IsValid)
        // {
            int id = (int)HttpContext.Session.GetInt32("userId");


            post.UserId = id;
            _context.Posts.Add(post);
            _context.SaveChanges();
            return RedirectToAction("Index");
        // }
       
    }

    private string UploadFile(Post marrNgaView)
    {
       string fileName = null;
       if(marrNgaView.Image!=null){
        string Uploaddir = Path.Combine(WebHostEnvironment.WebRootPath,"Images");
        fileName = Guid.NewGuid().ToString() + "-" + marrNgaView.Image.FileName;
        string filePath = Path.Combine(Uploaddir,fileName);
        using (var filestream = new FileStream(filePath,FileMode.Create))
        {
                marrNgaView.Image.CopyTo(filestream);
        }
       }
       return fileName;
    }
      private string UploadFile(User marrNgaView)
    {
       string fileName = null;
       if(marrNgaView.Image!=null){
        string Uploaddir = Path.Combine(WebHostEnvironment.WebRootPath,"Images");
        fileName = Guid.NewGuid().ToString() + "-" + marrNgaView.Image.FileName;
        string filePath = Path.Combine(Uploaddir,fileName);
        using (var filestream = new FileStream(filePath,FileMode.Create))
        {
                marrNgaView.Image.CopyTo(filestream);
        }
       }
       return fileName;
    }

    public IActionResult Like(int id, int id2)
    {

            Like mylike = new Like()
            {
                UserId = id,
                PostId = id2
            };
            _context.Add(mylike);
            _context.SaveChanges();
            return RedirectToAction("Index");
    }
     public IActionResult UnLike(int id, int id2)
    {
            Like dblike = _context.Likes.FirstOrDefault(e=>(e.UserId == id) && (e.PostId == id2));
           
            _context.Remove(dblike);
            _context.SaveChanges();
            return RedirectToAction("Index");
    }


    public IActionResult CommentCreate(int id, int id2,string content)
    {

       
            Comment mylike = new Comment()
            {
                UserId = id,
                PostId = id2,
                content=content
            };
            _context.Add(mylike);
            _context.SaveChanges();
            return RedirectToAction("Index");
        

    }
   
   public IActionResult Profilepicadd()
    {
        
        return View();
    }
    public IActionResult Profilepicsave(User marrNgaView)
    {
        int? id= HttpContext.Session.GetInt32("userId");
  string StringFileName = UploadFile(marrNgaView);
       
        User editUser = _context.Users.First(e => e.UserId == id);
        editUser.Myimage = StringFileName;
            _context.SaveChanges();
            return RedirectToAction("Index");
    
       
    }

    //    public IActionResult Like(int id, int id2)
    // {

    //         Like mylike = new Like()
    //         {
    //             UserId = id,
    //             PostId = id2
    //         };
    //         _context.Add(mylike);
    //         _context.SaveChanges();
    //         return RedirectToAction("Index");
    // }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
