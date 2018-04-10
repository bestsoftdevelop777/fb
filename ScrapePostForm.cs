using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SautinSoft;
using DotNetBrowser.WinForms;
using DotNetBrowser;
using System.IO;
using System;
namespace FBgroupsScraper
{
    public partial class ScrapePostForm : Form
    {
        public BrowserView webBrowser;
        public ScrapePostForm()
        {
            InitializeComponent();
        }

        public bool FinishWork = false;
        public Form1.FBPost scrapedPost = new Form1.FBPost();

        public string webDocument;

        public void Browser_FinishLoadingFrameEvent(object sender, DotNetBrowser.Events.FinishLoadingEventArgs e)
        {
            if (!e.IsMainFrame)
            {
                return;
            }
            scrapedPost = Form1.instance.post;
            webDocument = webBrowser.Browser.GetHTML();
            //  File.WriteAllText("result.txt", webDocument);

            scrapePost();
            FinishWork = true;

            // webBrowser1.Dispose();
            // webBrowser1 = null;
            //System.GC.Collect();
            // System.GC.WaitForPendingFinalizers();
            // System.GC.Collect();
        }

        public void scrapePost()
        {
            scrapedPost.date = getDateFromPost();
           
            if (!isDateOkey(scrapedPost.date)) { scrapedPost.date = 0; return; }
            scrapedPost.Text = getTextFromPost();
            scrapedPost.likesCount = getLikesFromPost();
            scrapedPost.comments = scrapeComments();
            scrapedPost.commentsCount = scrapedPost.comments.Count;
        }
        

        public string convertHtmlToText(string html)
        {
            SautinSoft.HtmlToRtf h = new SautinSoft.HtmlToRtf();
            string htmlFile = @"d:\Resurrection.html";
            string htmlString = html;

            // Start the conversion.
            h.OutputFormat = HtmlToRtf.eOutputFormat.TextAnsi;

            string s = h.ConvertString(htmlString);
            int ind = s.IndexOf("Trial");
            string reals = s;
            if (ind != -1) reals = s.Substring(0, ind);
            return reals;
        }

        public string getTextFromPost()
        {
            //webDocument = @"</SPAN></SPAN></DIV></DIV></DIV></DIV></DIV></DIV></DIV></DIV>";
            int l = webDocument.LastIndexOf("</span></span></div></div></div></div></div></div></div></div>");
            if (l == -1) return "";

            int r = webDocument.LastIndexOf("<div class=" + (char)34 + "_3x-2" + (char)34 + " data-ft=");
            string s = "";
            try
            {
                s = webDocument.Substring(l, r - l);
            }
            catch
            {
                s = "2565695110987851";
            }
            string realtext = convertHtmlToText(s);

            // int ind = realtext.IndexOf("Trial version");
            // realtext.Remove(realtext.Length-30, realtext.Length - ind);
            // richTextBox1.AppendText(realtext + "\u2028");
            return realtext;
        }

        private bool isDateOkey(long ts)
        {

          
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ts).ToLocalTime();
          

            var hours = (System.DateTime.Now - dt).TotalHours;
            if (hours > 24) return false;
            return true;
        }

        public long getDateFromPost()
        {
            int r = webDocument.IndexOf("data-shorten="+(char)34+"1"+(char)34+" class="+(char)34+"_5ptz");
          //  File.WriteAllText("ragaca.txt", webDocument);
            if (r == -1) return 0;
            r = r -3;
            int l = r;
            for (int i = r; ; i--)
            {
                if (webDocument[i] == (char)34) break;
                l = i;
            }
            long s = 0;
            try
            {
                s = long.Parse(webDocument.Substring(l, r - l + 1));
            }
            catch
            {
                s = 0;
            }

            // int ind = realtext.IndexOf("Trial version");
            // realtext.Remove(ind, realtext.Length - ind);
            //richTextBox1.AppendText(realtext + "\u2028");
            return s;
        }

        public string getLikesFromPost()
        {
            int r = webDocument.IndexOf("reactionsentences:{current:{text:" + (char)34);
            if (r == -1) return "0";
            r = r - 3;
            int l = r;
            for (int i = r; ; i--)
            {
                if (webDocument[i] == (char)34) break;
                l = i;
            }
            string s = "";
            try
            {
                s = webDocument.Substring(l, r - l + 1);
            }
            catch
            {
                s = "2565695110987853";
            }
            //richTextBox1.AppendText("Likes: "+ s + "\u2028");

            return  s ;
        }

        List<string> scrapeCommentsIds()
        {
            List<string> results = new List<string>();

            int ind = webDocument.IndexOf(",clienthasall:");

            int r = -1;
            for (int i = ind; i > 0; i--)
            {
                if (webDocument[i] == '[') break;
                if (webDocument[i] == (char)34) r = i;

                if (webDocument[i] == '_')
                {
                    try
                    {
                        results.Add(webDocument.Substring(i + 1, r - i - 1));
                    }
                    catch
                    {

                        string s = "2565695110987853";
                        results.Add(s);
                    }
                }
            }
            results.Reverse();
            return results;
        }
        List<string> scrapeCommentsAuthors()
        {
            List<string> results = new List<string>();
            int startInd = 0;
            int l = 0, r = 0;

            while (true)
            {
                int ind = webDocument.IndexOf(",author_name:" + (char)34, startInd);
                if (ind == -1) return results;
                startInd = ind + 1;
                l = ind + 14;
                r = l;
                for (int i = l; ; i++)
                {
                    if (webDocument[i] == (char)34) break;
                    r = i;
                }
                try
                {
                    results.Add(webDocument.Substring(l, r - l + 1));
                }
                catch
                {

                    string s = "2565695110987854";
                    results.Add(s);
                }

            }

        }
        List<long> scrapeCommentsDates()
        {
            List<long> results = new List<long>();

            int startInd = 0;
            int l = 0, r = 0;

            while (true)
            {
                int ind = webDocument.IndexOf("timestamp:{time:", startInd);
                if (ind == -1) return results;
                startInd = ind + 1;
                l = ind + 16;
                r = l;
                for (int i = l; ; i++)
                {
                    if (webDocument[i] <'0' || webDocument[i]>'9') break;
                    r = i;
                }

                try
                {
                    results.Add(long.Parse( webDocument.Substring(l, r - l + 1)));
                }
                catch
                {

                    string s = "2565695110987855";
                    results.Add(0);
                }

            }
        }
        List<string> scrapeCommentsTexts()
        {

            List<string> results = new List<string>();

            int startInd = 0;
            int l = 0, r = 0;

            while (true)
            {
                int ind = webDocument.IndexOf("{body:{text:" + (char)34, startInd);
                if (ind == -1) return results;
                ind = ind + 13;
                startInd = ind + 1;

                l = ind;
                r = l;
                for (int i = l; ; i++)
                {
                    if (webDocument[i] == (char)34) break;
                    r = i;
                }
                try
                {
                    results.Add(webDocument.Substring(l, r - l + 1));
                }
                catch
                {

                    string s = "2565695110987857";
                    results.Add(s);
                }

            }
        }
        public List<Form1.FBComment> scrapeComments()
        {
            List<Form1.FBComment> comments = new List<Form1.FBComment>();
            List<string> ids = scrapeCommentsIds();
            List<string> authors = scrapeCommentsAuthors();
            List<long> dates = scrapeCommentsDates();
            List<string> texts = scrapeCommentsTexts();
            for (int i = 0; i < ids.Count; i++)
            {
                Form1.FBComment comment = new Form1.FBComment();
                comment.author = authors[i];
                comment.date = dates[i];
                comment.Text = texts[i];
                comment.ID = ids[i];

                //richTextBox1.AppendText(ids[i] + " "+ authors[i]+" "+ texts[i]+" "+comment.date+"\u2028");

                comments.Add(comment);
            }
            return comments;
        }

        private void ScrapePostForm_Load(object sender, EventArgs e)
        {

        }
    }
}
 