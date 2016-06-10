using System;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

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

            int size = 0;
            int.TryParse(Request["size"], out size);            
            if (size > 0)
                image = CreateThumbnail(image, size, size);
            

            Response.ContentType = "image/png";
            Response.BinaryWrite(image);

        }


        public static byte[] CreateThumbnail(byte[] PassedImage, int newWidth, int newHeight) {
            byte[] ReturnedThumbnail;

            using (MemoryStream StartMemoryStream = new MemoryStream(), NewMemoryStream = new MemoryStream()) {
                // write the string to the stream   
                StartMemoryStream.Write(PassedImage, 0, PassedImage.Length);

                // create the start Bitmap from the MemoryStream that contains the image   
                Bitmap startBitmap = new Bitmap(StartMemoryStream);

                // create a new Bitmap with dimensions for the thumbnail.   
                Bitmap newBitmap = new Bitmap(newWidth, newHeight);

                // Copy the image from the START Bitmap into the NEW Bitmap.   
                // This will create a thumnail size of the same image.   
                newBitmap = ResizeImage(startBitmap, newWidth, newHeight);

                // Save this image to the specified stream in the specified format.   
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                newBitmap.Save(NewMemoryStream, jpgEncoder, myEncoderParameters);

                // Fill the byte[] for the thumbnail from the new MemoryStream.   
                ReturnedThumbnail = NewMemoryStream.ToArray();
            }

            // return the resized image as a string of bytes.   
            return ReturnedThumbnail;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format) {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
        }

        // Resize a Bitmap   
        private static Bitmap ResizeImage(Bitmap image, int width, int height) {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage)) {
                gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }

    }
}