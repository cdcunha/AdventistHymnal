using System;
using System.IO;
using System.Text;
using AdventistHymnal.Components;

namespace AdventistHymnal.Exporter
{
	public abstract class HymnExporter
	{
		protected HymnDatabase Database { get; }
		protected string CollectionId { get; }

		private readonly OverwritePrompt _overwritePrompt = new();

		protected HymnExporter(HymnDatabase database, string collectionId)
		{
			Database = database;
			CollectionId = collectionId;
		}

		// TODO: Move these to a config.ini file
		protected abstract int MaxLinesPerSlide { get; set; }
		protected const int CCLINumber = 999999;
		
		protected abstract string OutputFolderName { get; }
		protected abstract string TitleTag { get; }
		protected abstract string AuthorTag { get; }
		protected abstract string CCLITag { get; }
		protected abstract string[] InvalidCharacters { get; }

		public void ExportAll()
		{
			ExportRange(Database.MinNumber, Database.MaxNumber);
		}

		public void ExportRange(int min, int max)
		{
			if (max <= 0)
			{
				Logger.LogMessage("No hymn was found in the database");
				return;
			}
			Logger.LogMessage($"Beginning export for hymns #{min} - #{max}");
			for (int i = min; i <= max; i++)
			{
				if (Database.GetHymn(i) != null)
				{
					ExportHymn(i);
					if (_overwritePrompt.Cancelled)
					{
						Logger.LogMessage("Export cancelled.");
						return;
					}
				}
			}
		}

		public void ExportHymn(int number)
		{
			Hymn hymn = Database.GetHymn(number);

            StringBuilder builder = new();
			builder.AppendLine($"{TitleTag}#{hymn.Number} {hymn.Title}");
			builder.AppendLine($"{AuthorTag}{hymn.Author}");
			
			if (hymn.CcliNumber.HasValue)
			{
				builder.AppendLine($"{CCLITag}{hymn.CcliNumber.Value}");
			}
			
			builder.AppendLine();

			builder.AppendLine("Tag");
			builder.AppendLine($"{CollectionId} #{hymn.Number}");
			builder.AppendLine($"{hymn.Title}");
			builder.AppendLine();

			foreach (string s in hymn.StanzaOrder)
			{
				AddStanza(hymn.GetStanza(StanzaReference.FromString(s)), ref builder);
				builder.AppendLine();
			}
			
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Program.OutputDirectory, OutputFolderName, CollectionId);
            
			if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

			string filename = Path.Combine(path, $"{hymn.Number:000}.txt");

			if (!_overwritePrompt.ShouldWrite(filename))
			{
				if (!_overwritePrompt.Cancelled)
				{
					Logger.LogMessage($"Hymn #{hymn.Number} skipped.");
				}
				return;
			}

            File.WriteAllText(filename, builder.ToString());
            Logger.LogMessage($"Hymn #{hymn.Number} successful!");
        }

		private void AddStanza(Stanza stanza, ref StringBuilder builder)
		{
			builder.AppendLine(stanza.Reference.ToString());
			int x = 0;

			foreach (string line in stanza.Lines)
			{
				if (x == MaxLinesPerSlide)
				{
					builder.AppendLine();
					x = 0;
				}

				string output = line;

				foreach (string c in InvalidCharacters)
				{
					output = output.Replace(c, string.Empty);
				}
				
				builder.AppendLine(output);
				x++;
			}
		}
	}
}