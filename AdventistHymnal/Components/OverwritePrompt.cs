using System;
using System.IO;

namespace AdventistHymnal.Components
{
	/// <summary>
	/// Asks the user whether existing files should be replaced, remembering "all"/"none" answers.
	/// </summary>
	public class OverwritePrompt
	{
		private enum Mode { Ask, ReplaceAll, ReplaceNone }

		private Mode _mode = Mode.Ask;

		public bool Cancelled { get; private set; }

		public bool ShouldWrite(string filename)
		{
			if (Cancelled)
			{
				return false;
			}

			if (!File.Exists(filename))
			{
				return true;
			}

			switch (_mode)
			{
				case Mode.ReplaceAll:
					return true;
				case Mode.ReplaceNone:
					return false;
			}

			while (true)
			{
				Console.WriteLine($"The file \"{Path.GetFileName(filename)}\" already exists. Do you want to replace it?");
				Console.WriteLine("1) Yes");
				Console.WriteLine("2) No");
				Console.WriteLine("3) Replace All");
				Console.WriteLine("4) Don't replace any. Only new files will be created");
				Console.WriteLine("5) Cancel");
				Console.WriteLine();
				Console.Write("> ");

				if (!int.TryParse(Console.ReadLine(), out int option))
				{
					Console.WriteLine("Invalid option.");
					continue;
				}

				switch (option)
				{
					case 1:
						return true;
					case 2:
						return false;
					case 3:
						_mode = Mode.ReplaceAll;
						return true;
					case 4:
						_mode = Mode.ReplaceNone;
						return false;
					case 5:
						Cancelled = true;
						Console.WriteLine("Process cancelled.");
						return false;
					default:
						Console.WriteLine("Invalid option.");
						break;
				}
			}
		}
	}
}
