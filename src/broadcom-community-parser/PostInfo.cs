using broadcom_community_parser.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace broadcom_community_parser
{
    public class PostInfo
    {
        public PostInfo() { }

        public gdprAuditReportInfo GetPostInfo()
        {
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

            return output;
        }

        public IEnumerable<gdprAuditReportInfoContactDataEntity?> GetPostDocuments(gdprAuditReportInfo output)
        {
            var data = (gdprAuditReportInfoContactData)output.Items[1];
            var documents = data.entity.Where(d => d.name == "Document");
            
            return documents;
        }

        public List<Post> ConvertToPosts(IEnumerable<gdprAuditReportInfoContactDataEntity?> documents)
        {
            List<Post> posts = new();

            foreach (var document in documents)
            {
                Post post = new();
                post.DateTime = DateTime.Parse(document.dateTime);
                //Console.WriteLine(document.dateTime);
                foreach (var column in document.column.Where(c => c.name != "Document key"))
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

            return posts;
        }

    }
}
