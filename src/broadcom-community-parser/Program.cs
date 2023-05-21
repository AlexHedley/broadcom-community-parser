using System.Configuration;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

using broadcom_community_parser.Models;
using HtmlAgilityPack;

Console.WriteLine("Hello, World!");

XmlReaderSettings settings = new XmlReaderSettings();
settings.IgnoreWhitespace = true;

string filepath = ConfigurationManager.AppSettings["filepath"];
Console.WriteLine(filepath);

XmlSerializer serializer = new XmlSerializer(typeof(gdprAuditReportInfo));
gdprAuditReportInfo output;

using (StreamReader sr = new StreamReader(filepath))
{
    output = (gdprAuditReportInfo)serializer.Deserialize(sr);
}

var data = (gdprAuditReportInfoContactData)output.Items[1];
var documents = data.entity.Where(d => d.name == "Document");
Console.WriteLine(documents.Count());

List<Post> posts = new();

foreach(var document in documents)
{
    Post post = new();
    post.DateTime = DateTime.Parse(document.dateTime);
    //Console.WriteLine(document.dateTime);
    foreach(var column in document.column.Where(c => c.name != "Document key"))
    {
        //Console.WriteLine($"{column.name}: {column.value}");
        switch (column.name)
        {
            case "Name":
                post.Name = column.value;
                break;
            case "Description":
                post.Description = column.value;
                break;
            default:
                break;
        }
    }

    posts.Add(post);
}

Console.WriteLine(posts.Count());

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

Console.WriteLine("Finished.");
Console.WriteLine("Press <Enter> to exit.");
Console.ReadLine();