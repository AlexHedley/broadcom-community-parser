using broadcom_community_parser;
using System.Configuration;

Console.WriteLine("Hello, World!");

//var postCreator = new PostCreator();
//postCreator.CreatePosts();

var postInfo = new PostInfo();
var postInfoData = postInfo.GetPostInfo();
var documents = postInfo.GetPostDocuments(postInfoData);
//var docsByDate = documents.OrderBy(d => d.dateTime).ToList();
var posts = postInfo.ConvertToPosts(documents);

string outputFolder = ConfigurationManager.AppSettings["outputFolder"];

foreach (var post in posts.OrderBy(d => d.DateTime))
{
    Console.Write(post.DateTime.ToString("yyyy-MM-dd"));
    Console.Write(" ");
    Console.Write(post.Name);
    Console.WriteLine();

    var commitUpdate = $"GIT_COMMITTER_DATE=\"{post.DateTime.ToString("dd MMM yyyy")} 12:00:00 BST\" git commit --amend --no-edit --date \"{post.DateTime.ToString("dd MMM yyyy")} 12:00:00 BST\"";
    Console.WriteLine(commitUpdate);

    string fileName = $"{outputFolder}\\files.txt";
    using (StreamWriter sw = File.AppendText(fileName))
    {
        sw.Write(post.DateTime.ToString("yyyy-MM-dd"));
        sw.Write(" ");
        sw.Write(post.Name);
        sw.WriteLine();

        sw.WriteLine(commitUpdate);
    }
}

Console.WriteLine("Finished.");
Console.WriteLine("Press <Enter> to exit.");
Console.ReadLine();