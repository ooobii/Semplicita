using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Razor.Parser.SyntaxTree;

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
                    , Name = $"{FileName}{FileExt}"
                };

                //return model
                return output;

            }           

        }


        public HtmlString GetDisplayHtml() {
            var output = new StringBuilder();
            output.AppendLine("<ul class=\"products-list product-list-in-card pl-2 pr-2\">");
            output.AppendLine(" <li class=\"item\">");
            output.AppendLine("    <div class=\"product-img\">");
            output.AppendLine("        <img src=\"/img/upload.png\" alt=\"File\" class=\"img-size-50\">");
            output.AppendLine("    </div>");
            output.AppendLine("    <div class=\"product-info\">");
            output.AppendLine($"       <a target=\"_blank\" href=\"/tickets/{this.ParentTicket.GetTicketIdentifier()}/attachments/{this.Id}\" class=\"product-title\">{this.Name}</a>");
            output.AppendLine($"       <span class=\"product-description text-sm\">Uploaded {this.UploadedAt.ToString("MMMM d, yyyy h:mm:ss tt")}</span>");
            output.AppendLine($"       <span class=\"product-description text-sm\">by {this.Author.FullNameStandard}</span>");
            output.AppendLine("    </div>");
            output.AppendLine(" </li>");
            output.AppendLine("</ul>");

            return new HtmlString(output.ToString());
        }


    }
}