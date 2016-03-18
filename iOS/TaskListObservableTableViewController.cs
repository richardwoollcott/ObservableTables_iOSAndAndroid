using UIKit;

namespace ObservableTables.iOS
{
	public class TaskListObservableTableViewController<T> : ObservableTableViewController<T>
	{
		public TaskListObservableTableViewController()
			: base()
		{
		}

		public TaskListObservableTableViewController(UITableViewStyle tableStyle)
			: base(tableStyle)
		{
		}

		protected override ObservableTableSource<T> CreateSource()
		{
			_tableSource = new TaskListObservableTableSource<T>(this);
			return _tableSource;
		}
	}
}

