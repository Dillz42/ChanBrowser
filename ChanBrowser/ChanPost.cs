using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace ChanBrowser
{
    public class ChanPost
    {
        public string board;

        public int no;
        public int resto;
        public int sticky;
        public int closed;
        public int archived;
        public int archived_on;
        public string now;
        public int time;
        public string name;
        public string trip;
        public string id;
        public string capcode;
        public string country;
        public string country_name;
        public string sub { get; set; }
        public string com { get; set; }
        public long tim;
        public string filename;
        public string ext;
        public int fsize;
        public string md5;
        public int w;
        public int h;
        public int tn_w;
        public int tn_h;
        public int filedeleted;
        public int spoiler;
        public int custom_spoiler;
        public int omitted_posts;
        public int omitted_images;
        public int replies;
        public int images;
        public string replies_images_string { get; set; }
        public int bumplimit;
        public int imagelimit;
        string[] capcode_replies;
        public int last_modified;
        public string tag;
        public string semantic_url;

        public List<ChanPost> replyList = new List<ChanPost>();

        public string imageUrl { get; set; } = Global.DEFAULT_IMAGE;
        private System.Windows.Media.ImageSource _imageSource;

        public System.Windows.Media.ImageSource imageSource
        {
            get
            {
                _imageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(imageUrl, UriKind.Absolute));
                return _imageSource;
            }
            set
            {
                _imageSource = value;
            }
        }

        //public List<string> imageUrlList = new List<string>();

        public ChanPost()
        {

        }
        public ChanPost(JObject jsonObject, string board)
        {
            this.board = board;

            no = (int)(jsonObject["no"] ?? 0);
            resto = (int)(jsonObject["resto"] ?? 0);
            sticky = (int)(jsonObject["sticky"] ?? 0);
            closed = (int)(jsonObject["closed"] ?? 0);
            archived = (int)(jsonObject["archived"] ?? 0);
            archived_on = (int)(jsonObject["archived_on"] ?? 0);
            now = (string)(jsonObject["now"] ?? "");
            time = (int)(jsonObject["time"] ?? 0);
            name = (string)(jsonObject["name"] ?? "");
            trip = (string)(jsonObject["trip"] ?? "");
            id = (string)(jsonObject["id"] ?? "");
            capcode = (string)(jsonObject["capcode"] ?? "");
            country = (string)(jsonObject["country"] ?? "");
            country_name = (string)(jsonObject["country_name"] ?? "");
            sub = (string)(jsonObject["sub"] ?? "");
            com = (string)(jsonObject["com"] ?? "");
            tim = (long)(jsonObject["tim"] ?? 0);
            filename = (string)(jsonObject["filename"] ?? "");
            ext = (string)(jsonObject["ext"] ?? "");
            fsize = (int)(jsonObject["fsize"] ?? 0);
            md5 = (string)(jsonObject["md5"] ?? "");
            w = (int)(jsonObject["w"] ?? 0);
            h = (int)(jsonObject["h"] ?? 0);
            tn_w = (int)(jsonObject["tn_w"] ?? 0);
            tn_h = (int)(jsonObject["tn_h"] ?? 0);
            filedeleted = (int)(jsonObject["filedeleted"] ?? 0);
            spoiler = (int)(jsonObject["spoiler"] ?? 0);
            custom_spoiler = (int)(jsonObject["custom_spoiler"] ?? 0);
            omitted_posts = (int)(jsonObject["omitted_posts"] ?? 0);
            omitted_images = (int)(jsonObject["omitted_images"] ?? 0);
            replies = (int)(jsonObject["replies"] ?? 0);
            images = (int)(jsonObject["images"] ?? 0);
            replies_images_string = "R: " + replies + " / I:" + images;
            bumplimit = (int)(jsonObject["bumplimit"] ?? 0);
            imagelimit = (int)(jsonObject["imagelimit"] ?? 0);
            //string[] capcode_replies;;
            last_modified = (int)(jsonObject["last_modified"] ?? 0);
            tag = (string)(jsonObject["tag"] ?? "");
            semantic_url = (string)(jsonObject["semantic_url"] ?? "");

            if (ext != "")
                imageUrl = (Global.BASE_IMAGE_URL + Global.currentBoard + "/" + tim + "s.jpg");
            //else
            //    imageUrlList.Add("http://s.4cdn.org/image/fp/logo-transparent.png");

        }
    }
}
