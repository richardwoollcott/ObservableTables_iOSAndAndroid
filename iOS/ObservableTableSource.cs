using System;
using UIKit;
using Foundation;
using System.Diagnostics;

namespace ObservableTables.iOS
{
	/// <summary>
	/// A <see cref="UITableViewSource"/> that handles changes to the underlying
	/// data source if this data source is an <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T2">The type of the items that the data source contains.</typeparam>
	/// <remarks>In the current implementation, only one section is supported.</remarks>
	/*protected*/ public class ObservableTableSource<T2> : UITableViewSource
	{
		/*private*/ protected readonly ObservableTableViewController<T2> _controller;
		private readonly NSString _reuseId = new NSString("C");

		/// <summary>
		/// Initializes an instance of this class.
		/// </summary>
		/// <param name="controller">The controller associated to this instance.</param>
		public ObservableTableSource(ObservableTableViewController<T2> controller)
		{
			_controller = controller;
		}

		/// <summary>
		/// Attempts to dequeue or create a cell for the list.
		/// </summary>
		/// <param name="tableView">The TableView that is the cell's parent.</param>
		/// <param name="indexPath">The NSIndexPath for the cell.</param>
		/// <returns>The created or recycled cell.</returns>
		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell(_reuseId) ??
				_controller.CreateCell(_reuseId);

			try
			{
				//var coll = _controller._dataSource;
				var coll = _controller.DataSource;
				if (coll != null)
				{
					var obj = coll[indexPath.Row];
					_controller.BindCell(cell, obj, indexPath);
				}

				return cell;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			return cell;
		}

		/// <summary>
		/// When called, checks if the ObservableTableViewController{T}.GetHeightForFooter
		/// delegate has been set. If yes, calls that delegate to get the TableView's footer height.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="section">The section index.</param>
		/// <returns>The footer's height.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override nfloat GetHeightForFooter(UITableView tableView, nint section)
		{
			if (_controller.GetHeightForFooterDelegate != null)
			{
				return _controller.GetHeightForFooterDelegate(tableView, section);
			}

			return 0;
		}

		/// <summary>
		/// When called, checks if the ObservableTableViewController{T}.GetHeightForHeader
		/// delegate has been set. If yes, calls that delegate to get the TableView's header height.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="section">The section index.</param>
		/// <returns>The header's height.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			if (_controller.GetHeightForHeaderDelegate != null)
			{
				return _controller.GetHeightForHeaderDelegate(tableView, section);
			}

			return 0;
		}

		/// <summary>
		/// When called, checks if the ObservableTableViewController{T}.GetViewForFooter
		/// delegate has been set. If yes, calls that delegate to get the TableView's footer.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="section">The section index.</param>
		/// <returns>The UIView that should appear as the section's footer.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override UIView GetViewForFooter(UITableView tableView, nint section)
		{
			if (_controller.GetViewForFooterDelegate != null)
			{
				return _controller.GetViewForFooterDelegate(tableView, section);
			}

			return base.GetViewForFooter(tableView, section);
		}

		/// <summary>
		/// When called, checks if the ObservableTableViewController{T}.GetViewForHeader
		/// delegate has been set. If yes, calls that delegate to get the TableView's header.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="section">The section index.</param>
		/// <returns>The UIView that should appear as the section's header.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
			if (_controller.GetViewForHeaderDelegate != null)
			{
				return _controller.GetViewForHeaderDelegate(tableView, section);
			}

			return base.GetViewForHeader(tableView, section);
		}

		/// <summary>
		/// Overrides the <see cref="UITableViewSource.NumberOfSections"/> method.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <returns>The number of sections of the UITableView.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override nint NumberOfSections(UITableView tableView)
		{
			return 1;
		}

		/// <summary>
		/// Overrides the <see cref="UITableViewSource.RowSelected"/> method
		/// and notifies the associated <see cref="ObservableTableViewController{T}"/>
		/// that a row has been selected, so that the corresponding events can be raised.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="indexPath">The row's NSIndexPath.</param>
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			//var item = _controller._dataSource != null ? _controller._dataSource[indexPath.Row] : default(T2);
			var item = _controller.DataSource != null ? _controller.DataSource[indexPath.Row] : default(T2);
			_controller.OnRowSelected(item, indexPath);
		}

		/// <summary>
		/// Overrides the <see cref="UITableViewSource.RowsInSection"/> method
		/// and returns the number of rows in the associated data source.
		/// </summary>
		/// <param name="tableView">The active TableView.</param>
		/// <param name="section">The active section.</param>
		/// <returns>The number of rows in the data source.</returns>
		/// <remarks>In the current implementation, only one section is supported.</remarks>
		public override nint RowsInSection(UITableView tableView, nint section)
		{
			//var coll = _controller._dataSource;
			var coll = _controller.DataSource;
			return coll != null ? coll.Count : 0;
		}
	}
}

