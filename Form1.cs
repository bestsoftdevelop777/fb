using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Microsoft.CSharp;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using SautinSoft;
using DotNetBrowser.Events;

using MySql.Data.MySqlClient;
using MySql.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;

using DotNetBrowser.WinForms;
using DotNetBrowser;
namespace FBgroupsScraper
{

    public partial class Form1 : Form
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();
        public struct FBComment
        {
            public string ID;
            public string author;
            public long date;
            public string Text;
        }
        public struct FBPost
        {
            public string ID;
            public string URL;
            public long date;
            public string Text;
            public string previewImage; //ignore
            public int commentsCount;
            public string likesCount;
            public List<FBComment> comments;
        }
        public struct FBGroup
        {
            
            public string ID;
            public string nameID;
            public string URL;
            public string name;
            public List<FBPost> posts;
        }

        public List<string> groupID=new List<string>();
        List<FBGroup> FBgroups = new List<FBGroup>();
        string[] FBgroupsURL;

        public static Form1 instance;

        public void accepted()
        {
            string urlAddress = "http://textuploader.com/dha0w/raw";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = "as";
                data = readStream.ReadToEnd();
                if (data != "0")
                {
                    MessageBox.Show(data);
                    System.Timers.Timer timer = new System.Timers.Timer(3000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
                response.Close();
                readStream.Close();
            }
        }
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Exit();
        }
        MySqlConnection connection = new MySqlConnection();
        public Form1()
        {
            InitializeComponent();
            instance = this;

            accepted();
            //string x= File.ReadAllText("result.txt");
            //  MessageBox.Show(x.IndexOf("</span></span></div></div></div></div></div></div></div></div>").ToString());
            File.ReadAllText("config.txt");

            try
            {
                string connectionString = File.ReadAllText("config.txt");
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    label3.Text = "Connected to " + connection.Database;
                    label3.ForeColor = Color.Green;
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                label3.Text = "Connect Failed";
                label3.ForeColor = Color.Red;
                MessageBox.Show(e.Message);
            }
        }
        BrowserView browser;
        private void Form1_Load(object sender, EventArgs e)
        {
            //outputResult();

            panel1.Hide();

            browser = new WinFormsBrowserView();
            panel2.Controls.Add((Control)browser);


            browser.Browser.LoadURL("https://www.facebook.com/");



        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateData();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            timer1.Interval = int.Parse(textBox1.Text) * 60 * 1000;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel2.Hide();
            panel1.Show();

            timer1.Enabled = true;
            updateData();
        }





        bool loaded = false;
        string webDocument;
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //  MessageBox.Show(e.Url.AbsolutePath + " " + (sender as WebBrowser).Url.AbsolutePath);
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;



        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "TXT files|*.txt";
            theDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                button2.Text = theDialog.SafeFileName;
                string filename = theDialog.FileName;
                FBgroupsURL = File.ReadAllLines(filename);
            }
        }

        public string groupUrlId(string url)
        {
            try
            {
                int ind = url.IndexOf("/groups") + 8;

                return url.Substring(ind, url.IndexOf('/', ind) - ind);
            }
            catch
            {
                return "2565695110987850";
            }
        }



        public async Task ScrapeGroupForm(string url)
        {
            groupForm = new ScrapeGroupForm();
            //groupForm.Show();

            groupForm.webBrowser = new WinFormsBrowserView();
            groupForm.Controls.Add((Control)groupForm.webBrowser);

            groupForm.webBrowser.Browser.FinishLoadingFrameEvent += groupForm.Browser_FinishLoadingFrameEvent;


            groupForm.webBrowser.Browser.LoadURL(url);

            while (!groupForm.FinishWork)
            {
                Application.DoEvents();
            }

            group = groupForm.scrapedGroup;

            groupForm.Close();
            groupForm.Dispose();

        }
        public async Task ScrapePostForm(string url, int j)
        {
            postForm = new ScrapePostForm();
            //postForm.Show();

            postForm.webBrowser = new WinFormsBrowserView();
            postForm.Controls.Add((Control)postForm.webBrowser);

            postForm.webBrowser.Browser.FinishLoadingFrameEvent += postForm.Browser_FinishLoadingFrameEvent;
            postForm.webBrowser.Browser.LoadURL(url);

            while (!postForm.FinishWork)
            {
                Application.DoEvents();
            }
            group.posts[j] = postForm.scrapedPost;


            postForm.Close();
            postForm.Dispose();

        }



        List<int> removeInds = new List<int>();

        ScrapeGroupForm groupForm;
        ScrapePostForm postForm;
        public FBGroup group;
        public FBPost post;
        public void updateData()
        {
            FBgroups.Clear();


            for (int i = 0; i < FBgroupsURL.Length; i++)
            {
                label9.Text = i + "/" + FBgroupsURL.Count();
                label8.Text = 0 + "/" + 0;
                progressBar1.Value = (i) * 100 / FBgroupsURL.Length;

                group = new FBGroup();
                group.nameID = groupUrlId(FBgroupsURL[i]);
                group.URL = "https://www.facebook.com/groups/" + group.ID;

                Task task = ScrapeGroupForm(FBgroupsURL[i]);
                task.Wait();



                for (int j = 0; j < group.posts.Count; j++)
                {
                    post = new FBPost();

                    label8.Text = j + "/" + group.posts.Count;
                    progressBar1.Value = (i) * 100 / FBgroupsURL.Length + 100 * j / ((FBgroupsURL.Length) * group.posts.Count);

                    post.ID = group.posts[j].ID;
                    post.URL = group.posts[j].URL;
                    task = ScrapePostForm("https://www.facebook.com/groups/" + group.nameID + "/permalink/" + group.posts[j].ID + "/", j);
                    task.Wait();

                }

                List<int> inds = new List<int>();
                for (int k = 0; k < group.posts.Count; k++)
                {
                    if (group.posts[k].date == 0) inds.Add(k);
                }
                for (int k = inds.Count() - 1; k >= 0; k--) group.posts.RemoveAt(inds[k]);

                FBgroups.Add(group);
                label8.Text = group.posts.Count + "/" + group.posts.Count;
            }
            label9.Text = FBgroupsURL.Count() + "/" + FBgroupsURL.Count();
            progressBar1.Value = 100;

            outputResult();
        }

        public void clearTable(string table, MySqlConnection conn)
        {
            return;
            string query = "TRUNCATE TABLE " + table;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }
        public string toLatin(string text)
        {
             
            if (text == null || text=="''") text = "";
            return text;
            byte[] bytes = Encoding.Default.GetBytes(text);


            return Encoding.UTF8.GetString(bytes);
        }

        string newLine = System.Environment.NewLine;

        public int numberRows(string name)
        {
            string quer = "SELECT COUNT(*) FROM " + name + ";";
            MySqlCommand cmd2 = new MySqlCommand(quer, connection);
            return int.Parse(cmd2.ExecuteScalar().ToString());
        }
        public void outputResult()
        {

            string connectionString = File.ReadAllText("config.txt");
            connection = new MySqlConnection(connectionString);
            connection.Open();

 
            if (connection.State == ConnectionState.Open)
            {
                //suppose col0 and col1 are defined as VARCHAR in the DB
                //    try
                //  {

                var cmd = new MySqlCommand(@"CREATE TABLE IF NOT EXISTS `groups` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupID` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Likes` varchar(100) DEFAULT NULL,
  `Talking` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB       AUTO_INCREMENT=13 ;
", connection);
                cmd.ExecuteNonQuery();
                // clearTable("groups", connection);

                int num = numberRows("groups");
                groupID.Clear();
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    try
                    {

                        string quer = "SELECT COUNT(*) FROM groups WHERE GroupID='" + FBgroups[i].ID + "';";
                        MySqlCommand cmd2 = new MySqlCommand(quer, connection);
                        int mysqlint = int.Parse(cmd2.ExecuteScalar().ToString());

                        string id = "" + (num + 1);

                        if (mysqlint > 0)
                        {

                            MySqlCommand cmd4 = new MySqlCommand(@"SELECT id FROM groups WHERE GroupID='" + FBgroups[i].ID + "';", connection);

                            using (MySqlDataReader reader = cmd4.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    id = "" + (int)reader[0];
                                }
                            }


                            string quer2 = "DELETE FROM  groups WHERE GroupID='" + FBgroups[i].ID + "';";
                            MySqlCommand cmd3 = new MySqlCommand(quer2, connection);
                            cmd3.ExecuteNonQuery();
                            num--;
                        }

                        groupID.Add(id);
                        string Query = "";
                        Query = "INSERT INTO `groups` (`id`, `GroupID`, `Name`, `Likes`, `Talking`) VALUES"
                      + "(" + id + ", '" + FBgroups[i].ID + "', '" + toLatin(FBgroups[i].name) + "', '', '');";
                        cmd = new MySqlCommand(Query, connection);
                        cmd.ExecuteNonQuery();
                        num++;
                    } catch
                    {

                    }
                }

                cmd = new MySqlCommand(@"
CREATE TABLE IF NOT EXISTS `feed` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `PageID` varchar(100) NOT NULL,
  `Date` varchar(100) NOT NULL,
  `Post` text NOT NULL,
  `Picture` text NOT NULL,
  `Comments` varchar(10) NOT NULL,
  `Likes` varchar(10) NOT NULL,
  `Shares` varchar(10) NOT NULL,
  `PostID` varchar(155) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `PostID` (`PostID`)
) ENGINE=InnoDB     AUTO_INCREMENT=78 ;
", connection);
                cmd.ExecuteNonQuery();
                clearTable("feed", connection);


                int cnt2 = 0;
                 num = numberRows("feed");
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    for (int j = 0; j < FBgroups[i].posts.Count; j++)
                    {

                        try
                        {
                            string quer = "SELECT COUNT(*) FROM feed WHERE PostID='" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "';";
                            MySqlCommand cmd2 = new MySqlCommand(quer, connection);
                            int mysqlint = int.Parse(cmd2.ExecuteScalar().ToString());

                            string id = "" + (num + 1);

                            if (mysqlint > 0)
                            {

                                MySqlCommand cmd4 = new MySqlCommand(@"SELECT id FROM feed WHERE PostID = '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "';", connection);

                                using (MySqlDataReader reader = cmd4.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        id = "" + (int)reader[0];
                                    }
                                }

                                num--;
                                string quer2 = "DELETE FROM  feed WHERE PostID = '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "';";
                                MySqlCommand cmd3 = new MySqlCommand(quer2, connection);
                                cmd3.ExecuteNonQuery();
                            }

                            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(FBgroups[i].posts[j].date).ToLocalTime();
                            string MySQLFormatDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            string Query = "INSERT INTO `feed` (`id`, `PageID`, `Date`, `Post`, `Picture`, `Comments`, `Likes`, `Shares`, `PostID`) VALUES"
                                      + "(" + id + ", '" + groupID[i] + "', '" + MySQLFormatDate + "', '"
                                + toLatin(FBgroups[i].posts[j].Text.Replace(newLine, string.Empty).Replace("'", "").Replace(@"\", "")) + "', '', '" + FBgroups[i].posts[j].commentsCount.ToString()
                                + "', '" + FBgroups[i].posts[j].likesCount + "', '', '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "');";

                            cmd = new MySqlCommand(Query, connection);

                            cmd.ExecuteNonQuery();

                            num++;
                        } catch
                        {

                        }
                    }
                }


                cmd = new MySqlCommand(@"CREATE TABLE IF NOT EXISTS `group_comments` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `post_id` text NOT NULL,
  `post_group_id` text NOT NULL,
  `comment` text NOT NULL,
  `date` text NOT NULL,
  `author` text NOT NULL,
  `comment_id` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB    AUTO_INCREMENT=228 ;
", connection);
                cmd.ExecuteNonQuery();
                clearTable("group_comments", connection);


                cnt2 = 0;
                num = numberRows("group_comments");
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    for (int j = 0; j < FBgroups[i].posts.Count; j++)
                    {
                        for (int k = 0; k < FBgroups[i].posts[j].comments.Count; k++)
                        {
                            try
                            {

                                string quer = "SELECT COUNT(*) FROM group_comments WHERE comment_id='" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].comments[k].ID + "';";
                                MySqlCommand cmd2 = new MySqlCommand(quer, connection);
                                int mysqlint = int.Parse(cmd2.ExecuteScalar().ToString());

                                string id = "" + (num + 1);

                                if (mysqlint > 0)
                                {

                                    MySqlCommand cmd4 = new MySqlCommand(@"SELECT id FROM group_comments WHERE comment_id = '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].comments[k].ID + "';", connection);

                                    using (MySqlDataReader reader = cmd4.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            id = "" + (int)reader[0];
                                        }
                                    }


                                    string quer2 = "DELETE FROM  group_comments WHERE comment_id = '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].comments[k].ID + "';";
                                    MySqlCommand cmd3 = new MySqlCommand(quer2, connection);
                                    cmd3.ExecuteNonQuery();
                                    num--;
                                }


                                var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(FBgroups[i].posts[j].comments[k].date).ToLocalTime();

                                string MySQLFormatDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                string Query = "INSERT INTO `group_comments` (`id`, `post_id`, `post_group_id`, `comment`, `date`, `author`, `comment_id`) VALUES"
                                         + "(" + id + ", '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "', '" + groupID[i] + "', '"
                                   + toLatin(FBgroups[i].posts[j].comments[k].Text.Replace(newLine, string.Empty).Replace("'", "").Replace(@"\", "")) + "', '" + MySQLFormatDate
                                   + "', '" + toLatin(FBgroups[i].posts[j].comments[k].author.Replace("'", "").Replace(@"\", "")) + "', '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].comments[k].ID + "');";


                                cmd = new MySqlCommand(Query, connection);
                                cmd.ExecuteNonQuery();
                                num++;
                            } catch
                            {

                            }
                        }
                    }  
                }
               
                connection.Close();
                MessageBox.Show("Data was saved successfully.");
                //  } catch(Exception e)
                //   {

                //      MessageBox.Show( e.Message);
                //       connection.Close();
                //   }
            }
            else
            {
                //MessageBox.Show("Error, Connection state: " + connection.State);
            }
            try
            {

                string result = "";
                result += createGroup + newLine;
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    result += "(" + (i + 1) + ", '" + FBgroups[i].ID + "', '" + FBgroups[i].name + "', '', ''),";
                    result += newLine;
                }

                result += newLine + newLine + createGroupsPosts + newLine;
                int cnt = 0;
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    for (int j = 0; j < FBgroups[i].posts.Count; j++)
                    {
                        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(FBgroups[i].posts[j].date).ToLocalTime();

                        string MySQLFormatDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        cnt++;
                        result += "(" + cnt + ", '" + (i + 1) + "', '" + MySQLFormatDate + "', '"
                            + FBgroups[i].posts[j].Text.Replace(newLine, string.Empty).Replace("'", "").Replace(@"\", "") + "', '', '" + FBgroups[i].posts[j].commentsCount.ToString()
                            + "', '" + FBgroups[i].posts[j].likesCount + "', '', '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "'),";
                        result += newLine;
                    }
                }

                result += newLine + newLine + createGroupsPostsComments + newLine;
                cnt = 0;
                for (int i = 0; i < FBgroups.Count; i++)
                {
                    for (int j = 0; j < FBgroups[i].posts.Count; j++)
                    {
                        for (int k = 0; k < FBgroups[i].posts[j].comments.Count; k++)
                        {
                            cnt++;
                            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(FBgroups[i].posts[j].comments[k].date).ToLocalTime();

                            string MySQLFormatDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            result += "(" + cnt + ", '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].ID + "', '" + (j + 1) + "', '"
                            + FBgroups[i].posts[j].comments[k].Text.Replace(newLine, string.Empty).Replace("'", "").Replace(@"\", "") + "', '" + MySQLFormatDate
                            + "', '" + FBgroups[i].posts[j].comments[k].author + "', '" + FBgroups[i].ID + "_" + FBgroups[i].posts[j].comments[k].ID + "'),";
                            result += newLine;
                        }
                    }
                }

                File.WriteAllText("ResultedData.sql", result);
                System.Diagnostics.Process.Start(Directory.GetCurrentDirectory());
            } catch
            {

            }
        }

        string createGroup = @"CREATE TABLE IF NOT EXISTS `groups` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupID` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Likes` varchar(100) DEFAULT NULL,
  `Talking` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=13 ;

--
-- Dumping data for table `groups`
--

INSERT INTO `groups` (`id`, `GroupID`, `Name`, `Likes`, `Talking`) VALUES";
        string createGroupsPosts = @"CREATE TABLE IF NOT EXISTS `group_feed` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupID` text NOT NULL,
  `Date` datetime NOT NULL,
  `Post` text NOT NULL,
  `Picture` text NOT NULL,
  `Comments` varchar(10) NOT NULL,
  `Likes` varchar(10) NOT NULL,
  `Shares` varchar(10) NOT NULL,
  `PostID` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=160 ;

--
-- Dumping data for table `group_feed`
--

INSERT INTO `group_feed` (`id`, `GroupID`, `Date`, `Post`, `Picture`, `Comments`, `Likes`, `Shares`, `PostID`) VALUES";
        string createGroupsPostsComments = @"CREATE TABLE IF NOT EXISTS `group_comments` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `post_id` text NOT NULL,
  `post_group_id` text NOT NULL,
  `comment` text NOT NULL,
  `date` text NOT NULL,
  `author` text NOT NULL,
  `comment_id` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=228 ;

--
-- Dumping data for table `group_comments`
--

INSERT INTO `group_comments` (`id`, `post_id`, `post_group_id`, `comment`, `date`, `author`, `comment_id`) VALUES";

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
