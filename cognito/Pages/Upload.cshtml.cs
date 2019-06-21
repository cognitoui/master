using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cognito.Pages
{
    public class UploadModel : PageModel
    {
        public void OnGet()
        {

        }

        public string Message;

        public async Task<ActionResult> OnPostUpload(IFormFile file)
        {
            var okay = await UploadFile(file);
            if (okay == null) return RedirectToPage("Upload");
            return RedirectToPage("Upload", "Uploaded", new { Message = okay });

        }

        public ActionResult OnGetRemoveFile(string file)
        {
            string bucket = "";

            var pool = AWS.Instance.UserPools.Select(x => x.UserPools.First(y => y.Id == AWS.Instance.UserPoolId).Name).ToList()[0];

            if (pool == "dtb-dev") bucket = "dtb-users-dev";
            if (pool == "dtb-stg") bucket = "dtb-users-stg";
            if (pool == "dtb-prd") bucket = "dtb-users-prd";

            if (bucket.Length == 0) return RedirectToPage("Upload");

            var r = AWS.Instance.DeleteS3File(bucket + "/" + file);

            return RedirectToPage("Upload");

        }

        public void OnGetUploaded(string[] Message)
        {
            if (System.IO.File.Exists(Message[0]))
            {
                if (Message.Length == 2)
                {
                    //bucket selection
                    string bucket = "";

                    var pool = AWS.Instance.UserPools.Select(x => x.UserPools.First(y => y.Id == AWS.Instance.UserPoolId).Name).ToList()[0];
                    
                    if (pool == "dtb-dev") bucket = "dtb-users-dev";
                    if (pool == "dtb-stg") bucket = "dtb-users-stg";
                    if (pool == "dtb-prd") bucket = "dtb-users-prd";

                    bool done = AWS.Instance.UploadFile(Message[0], bucket, Message[1]);
                    if (done)
                    {
                        this.Message = ("File " + Message[1] + " uploaded to bucket " + pool);
                    } else
                    {
                        this.Message = ("File " + Message[1] + " not uploaded");
                    }

                } else
                {
                    this.Message = "Upload Error: " + Message[2];
                }
            }
        }

        public static async Task<string[]> UploadFile(IFormFile formFile)
        {
            if (formFile == null || formFile.FileName == null || formFile.Length == 0) return null;
            var tempname = System.IO.Path.GetTempFileName();

            try
            {

                using (var fs = new System.IO.FileStream(tempname,FileMode.OpenOrCreate))
                {
                    await formFile.OpenReadStream().CopyToAsync(fs);
                }

            }
            catch (Exception ex)
            {
                return new string[] { tempname, formFile.FileName, ex.Message };
            }

            return new string[] { tempname, formFile.FileName };
        }
    }
}
