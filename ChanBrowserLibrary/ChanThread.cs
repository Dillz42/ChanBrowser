﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChanBrowserLibrary
{
    public class ChanThread
    {
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
        public string sub;
        public string com;
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
        public int bumplimit;
        public int imagelimit;
        string[] capcode_replies;
        public int last_modified;
        public string tag;
        public string semantic_url;

        public List<string> imageUrlList = new List<string>();

        public ChanThread()
        {

        }
        public ChanThread(JObject jsonObject)
        {
            no = (int)(jsonObject["no"] != null ? jsonObject["no"] : 0);
            resto = (int)(jsonObject["resto"] != null ? jsonObject["resto"] : 0);
            sticky = (int)(jsonObject["sticky"] != null ? jsonObject["sticky"] : 0);
            closed = (int)(jsonObject["closed"] != null ? jsonObject["closed"] : 0);
            archived = (int)(jsonObject["archived"] != null ? jsonObject["archived"] : 0);
            archived_on = (int)(jsonObject["archived_on"] != null ? jsonObject["archived_on"] : 0);
            now = (string)(jsonObject["now"] != null ? jsonObject["now"] : "");
            time = (int)(jsonObject["time"] != null ? jsonObject["time"] : 0);
            name = (string)(jsonObject["name"] != null ? jsonObject["name"] : "");
            trip = (string)(jsonObject["trip"] != null ? jsonObject["trip"] : "");
            id = (string)(jsonObject["id"] != null ? jsonObject["id"] : "");
            capcode = (string)(jsonObject["capcode"] != null ? jsonObject["capcode"] : "");
            country = (string)(jsonObject["country"] != null ? jsonObject["country"] : "");
            country_name = (string)(jsonObject["country_name"] != null ? jsonObject["country_name"] : "");
            sub = (string)(jsonObject["sub"] != null ? jsonObject["sub"] : "");
            com = (string)(jsonObject["com"] != null ? jsonObject["com"] : "");
            tim = (long)(jsonObject["tim"] != null ? jsonObject["tim"] : 0);
            filename = (string)(jsonObject["filename"] != null ? jsonObject["filename"] : "");
            ext = (string)(jsonObject["ext"] != null ? jsonObject["ext"] : "");
            fsize = (int)(jsonObject["fsize"] != null ? jsonObject["fsize"] : 0);
            md5 = (string)(jsonObject["md5"] != null ? jsonObject["md5"] : "");
            w = (int)(jsonObject["w"] != null ? jsonObject["w"] : 0);
            h = (int)(jsonObject["h"] != null ? jsonObject["h"] : 0);
            tn_w = (int)(jsonObject["tn_w"] != null ? jsonObject["tn_w"] : 0);
            tn_h = (int)(jsonObject["tn_h"] != null ? jsonObject["tn_h"] : 0);
            filedeleted = (int)(jsonObject["filedeleted"] != null ? jsonObject["filedeleted"] : 0);
            spoiler = (int)(jsonObject["spoiler"] != null ? jsonObject["spoiler"] : 0);
            custom_spoiler = (int)(jsonObject["custom_spoiler"] != null ? jsonObject["custom_spoiler"] : 0);
            omitted_posts = (int)(jsonObject["omitted_posts"] != null ? jsonObject["omitted_posts"] : 0);
            omitted_images = (int)(jsonObject["omitted_images"] != null ? jsonObject["omitted_images"] : 0);
            replies = (int)(jsonObject["replies"] != null ? jsonObject["replies"] : 0);
            images = (int)(jsonObject["images"] != null ? jsonObject["images"] : 0);
            bumplimit = (int)(jsonObject["bumplimit"] != null ? jsonObject["bumplimit"] : 0);
            imagelimit = (int)(jsonObject["imagelimit"] != null ? jsonObject["imagelimit"] : 0);
            //string[] capcode_replies;;
            last_modified = (int)(jsonObject["last_modified"] != null ? jsonObject["last_modified"] : 0);
            tag = (string)(jsonObject["tag"] != null ? jsonObject["tag"] : "");
            semantic_url = (string)(jsonObject["semantic_url"] != null ? jsonObject["semantic_url"] : "");

            if (ext != "")
                imageUrlList.Add(Global.BASE_IMAGE_URL + Global.currentBoard + "/" + tim + "s.jpg");
            else
                imageUrlList.Add("http://s.4cdn.org/image/fp/logo-transparent.png");

        }
    }
}
