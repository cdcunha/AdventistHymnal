# AdventistHymnal
This is a management and export tool for the Seventh-day Adventist hymnal into various presentation software, such as ProPresenter, Proclaim, or MediaShout.

The hymn files and idea came from <a>https://github.com/ariseforgod/adventist_hymnal</a>. That repository is no longer maintained, so I wrote this new program to manage the hymns and assist in the importing process into ProPresenter. Other software can be supported, but contributions will be required to get them working. The framework is here for it, though. It just needs a bit of work.

Additionally, this tool can allow you to easily modify the layout of the files in bulk. This makes it easy to adjust spacing and re-import in just a few minutes.

## How to use
Generated ProPresenter exports are written to `Outputs/ProPresenter/<collection-id>` and can be mass imported as `.txt` files. If you would like to further customize the aesthetic, manual tweaking of the generation system will be required.

If you build the project, you must copy the Inputs/Database folders into the root directory with the executable.

- `Inputs/<collection-id>`: Raw `.txt` hymn files for a hymnal collection, such as `Inputs/SDA-1941-en`.
- `Database/<collection-id>`: Imported hymn files parsed and stored as JSON, such as `Database/SDA-1941-en`.
- `Outputs/<format>/<collection-id>`: Exported files ready to be imported into presentation software, such as `Outputs/ProPresenter/SDA-1941-en`.

Choose the collection when importing or exporting. The collection folder name is used as its ID and as the export subfolder name. Importing a collection writes its JSON files to the matching `Database/<collection-id>` folder.

Every collection must be in its own subfolder; files placed directly in the root `Inputs` or `Database` folder are ignored.

## To do
- Improve abstraction to allow other exporters that may not have a similar syntax.
- Review copyright information
- Add author credits
- Review text for typos or other issues
- Phase out 'Inputs' folder in favor of the json database

## Closing notes
If you have questions or would like to contribute, please contact me, my information is below.

Discord: kalradia

*I sincerely hope this helps you and your church up your presentation game and makes your life a bit easier.*
