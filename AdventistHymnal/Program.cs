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
		private const string LegacyCollectionId = "SDA-1985-en";
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

			List<HymnalFolder> collections = GetCollections(DatabaseDirectory, "*.json");
			HymnalFolder collection = SelectCollection(collections, "database");
			if (collection == null)
			{
				MainMenu();
				return;
			}

			HymnDatabase database = new();
			database.LoadData(collection.Path);
			new ProPresenterExporter(maxLinesPerSlide, database, collection.Id).ExportAll();
			
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

			List<HymnalFolder> collections = GetCollections(InputDirectory, "*.txt");
			HymnalFolder collection = SelectCollection(collections, "input");
			if (collection == null)
			{
				MainMenu();
				return;
			}

			string databasePath = collection.IsRoot
				? DatabaseDirectory
				: Path.Combine(DatabaseDirectory, collection.Id);
			TextImporter importer = new();
			importer.ImportTextFiles(collection.Path, databasePath);
			
			Logger.LogMessage("Press any key to return to the main menu");
			Console.ReadKey();
			
			MainMenu();
		}

		private static List<HymnalFolder> GetCollections(string rootPath, string filePattern)
		{
			if (!Directory.Exists(rootPath))
			{
				return new List<HymnalFolder>();
			}

			List<HymnalFolder> collections = Directory.GetDirectories(rootPath)
				.Where(path => Directory.GetFiles(path, filePattern).Length > 0)
				.Select(path => new HymnalFolder(Path.GetFileName(path), path, false))
				.OrderBy(collection => collection.Id, StringComparer.OrdinalIgnoreCase)
				.ToList();

			bool hasLegacyFolder = collections.Exists(collection =>
				string.Equals(collection.Id, LegacyCollectionId, StringComparison.OrdinalIgnoreCase));
			if (!hasLegacyFolder && Directory.GetFiles(rootPath, filePattern).Length > 0)
			{
				collections.Insert(0, new HymnalFolder(LegacyCollectionId, rootPath, true));
			}

			return collections;
		}

		private static HymnalFolder SelectCollection(List<HymnalFolder> collections, string sourceName)
		{
			if (collections.Count == 0)
			{
				Logger.LogMessage($"No hymn collections found in the {sourceName} folder");
				return null;
			}

			Console.WriteLine("Select a hymnal collection:");
			for (int i = 0; i < collections.Count; i++)
			{
				string label = collections[i].IsRoot
					? $"{collections[i].Id} (root files)"
					: collections[i].Id;
				Console.WriteLine($"{i + 1}) {label}");
			}
			Console.WriteLine();
			Console.Write("> ");

			if (!int.TryParse(Console.ReadLine(), out int selection) ||
				selection < 1 ||
				selection > collections.Count)
			{
				Logger.LogMessage("Invalid collection selection");
				return null;
			}

			return collections[selection - 1];
		}

		private sealed class HymnalFolder
		{
			public string Id { get; }
			public string Path { get; }
			public bool IsRoot { get; }

			public HymnalFolder(string id, string path, bool isRoot)
			{
				Id = id;
				Path = path;
				IsRoot = isRoot;
			}
		}
	}
}