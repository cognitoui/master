using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace cognito.Pages
{
    public class UsersModel : PageModel
    {
        public void OnGet()
        {

        }

        public void OnPostGetUsers()
        {
            AWS.Instance.GetUsers();
        }

        public JsonResult OnGetLoadUsers(string PaginationToken)
        {
            var users = AWS.Instance.GetUsers(PaginationToken);
            return new JsonResult(users);
        }

        public List<AWSUsersModel> Users {
            get
            {
                return AWS.Instance.Users;
            }    
        }


    }
}
