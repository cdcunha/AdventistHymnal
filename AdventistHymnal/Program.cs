using System;
using System.IO;
using AdventistHymnal.Components;
using AdventistHymnal.Exporter;
using AdventistHymnal.Importer;

namespace AdventistHymnal
{
	class Program
	{
		public const string DatabaseDirectory = @".\Database\";
		public const string OutputDirectory = @".\Outputs\";
		public const string InputDirectory = @".\Inputs\";

		public static HymnDatabase DB
		{
			get
			{
				if (_db == null)
				{
					_db = new HymnDatabase();
                    _db.LoadData(DatabaseDirectory);
                }

                return _db;
			}
		}
		private static HymnDatabase _db;
		private static int maxLinesPerSlide = 2;


        static void Main(string[] args)
		{
			MainMenu();
		}

		public static void MainMenu()
		{
            Console.Clear();
			Console.WriteLine("Input the number to execute the command."); 
			Console.WriteLine($"1) Setup Max Lines Per Slide (Set to: {maxLinesPerSlide} lines per slide)");
            Console.WriteLine("2) Export Hymns");
            Console.WriteLine("3) Import Hymns");
			Console.WriteLine("4) Quit");
			Console.WriteLine();
			Console.Write("> ");

			if (!int.TryParse(Console.ReadLine(), out int commandId))
			{
				MainMenu();
				return;
			}

			

            if (commandId == 1)
            {
                maxLinesPerSlide = SetupLinesPerSlide();
                MainMenu();
            }
            else if (commandId == 2)
			{
				ExportHymns(maxLinesPerSlide);
			}
			else if (commandId == 3)
			{
				ImportHymns();
			}
			else if (commandId == 4)
			{
				return;
			}
			else
			{
				MainMenu();
			}
		}

		private static int SetupLinesPerSlide()
		{
            Console.WriteLine("How many lines per slide? ");
            Console.WriteLine("1) One line per slide");
            Console.WriteLine("2) Two lines per slide");
            Console.WriteLine("3) Three lines per slide");
            Console.WriteLine("4) Four lines per slide");
            Console.WriteLine();
            Console.Write("> ");

            if (!int.TryParse(Console.ReadLine(), out int commandId))
            {
                return 2;
            }

			switch (commandId)
			{
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    return 4;
                default:
                    return 2;
            }
        }

		private static void ExportHymns(int maxLinesPerSlide)
		{
			Console.WriteLine("What format would you like to export?");
			Console.WriteLine("1) ProPresenter");
			Console.WriteLine();
			Console.Write("> ");

			if (!int.TryParse(Console.ReadLine(), out int commandId))
			{
				MainMenu();
				return;
			}

			if (commandId == 1)
			{
				new ProPresenterExporter(maxLinesPerSlide).ExportAll();
			}
			else
			{
				MainMenu();
				return;
			}
			
			Logger.LogMessage("Press any key to return to the main menu");
			Console.ReadKey();
			
			MainMenu();
		}

		private static void ImportHymns()
		{
			if (!Directory.Exists(InputDirectory))
			{
				Directory.CreateDirectory(InputDirectory);
			}

			TextImporter importer = new();
			importer.ImportTextFiles(InputDirectory);

			// Clear the old database from memory so it can be refreshed
			_db = null;
			
			Logger.LogMessage("Press any key to return to the main menu");
			Console.ReadKey();
			
			MainMenu();
		}
	}
}