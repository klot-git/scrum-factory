using System;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Drawing;
using System.IO;

namespace ScrumFactory.Services.Web {

    public partial class MemberImage : System.Web.UI.Page {

        private ITeamService teamService = null;
        private ITeamService TeamService {
            get {
                if (teamService != null)
                    return teamService;                                
                teamService = ScrumFactory.Services.Web.Application.CompositionContainer.GetExportedValueOrDefault<ITeamService>();
                

                return teamService;
            }
            
        }

        private byte[] GetNullImage() {
            System.IO.FileStream fs = System.IO.File.OpenRead(Server.MapPath("Images/whoMember.png"));
            byte[] image = new byte[fs.Length];
            fs.Read(image, 0, image.Length);
            fs.Close();
            return image;
        }

        private byte[] GetDefaultImage() {
            
            System.IO.FileStream fs = System.IO.File.OpenRead(Server.MapPath("Images/noneMember.png"));
            byte[] image = new byte[fs.Length];
            fs.Read(image, 0, image.Length);
            fs.Close();
            return image;
        }

        private byte[] GetDefaultImage(MemberProfile member) {
            var img = GetMemberNameImage(member);            
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();            

        }

        private byte[] GetContactImage() {
            System.IO.FileStream fs = System.IO.File.OpenRead(Server.MapPath("Images/contactMember.png"));
            byte[] image = new byte[fs.Length];
            fs.Read(image, 0, image.Length);
            fs.Close();
            return image;
        }

        private byte[] GetGravatarImage(string email) {

            try {
                string hash = HashEmailForGravatar(email);
                string url  = string.Format("http://www.gravatar.com/avatar/{0}?default=404", hash);
                
                var webClient = new WebClient();
                                
                return webClient.DownloadData(url);

            } catch(Exception ex) {

                //System.IO.FileStream fstream = new System.IO.FileStream(Server.MapPath("~/App_Data/log.txt"), System.IO.FileMode.Append);

                //System.IO.StreamWriter writer = new System.IO.StreamWriter(fstream);
                //writer.WriteLine(DateTime.Now);
                //writer.WriteLine("GET AVATAR:" + ex.Message);
                //if(ex.InnerException!=null)
                //    writer.WriteLine(ex.InnerException.Message);
                //writer.WriteLine("------------------------------------------------------------------------------");
                //writer.Close();

                return new byte[0];
            }
        }

        private byte[] GetImage(string memberUId) {
            
            if (String.IsNullOrEmpty(memberUId)) 
                return GetNullImage();

            if(TeamService==null)
                return GetDefaultImage();

            MemberProfile member = TeamService.GetMember(memberUId);
            if(member==null) 
                return GetDefaultImage();

            if (member.IsContactMember)
                return GetContactImage();
           
            MemberAvatar avatar = TeamService.GetMemberAvatar(memberUId);
            if (avatar != null)
                return avatar.AvatarImage;

            byte[] gravatar = GetGravatarImage(member.EmailAccount);
            if (gravatar.Length != 0)
                return gravatar;

            return GetDefaultImage(member);
        }

        public static string HashEmailForGravatar(string email) {
            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for(int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();  // Return the hexadecimal string. 
        }

        private Image GetMemberNameImage(MemberProfile member) {
            
            var name = member.FullName;
            if (String.IsNullOrEmpty(name)) {
                name = member.MemberUId;                
            }

            int idx = name.IndexOf('@');
            if (idx > 0)
                name = name.Substring(0, idx - 1);

            name = name.ToUpper();

            var names = name.Split(' ');
            if(names.Length <2) {
                names = name.Split('.');
            }
            if (names.Length <2) {
                names = name.Split('-');
            }

            string firstLetter = name.Substring(0, 1);
            string lastLetter = "";            
            if(names.Length>1)
                lastLetter = names[names.Length-1].Substring(0, 1);

            var text = firstLetter + lastLetter;

            int c1 = (int)text[0];
            int c2 = 12;
            if (text.Length > 1)
                c2 = (int)text[1];

            int r = (c1 * 10) % 220;
            int g = (c2 * 10) % 220;
            int b = ((c1+c2) * 10) % 220;

            
            var bg = Color.FromArgb(r, g, b);


            return DrawText(text, new Font(FontFamily.GenericSansSerif, 36), Color.White, bg);


        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor) {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap(100, 100);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            float px = 50 - (textSize.Width / 2);
            float py = 50 - (textSize.Height / 2);
            drawing.DrawString(text, font, textBrush, px, py);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        protected void Page_Load(object sender, EventArgs e) {
                        
            string memberUId = Request["memberUId"];

            byte[] image = GetImage(memberUId);

            Response.ContentType = "image/png";
            Response.BinaryWrite(image);

        }
    }
}