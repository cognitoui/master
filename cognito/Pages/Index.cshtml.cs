using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cognito.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }

        public void OnPostGetUser(string Username)
        {
            //na-yoshioka@ap.alpen-group.jp
            AwsUser = AWS.Instance.GetUser(Username);
            AwsUserGroups = AWS.Instance.GetUserGroups(Username);
            AwsGroups = AWS.Instance.GetGroups();
        }

        public void OnGetGetUser(string Username)
        {
            OnPostGetUser(Username);
        }

        public ActionResult OnPostAddUserToGroup(string Username, string Groupname)
        {
            AWS.Instance.AddUserToGroup(Username, Groupname);
            return RedirectToPage("Index","GetUser",new { Username = Username});
        }

        public ActionResult OnPostSelectUserPool(string PoolId)
        {
            AWS.Instance.UserPoolId = PoolId;
            AWS.Instance.Users = null;
            return RedirectToPage("Index");
        }

        public ActionResult OnPostRemoveUserFromGroup(string Username, string Groupname)
        {
            AWS.Instance.RemoveUserFromGroup(Username, Groupname);
            return RedirectToPage("Index", "GetUser", new { Username = Username });
        }

        public ActionResult OnPostDeleteUser(string Username)
        {
            AWS.Instance.DeleteUser(Username);
            return RedirectToPage("Index", "GetUser", new { Username = Username });
        }


        public AWSUserModel AwsUser;
        public AWSUserGroupListModel AwsUserGroups;
        public AWSGroupListModel AwsGroups;
    }
}
