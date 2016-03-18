using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace ObservableTables.iOS
{
	public static class ExtensionsMvvmLight
	{
		public static TaskListObservableTableViewController<T> GetTaskListController<T>(
			this IList<T> list,
			Func<NSString, UITableViewCell> createCellDelegate,
			Action<UITableViewCell, T, NSIndexPath> bindCellDelegate)
		{
			return new TaskListObservableTableViewController<T>
			{
				DataSource = list,
				CreateCellDelegate = createCellDelegate,
				BindCellDelegate = bindCellDelegate
			};
		}
	}
}

