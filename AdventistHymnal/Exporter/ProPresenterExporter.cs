using AdventistHymnal.Components;

namespace AdventistHymnal.Exporter
{
	public class ProPresenterExporter : HymnExporter
	{
        protected override int MaxLinesPerSlide { get; set; }
        protected override string OutputFolderName => "ProPresenter";
		protected override string TitleTag => "Title:";
		protected override string AuthorTag => "Author:";
		protected override string CCLITag => "CCLI:";
		protected override string[] InvalidCharacters => new[] { ":" };

        public ProPresenterExporter(int maxLinesPerSlide, HymnDatabase database, string collectionId)
            : base(database, collectionId)
		{
            MaxLinesPerSlide = maxLinesPerSlide;
        }
    }
}