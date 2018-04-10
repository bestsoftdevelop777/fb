using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetBrowser.WinForms;
using DotNetBrowser;
using System.IO;
namespace FBgroupsScraper
{
    public partial class ScrapeGroupForm : Form
    {
        public BrowserView webBrowser;
        public ScrapeGroupForm()
        {
            InitializeComponent();
             
             
        }

        public Form1.FBGroup scrapedGroup=new Form1.FBGroup();
        public bool FinishWork = false;

        public string webDocument;

        public void Browser_FinishLoadingFrameEvent(object sender, DotNetBrowser.Events.FinishLoadingEventArgs e)
        {
            if (!e.IsMainFrame)
            {
                return;
            }
            
            webDocument = webBrowser.Browser.GetHTML();
           // File.WriteAllText("result.txt", webDocument);
 

            scrapedGroup = Form1.instance.group;

            updateGroupCurrentIdName();
            scrapedGroup.posts = findPosts(scrapedGroup.nameID, scrapedGroup.ID);
            FinishWork = true;

            //webBrowser1.Dispose();
          //  webBrowser1 = null;
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
        
        public List<Form1.FBPost> findPosts(string groupName, string groupId)
        {

            List<Form1.FBPost> ids = new List<Form1.FBPost>();
            Dictionary<string, Boolean> postIDs = new Dictionary<string, bool>();

            string postLinkStart = "id=" + (char)34 + "mall_post_";

            int startInd = 0;
            while (true)
            {
                int ind = webDocument.IndexOf(postLinkStart, startInd);

                if (ind == -1) break;
                startInd = ind + 1;


                Form1.FBPost post = new Form1.FBPost();

                int i;
                for (i = ind + 14; i < webDocument.Length; i++)
                {
                    if (webDocument[i] < '0' || webDocument[i] > '9') break;
                    post.ID += webDocument[i];
                }
                if (webDocument[i] != ':') continue;
                if (postIDs.Contains(new KeyValuePair<string, Boolean>(post.ID, true))) continue;

                postIDs.Add(post.ID, true);
                post.URL = "https://www.facebook.com/" + groupId + "_" + post.ID;

                ids.Add(post);
            }
            while (ids.Count > 13) ids.RemoveAt(ids.Count - 1);

            return ids;
        }

        public void updateGroupCurrentIdName()
        {
            scrapedGroup.ID = "";
            int ind = webDocument.IndexOf("fb://group/?id=");
            if (ind == -1) return;
            for (int i = ind + 15; webDocument[i] != (char)(34); i++)
            {
                scrapedGroup.ID += webDocument[i];
            }

            scrapedGroup.name = "";
            ind = webDocument.IndexOf("<title id="+(char)34+"pageTitle"+(char)34+">");
            if (ind == -1) return;
            for (int i = ind + 22; webDocument[i] != '<'; i++)
            {
                scrapedGroup.name += webDocument[i];
            }
            scrapedGroup.name.Replace(@"\", "");
        }

        private void ScrapeGroupForm_Load(object sender, EventArgs e)
        {

        }
    }
}
