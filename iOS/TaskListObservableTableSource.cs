using UIKit;
using Foundation;

namespace ObservableTables.iOS
{
	public class TaskListObservableTableSource<T> : ObservableTableSource<T>
	{
		public TaskListObservableTableSource(ObservableTableViewController<T> controller)
			: base(controller)
		{
		}
			
		public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}

		public override void CommitEditingStyle(UITableView tableView,
												UITableViewCellEditingStyle editingStyle,
												NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
				case UITableViewCellEditingStyle.Delete:
					// remove the item from the underlying data source
					_controller.DataSource.RemoveAt(indexPath.Row);
					// No need to delete the row from the table as the tableview is bound to the data source
					break;
			}
		}

		public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView,
																		NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}

	}
}

