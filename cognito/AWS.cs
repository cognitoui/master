using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cognito.Pages
{
    public class AWS
    {

        private AWS()
        {
            UserPools = new List<AWSUserPoolsModel>();

        }

        private static AWS instance;
        public string UserPoolId = "eu-west-1_0Kwkcfesq";

        public static AWS Instance {
            get
            {
                if (instance == null) instance = new AWS();
                return instance;
            }
        }


        public AWSUserGroupListModel GetUserGroups(string Username)
        {
            var user = JsonConvert.DeserializeObject<AWSUserGroupListModel>(RunCommand($"admin-list-groups-for-user --username {Username}").Item2);
            return user;
        }

        public void DeleteUser(string Username)
        {
            RunCommand($"admin-delete-user --username {Username}");
        }

        public void AddUserToGroup(string Username, string Groupname)
        {
            var user = JsonConvert.DeserializeObject<AWSUserGroupListModel>(RunCommand($"admin-add-user-to-group --username {Username} --group-name {Groupname}").Item2);
        }
        public void RemoveUserFromGroup(string Username, string Groupname)
        {
            var user = JsonConvert.DeserializeObject<AWSUserGroupListModel>(RunCommand($"admin-remove-user-from-group --username {Username} --group-name {Groupname}").Item2);
        }

        public AWSGroupListModel GetGroups()
        {
            var user = JsonConvert.DeserializeObject<AWSGroupListModel>(RunCommand($"list-groups").Item2);
            return user;
        }

        public AWSUserModel GetUser(string Username)
        {           
            var user = JsonConvert.DeserializeObject<AWSUserModel>(RunCommand($"admin-get-user --username {Username}").Item2);
            return user;
        }

        public AWSUsersModel GetUsers(string PaginationToken = "")
        {
            if (PaginationToken == null) return null;
            string add = "";
            if (PaginationToken.Length == 0)
            {
                Users = new List<AWSUsersModel>();
                Users.Clear();
            }
            else
            {
                add = $"--pagination-token {PaginationToken}";
            }
            var users = JsonConvert.DeserializeObject<AWSUsersModel>(RunCommand($"list-users {add}").Item2);
            Users?.Add(users);
            return users;
        }

        public bool UploadFile(string Filename, string Bucket, string BucketFilename)
        {
            var back = RunCommand($"cp " + Filename + " s3://" + Bucket + "/" + BucketFilename, "s3");
            if (back.Item1 == 0 && back.Item2.Contains("upload")) return true;
            return false;
        }

        public List<string[]> GetS3Files(string Bucket)
        {
            List<string[]> f = new List<string[]>();
            var cmd = RunCommand($"ls s3://" + Bucket, "s3");

            if (cmd.Item1 != 0 || cmd.Item3.Length > 0) return f;

            var files = cmd.Item2;

            files = files.Replace('\r', '\n').Replace("\n\n", "\n");
            foreach (var line in files.Split('\n'))
            {
                string l = line.Trim(' ', '\t');
                if (l.Trim().Length == 0) continue;
                if (l.StartsWith("PRE"))
                {
                    f.Add(new string[] { "D", l.Substring(3) });
                }
                if (System.Text.RegularExpressions.Regex.IsMatch(l,"^[0-9- :]*.*"))
                {
                    int oldlen = 0;
                    while (oldlen != l.Length) 
                    {
                        oldlen = l.Length;
                        l = l.Replace("\t", " ").Replace("  ", " ");
                    }
                    f.Add(new string[] { "F", l.Split(" ")[0] + " " + l.Split(" ")[1], l.Split(" ")[2], l.Split(' ', 4)[3] });
                }
            }
            return f;
        }

        public bool DeleteS3File(string File)
        {
            var r = RunCommand($"rm s3://" + File, "s3");
            if (r.Item1 == 0 && r.Item2.Contains("delete")) return true;
            return false;
        }

        public List<AWSUserPoolsModel> GetUserPools(string NextToken = "")
        {
            if (NextToken == null) return null;
            string add = "";
            if (NextToken.Length == 0)
            {
                UserPools = new List<AWSUserPoolsModel>();
                UserPools.Clear();
            }
            else
            {
                add = $"--next-token {NextToken}";
            }
            var users = JsonConvert.DeserializeObject<AWSUserPoolsModel>(RunCommand($"list-user-pools --max-results 5 {add}").Item2);
            UserPools.Add(users);
            if (users?.NextToken != null && users?.NextToken.Length == 0)
            {
                GetUserPools(users.NextToken);
            }
            return UserPools;
        }

        public List<AWSUsersModel> Users;
        public List<AWSUserPoolsModel> UserPools;

     

        public (int, string, string) RunCommand(string Command, string Namespace = "cognito-idp")
        {
            string UserPoolArg = $"--user-pool-id {UserPoolId}";
            if (Command.StartsWith("list-user-pools") || Namespace != "cognito-idp") UserPoolArg = "";
            var proc = new System.Diagnostics.Process();
            string tx = "";

            try
            {
                proc.StartInfo = new System.Diagnostics.ProcessStartInfo { FileName = "aws", Arguments = $"{Namespace} {Command} {UserPoolArg}", CreateNoWindow = true, RedirectStandardOutput = true, UseShellExecute = false };
                proc.Start();
                var reader = proc.StandardOutput;
                while (!proc.HasExited && reader.Peek() > 0)
                {
                    tx += reader.ReadToEnd();
                }
            } catch (Exception E)
            {
                return (-1, tx, E.Message);
            }
            return (proc.ExitCode, tx, "");
        }

    }
    
    public class AWSUsersModel
    {
        public string PaginationToken;
        public List<AWSUserModel> Users;
    }

    public class AWSUserPoolsModel
    {
        public string NextToken;
        public List<AWSUserPoolModel> UserPools;
    }

    public class AWSUserGroupListModel
    {
        public List<AWSUserGroupModel> Groups;
    }

    public class AWSGroupListModel : AWSUserGroupListModel
    {
    }

    public class AWSGroupModel : AWSUserGroupModel
    {

    }

    public class AWSUserGroupModel
    {
        public string GroupName;
        public string CreationDate;
        public string UserPoolId;
        public string LastModifiedDate;   
    }



    public class AWSUserPoolModel
    {
        public string CreationDate;
        public string LastModifiedDate;
        public Dictionary<string,string> LambdaConfig;
        public string Id;
        public string Name;
    }

    public class AWSUserModel
    {
        public string Username;
        public bool Enabled;
        public string UserStatus;
        public string UserCreateDate;
        public List<AWSNameValueModel> UserAttributes
        {
            get { return Attributes;  }
            set { Attributes = value; }
        }

        public List<AWSNameValueModel> Attributes;
        public string UserLastModifiedDate;
        public string Get(string AttributeName)
        {
            var first = Attributes.FirstOrDefault(x => x.Name.ToLower() == AttributeName.ToLower());
            return first?.Value ?? "";
        }
    }

    public class AWSNameValueModel
    {
        public string Name;
        public string Value;
        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
    }

}
