using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Security.Principal;
using System.Web;

namespace Semplicita.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public int ParentTicketId { get; set; }
        public string AuthorId { get; set; }

        public string Name { get; set; }
        public DateTime UploadedAt { get; set; }
        public string MediaUrl { get; set; }


        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser Author { get; set; }


        public static TicketAttachment ProcessUpload(HttpPostedFileBase postedFile, HttpServerUtilityBase Server, Ticket parent, IPrincipal User, ApplicationDbContext context) {
            if( postedFile.ContentLength > 31457280 ) { //30MB in binary bytes
                throw new Exception("File is too big!");
            } else {

                //get file info
                var FileName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                var FileExt = Path.GetExtension(postedFile.FileName);
                var FileNameModded = $"{Util.URLFriendly(FileName)}-{DateTime.Now.Ticks}{FileExt}";

                //save to attachment path
                postedFile.SaveAs(Path.Combine(Server.MapPath("~/AttachmentUploads/"), FileNameModded));

                //generate model for uploaded file
                var output = new TicketAttachment() {
                    UploadedAt = DateTime.Now
                    , ParentTicketId = parent.Id
                    , AuthorId = User.Identity.GetUserId()
                    , MediaUrl = "/AttachmentUploads/" + FileNameModded
                };

                //return model
                return output;

            }

        }
    }
}