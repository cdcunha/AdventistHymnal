using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			if (commandId != 1)
			{
				MainMenu();
				return;
			}

			string collectionPath = SelectCollection(DatabaseDirectory);
			if (collectionPath == null)
			{
				MainMenu();
				return;
			}

            HymnDatabase database = new();
			database.LoadData(collectionPath);
			new ProPresenterExporter(maxLinesPerSlide, database, collectionPath.Replace(DatabaseDirectory, "")).ExportAll();

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

            string collectionPath = SelectCollection(InputDirectory);
            if (collectionPath == null)
            {
                MainMenu();
                return;
            }

			string databasePath = Path.Combine(DatabaseDirectory, collectionPath.Replace(InputDirectory, ""));
			TextImporter importer = new();
			importer.ImportTextFiles(collectionPath, databasePath);

			Logger.LogMessage("Press any key to return to the main menu");
			Console.ReadKey();

			MainMenu();
		}

        private static string SelectCollection(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                return null;
            }

            string[] subFolders = Directory.GetDirectories(rootPath);
            if (subFolders.Length == 0)
            {
                Logger.LogMessage($"No collection subfolders found in the {rootPath} folder");
                MainMenu();
                return null;
            }

            Console.WriteLine("Choose a collection:");
            for (int i = 0; i < subFolders.Length; i++)
            {
                Console.WriteLine($"{i + 1}) {Path.GetFileName(subFolders[i])}");
            }
            Console.WriteLine();
            Console.Write("> ");

            if (!int.TryParse(Console.ReadLine(), out int commandId))
            {
                MainMenu();
                return null;
            }

            return subFolders[commandId - 1];
        }

        private sealed class HymnalFolder
		{
			public string Id { get; }
			public string Path { get; }

			public HymnalFolder(string id, string path)
			{
				Id = id;
				Path = path;
			}
		}
	}
}