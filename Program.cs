// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using Newtonsoft.Json;
using Crawler;
using System.Reflection;


internal class Program
{

    static DataTable table = new DataTable();
    static DataTable images = new DataTable();

    public static string GetHTML(string URL)
    {
        string html = "";
        using (var client = new WebClient())
        {
            try
            {
                html = client.DownloadString(URL);
            }
            catch (Exception ex)
            {
                html = "ERROR: " + ex.Message;
            }
        }
        return html;
    }
    public static string DataTableToJSONWithJSONNet(DataTable table)
    {

        string JSONString = string.Empty;
        JSONString = JsonConvert.SerializeObject(table
            //,new JsonSerializerSettings { 
            //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            //    Formatting= Newtonsoft.Json.Formatting.Indented,
                
            
            //}
            );
        return JSONString;
    //    return JsonConvert.SerializeObject(table);

    //    JsonConvert.SerializeObject(table, Formatting.Indented,
    //new JsonSerializerSettings()
//    {
//        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
//    }
//);
    }

    private static void Crawl()
    {
        CreateTable();
        Console.WriteLine("Enter the URL:");
        List<string> SiteMaps = new List<string>();
        //File.ReadAllLines("c:\\Paths.txt");

        foreach (string s in File.ReadAllLines("c:\\JSON\\Paths.txt"))
        {
            SiteMaps.Add(s);
        }

        //https://www.northropgrumman.com/sitemap_index.xml
        //https://www.northropgrumman.com/suppliers/sitemap_index.xml
        //https://now.northropgrumman.com/sitemap_index.xml
        var counter = 1;
        var imageCounter = 1;
        foreach (string SiteMapURL in SiteMaps)
        {

            XmlDocument mainXMLDoc = new XmlDocument();
            mainXMLDoc.Load(SiteMapURL);
            XmlNode root = mainXMLDoc.DocumentElement;
            XmlNodeList Maps = root.ChildNodes;
            string url = "";

            foreach (XmlNode SiteMap in Maps)
            {

                url = SiteMap["loc"].InnerText;

                try
                {
                    var row = table.NewRow();
                    row["Id"] = counter;
                    row["SiteMap"] = url;



                    counter++;

                    table.Rows.Add(row);

                    XmlDocument innerXMLDoc = new XmlDocument();
                    innerXMLDoc.Load(url);
                    XmlNode pagesRoot = innerXMLDoc.DocumentElement;
                    XmlNodeList Pages = pagesRoot.ChildNodes;
                    foreach (XmlNode Page in Pages)
                    {

                        string path = Page["loc"].InnerText;
                        var pageRow = table.NewRow();
                        pageRow["Id"] = counter;
                        pageRow["SiteMap"] = url;
                        pageRow["URL"] = path;
                        pageRow["content"] = GetHTML(path);
                        counter++;
                        table.Rows.Add(pageRow);
                        Console.WriteLine(counter + "- " + url + " | " + path);



                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n URL is: " + url);
                }




            }
        }

        File.WriteAllText("c:\\JSON\\JSON.JSON", DataTableToJSONWithJSONNet(table));
        Console.WriteLine("END!");
        Console.ReadLine();
    }
    private static void CreateTable()
    {
        table.Columns.Add(new DataColumn
        {
            ColumnName = "Id",
            DataType = Type.GetType("System.Int32"),
        });

        table.Columns.Add(new DataColumn
        {
            ColumnName = "SiteMap",
            DataType = Type.GetType("System.String"),
        });
        table.Columns.Add(new DataColumn
        {
            ColumnName = "URL",
            DataType = Type.GetType("System.String"),
        });
        table.Columns.Add(new DataColumn
        {
            ColumnName = "Content",
            DataType = Type.GetType("System.String"),
        });

        images.Columns.Add(new DataColumn
        {
            ColumnName = "Id",
            DataType = Type.GetType("System.Int32"),
        });
        images.Columns.Add(new DataColumn
        {
            ColumnName = "PageId",
            DataType = Type.GetType("System.Int32"),
        });
        images.Columns.Add(new DataColumn
        {
            ColumnName = "Path",
            DataType = Type.GetType("System.String"),
        });
    }
    static void BuildURLJson()
    {
        try
        {
            table.Columns.Clear();
            images.Columns.Clear();
            CreateTable();

            List<string> JSONs = new List<string>();
            JSONs.Add("C:\\JSON\\JSON Now Sitemap_Index.JSON");
            JSONs.Add("C:\\JSON\\JSON sitemap_index.JSON");
            JSONs.Add("C:\\JSON\\JSON suppliers sitemap_index.JSON");
            int counter = 0;
            foreach (var json in JSONs)
            {

                using (StreamReader r = new StreamReader(json))
                {
                    string jsonContent = r.ReadToEnd();
                    List<Page> items = JsonConvert.DeserializeObject<List<Page>>(jsonContent);
                    Console.WriteLine("Item Loaded: " + json);

                    foreach (Page item in items)
                    {
                        if (item.URL != null && item.URL.ToUpper() != "NULL")
                        {
                            var pageRow = table.NewRow();
                            pageRow["Id"] = ++counter;
                            //pageRow["SiteMap"] = item.SiteMap;
                            pageRow["URL"] = item.URL;
                            table.Rows.Add(pageRow);
                            Console.WriteLine("Item Added to table: " + pageRow["Id"] + " - " + item.URL);
                        }
                        else
                            {
                            Console.WriteLine("SiteMap Item Skipped!");
                        }
                    }

                }

            }
            File.WriteAllText("c:\\JSON\\URLs.JSON", DataTableToJSONWithJSONNet(table));
            Console.WriteLine("END!");
            Console.ReadLine();
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: "+ex.ToString());
            Console.ReadLine();

        }

    }
    private static void Main(string[] args)
    {

        //Crawl();
        BuildURLJson();


        
    }
}