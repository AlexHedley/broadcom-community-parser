using System.Configuration;
using System.Net;
using System.Web;
using System.Xml.Serialization;
using System.Xml;

using HtmlAgilityPack;

using broadcom_community_parser.Models;

namespace broadcom_community_parser
{
    public class PostCreator
    {
        public PostInfo postInfo { get; set; }

        public PostCreator()
        {
            postInfo = new PostInfo();
        }

        public void CreatePosts()
        {
            var postInfoData = postInfo.GetPostInfo();
            var documents = postInfo.GetPostDocuments(postInfoData);
            Console.WriteLine(documents.Count());

            var posts = postInfo.ConvertToPosts(documents);

            string outputFolder = ConfigurationManager.AppSettings["outputFolder"];
            // Create Folders?

            foreach (var post in posts)
            {
                try
                {
                    var invalids = Path.GetInvalidFileNameChars();
                    var newName = String.Join("_", post.Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                    newName = newName.Trim();

                    // Get image src
                    // From String
                    var doc = new HtmlDocument();
                    doc.LoadHtml(post.Description);

                    string command = "//img";
                    List<string> listImages = new();
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes(command))
                    {
                        var src = node.Attributes["src"].Value;
                        Console.WriteLine(src);

                        ////src = HttpUtility.UrlEncode(src);
                        //src = Uri.EscapeDataString(src);
                        //Console.WriteLine(src);

                        var imageFileName = Path.GetFileName(src);
                        imageFileName = HttpUtility.UrlEncode(imageFileName);
                        Console.WriteLine(imageFileName);
                        var imagesPath = Path.Combine(outputFolder, "images", imageFileName);

                        // Check if image already exists.
                        if (File.Exists(imagesPath))
                        {
                            Console.WriteLine("Image already exists on disk, no need to download");
                        }
                        else
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFileAsync(new Uri(src), imagesPath); //DownloadFile
                            }
                        }

                        // replace src with local for markdown conversion next
                        post.Description = post.Description.Replace(src, Path.Combine("images", imageFileName));
                    }

                    string fileName = $"{outputFolder}\\{newName}.md";
                    var converter = new ReverseMarkdown.Converter();
                    string result = converter.Convert(post.Description);

                    //// Check if file already exists. If yes, delete it.     
                    //if (File.Exists(fileName))
                    //{
                    //    File.Delete(fileName);
                    //}

                    //Create a new file
                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        //---
                        //title: My First Post
                        //published: 2010-11-08
                        //---
                        //
                        // <Content>

                        sw.WriteLine("---");
                        sw.WriteLine($"title: {post.Name.Trim()}");
                        sw.WriteLine("# tags:");
                        sw.WriteLine("#     - ");
                        sw.WriteLine("author: AlexHedley");
                        sw.WriteLine("# description: ");
                        sw.WriteLine($"published: {post.DateTime.ToString("yyyy-MM-dd")}");
                        sw.WriteLine("---");
                        sw.WriteLine("");
                        sw.WriteLine(result);
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
            }
        }
    }
}
