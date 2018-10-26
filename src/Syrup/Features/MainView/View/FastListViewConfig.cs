using System.Windows.Forms;
using BrightIdeasSoftware;
using Syrup.Features.MainView.Controls;

namespace Syrup.Features.MainView.View
{
    public class FastListViewConfig
    {
        public static void Make(FastListView fastListView)
        {
            ConfigureColumns(fastListView);
            fastListView.ShowGroups  = false;
            fastListView.ShowItemCountOnGroups = true;
            fastListView.SortGroupItemsByPrimaryColumn = false;
            fastListView.FullRowSelect = true;

        }

        private static void ConfigureColumns(ObjectListView fastListView)
        {
            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.Width = 200;
            fastListView.AllColumns.Add(nameColumn);

            OLVColumn fileColumn = new OLVColumn("File", "File");
            fileColumn.Width = 200;
            fastListView.AllColumns.Add(fileColumn);

            var releaseDateColumn = new OLVColumn("Release date", "RelaseDate");
            releaseDateColumn.Width = 140;
            fastListView.AllColumns.Add(releaseDateColumn);
          
            var activeColumn = new OLVColumn("Active", "IsActive");
            activeColumn.CheckBoxes = true;
            activeColumn.Width = 50;
            activeColumn.IsEditable = false;
            activeColumn.TextAlign = HorizontalAlignment.Center;
            fastListView.AllColumns.Add(activeColumn);
          

            fastListView.Columns.AddRange(new ColumnHeader[]
            {
                nameColumn,
                fileColumn,
                releaseDateColumn,
                activeColumn
            });
            
        }
    }
}