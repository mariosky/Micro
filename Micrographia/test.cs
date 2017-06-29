using System;
using System.IO;
using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

namespace Folders
{

    class Study
     {
        public string PatientName;
		public DateTime CreationDate;

        public Study(string pn)
        {
            CreationDate = DateTime.Now;
            PatientName = pn;
        }

    }



    class Drawer
    {

        public string path;

        public Drawer(string path)
        {
            this.path = path;

            if (! Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        
        }

        public void AddFolder(string name)
        {
            string folder_path = path + "/" + name;
            string file_path = folder_path + "/study_info.txt";


            if (!Directory.Exists(folder_path))
            {
                Directory.CreateDirectory(folder_path);
            }

             
            using (StreamWriter outputFile = new StreamWriter(file_path))
			{
					outputFile.WriteLine(name);
			}

        }

        public List<String> GetStudies()
        {
            List<String> studies = new List<string>();

			List<string> dirs = new List<string>(Directory.EnumerateDirectories(this.path));

            foreach (String folder in dirs)
            {
             
                Console.WriteLine(folder);
            

			try
			{   // Open the text file using a stream reader.
				using (StreamReader sr = new StreamReader(folder + @"/study_info.txt"))
				{
					// Read the stream to a string, and write the string to the console.
					String line = sr.ReadToEnd();
					Console.WriteLine(line);
                    studies.Add(line);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("The file could not be read:");
				Console.WriteLine(e.Message);
			}

			}

            return studies;
		}
    

    }


   


    class Folder
    {


    }




    class MainClass
    {
        public static void Main(string[] args)
        {
         /*   Console.WriteLine("Hello World!");

            Drawer d = new Drawer(@"/Users/mariosky/Desktop/Drawer");
            for (int i = 0; i < 2000; i++)
                d.AddFolder("Test" + i.ToString());


            List<String> s = d.GetStudies();

            foreach (String folder in s)
                Console.WriteLine(folder);

			FileInfo file = new FileInfo(@"/Users/mariosky/Desktop/Drawer/hello_world.pdf");
			file.Directory.Create();
			*/
			new C01E01_HelloWorld().CreatePdf(@"/Users/mariosky/Desktop/Drawer/hello_world.pdf");




		}



 
	}
		



/// <summary>Simple Hello World example.</summary>
public class C01E01_HelloWorld
{
	public const String DEST = "results/chapter01/hello_world.pdf";

	
	
	/// <exception cref="System.IO.IOException"/>
	public virtual void CreatePdf(String dest)
	{
		//Initialize PDF writer
		PdfWriter writer = new PdfWriter(dest);
		//Initialize PDF document
		PdfDocument pdf = new PdfDocument(writer);
		// Initialize document
		Document document = new Document(pdf);
		//Add paragraph to the document
		document.Add(new Paragraph("Hello World!"));

        Image yorkie = new Image( ImageDataFactory.Create(@"/Users/mariosky/Desktop/york.jpg"));
            Image blood = new Image(ImageDataFactory.Create(@"/Users/mariosky/Desktop/blood.jpg"));

            yorkie.SetWidth(200);
		Paragraph p = new Paragraph(" ").Add(yorkie).Add("     ").Add(yorkie);
			// Add Paragraph to document
			document.Add(p);

			Paragraph p2 = new Paragraph(" ").Add(yorkie).Add("     ").Add(yorkie);
			// Add Paragraph to document
			document.Add(p2);
            float[] w = { 200, 250 };

            Table table = new Table( w, false);
            Cell cell = new Cell(1, 1).Add(yorkie.SetAutoScale(true) );
			Cell cell2 = new Cell(1, 2).Add(blood.SetAutoScale(true));
            Cell cell3 = new Cell(2, 2).Add(blood.SetAutoScale(true));
            table.AddCell(cell);
            table.AddCell(cell2);
            document.Add(table);

			//Close document
			document.Close();

		//Close document
		document.Close();
	}
}
    
    
}
