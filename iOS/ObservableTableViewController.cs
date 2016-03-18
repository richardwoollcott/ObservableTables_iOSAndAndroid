using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Foundation;
using UIKit;

namespace ObservableTables.iOS
{
	/// <summary>
	/// A <see cref="UITableViewController"/> that can be used as an iOS view. After setting
	/// the <see cref="DataSource"/> and the <see cref="BindCellDelegate"/> and <see cref="CreateCellDelegate"/> 
	/// properties, the controller can be loaded. If the DataSource is an <see cref="INotifyCollectionChanged"/>,
	/// changes to the collection will be observed and the UI will automatically be updated.
	/// </summary>
	/// <remarks>Credits go to Frank A Krueger for the initial idea and the inspiration
	/// for this class. Frank gracefully accepted to let me add his code (with a few changes)
	/// to MVVM Light.
	/// <para>https://gist.github.com/praeclarum/10024108</para>
	/// </remarks>
	/// <typeparam name="T">The type of the items contained in the <see cref="DataSource"/>.</typeparam>
	////[ClassInfo(typeof(ObservableTableViewController<T>),
	////    VersionString = "5.1.1",
	////    DateString = "201502072030",
	////    UrlContacts = "http://www.galasoft.ch/contact_en.html",
	////    Email = "laurent@galasoft.ch")]
	public class ObservableTableViewController<T> : UITableViewController, INotifyPropertyChanged
	{
		/// <summary>
		/// The <see cref="SelectedItem" /> property's name.
		/// </summary>
		public const string SelectedItemPropertyName = "SelectedItem";

		private IList<T> _dataSource;
		private bool _loadedView;
		private Thread _mainThread;
		private INotifyCollectionChanged _notifier;
		/*private*/ protected ObservableTableSource<T> _tableSource;

		/// <summary>
		/// When set, specifies which animation should be used when rows change.
		/// </summary>
		public UITableViewRowAnimation AddAnimation
		{
			get;
			set;
		}

		/// <summary>
		/// A delegate to a method taking a <see cref="UITableViewCell"/>
		/// and setting its elements' properties according to the item
		/// passed as second parameter.
		/// The cell must be created first in the <see cref="CreateCellDelegate"/>
		/// delegate.
		/// </summary>
		public Action<UITableViewCell, T, NSIndexPath> BindCellDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// A delegate to a method creating or reusing a <see cref="UITableViewCell"/>.
		/// The cell will then be passed to the <see cref="BindCellDelegate"/>
		/// delegate to set the elements' properties.
		/// </summary>
		public Func<NSString, UITableViewCell> CreateCellDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// The data source of this list controller.
		/// </summary>
		public IList<T> DataSource
		{
			get
			{
				return _dataSource;
			}

			set
			{
				if (Equals(_dataSource, value))
				{
					return;
				}

				if (_notifier != null)
				{
					_notifier.CollectionChanged -= HandleCollectionChanged;
				}

				_dataSource = value;
				_notifier = value as INotifyCollectionChanged;

				if (_notifier != null)
				{
					_notifier.CollectionChanged += HandleCollectionChanged;
				}

				if (_loadedView)
				{
					TableView.ReloadData();
				}
			}
		}

		/// <summary>
		/// When set, specifieds which animation should be used when a row is deleted.
		/// </summary>
		public UITableViewRowAnimation DeleteAnimation
		{
			get;
			set;
		}

		/// <summary>
		/// When set, returns the height of the view that will be used for the TableView's footer.
		/// </summary>
		/// <seealso cref="GetViewForFooterDelegate"/>
		public Func<UITableView, nint, nfloat> GetHeightForFooterDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// When set, returns the height of the view that will be used for the TableView's header.
		/// </summary>
		/// <seealso cref="GetViewForHeaderDelegate"/>
		public Func<UITableView, nint, nfloat> GetHeightForHeaderDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// When set, returns a view that can be used as the TableView's footer.
		/// </summary>
		/// <seealso cref="GetHeightForFooterDelegate"/>
		public Func<UITableView, nint, UIView> GetViewForFooterDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// When set, returns a view that can be used as the TableView's header.
		/// </summary>
		/// <seealso cref="GetHeightForHeaderDelegate"/>
		public Func<UITableView, nint, UIView> GetViewForHeaderDelegate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the TableView's selected item.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public T SelectedItem
		{
			get;
			private set;
		}

		/// <summary>
		/// The source of the TableView.
		/// </summary>
		public UITableViewSource TableSource
		{
			get
			{
				return _tableSource;
			}
		}

		/// <summary>
		/// Overrides <see cref="UITableViewController.TableView"/>.
		/// Sets or gets the controllers TableView. If you use a TableView
		/// placed in the UI manually, use this property's setter to assign
		/// your TableView to this controller.
		/// </summary>
		public override UITableView TableView
		{
			get
			{
				return base.TableView;
			}
			set
			{
				base.TableView = value;
				base.TableView.Source = _tableSource ?? CreateSource();
				_loadedView = true;
			}
		}

		/// <summary>
		/// Initializes a new instance of this class with a plain style.
		/// </summary>
		public ObservableTableViewController()
			: base(UITableViewStyle.Plain)
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of this class with a specific style.
		/// </summary>
		/// <param name="tableStyle">The style that will be used for this controller.</param>
		public ObservableTableViewController(UITableViewStyle tableStyle)
			: base(tableStyle)
		{
			Initialize();
		}

		/// <summary>
		/// Overrides the <see cref="UIViewController.ViewDidLoad"/> method.
		/// </summary>
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			TableView.Source = CreateSource();
			_loadedView = true;
		}

		/// <summary>
		/// Binds a <see cref="UITableViewCell"/> to an item's properties.
		/// If a <see cref="BindCellDelegate"/> is available, this delegate will be used.
		/// If not, a simple text will be shown.
		/// </summary>
		/// <param name="cell">The cell that will be prepared.</param>
		/// <param name="item">The item that should be used to set the cell up.</param>
		/// <param name="indexPath">The <see cref="NSIndexPath"/> for this cell.</param>
		/*protected*/ public virtual void BindCell(UITableViewCell cell, object item, NSIndexPath indexPath)
		{
			if (BindCellDelegate == null)
			{
				cell.TextLabel.Text = item.ToString();
			}
			else
			{
				BindCellDelegate(cell, (T)item, indexPath);
			}
		}

		/// <summary>
		/// Creates a <see cref="UITableViewCell"/> corresponding to the reuseId.
		/// If it is set, the <see cref="CreateCellDelegate"/> delegate will be used.
		/// </summary>
		/// <param name="reuseId">A reuse identifier for the cell.</param>
		/// <returns>The created cell.</returns>
		/*protected*/ public virtual UITableViewCell CreateCell(NSString reuseId)
		{
			if (CreateCellDelegate == null
				|| BindCellDelegate == null)
			{
				return new UITableViewCell(UITableViewCellStyle.Default, reuseId);
			}

			return CreateCellDelegate(reuseId);
		}

		/// <summary>
		/// Created the ObservableTableSource for this controller.
		/// </summary>
		/// <returns>The created ObservableTableSource.</returns>
		protected virtual ObservableTableSource<T> CreateSource()
		{
			_tableSource = new ObservableTableSource<T>(this);
			return _tableSource;
		}

		/// <summary>
		/// Called when a row gets selected. Raises the SelectionChanged event.
		/// </summary>
		/// <param name="item">The selected item.</param>
		/// <param name="indexPath">The NSIndexPath for the selected row.</param>
		/*protected*/ public virtual void OnRowSelected(object item, NSIndexPath indexPath)
		{
			SelectedItem = (T)item;

			// ReSharper disable ExplicitCallerInfoArgument
			RaisePropertyChanged(SelectedItemPropertyName);
			// ReSharper restore ExplicitCallerInfoArgument

			var handler = SelectionChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_loadedView)
			{
				return;
			}

			Action act = () =>
			{
				if (e.Action == NotifyCollectionChangedAction.Reset)
				{
					TableView.ReloadData();
				}

				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					var count = e.NewItems.Count;
					var paths = new NSIndexPath[count];
					for (var i = 0; i < count; i++)
					{
						paths[i] = NSIndexPath.FromRowSection(e.NewStartingIndex + i, 0);
					}
					TableView.InsertRows(paths, AddAnimation);
				}
				else if (e.Action == NotifyCollectionChangedAction.Remove)
				{
					var count = e.OldItems.Count;
					var paths = new NSIndexPath[count];
					for (var i = 0; i < count; i++)
					{
						paths[i] = NSIndexPath.FromRowSection(e.OldStartingIndex + i, 0);
					}
					TableView.DeleteRows(paths, DeleteAnimation);
				}
			};

			var isMainThread = Thread.CurrentThread == _mainThread;

			if (isMainThread)
			{
				act();
			}
			else
			{
				NSOperationQueue.MainQueue.AddOperation(act);
				NSOperationQueue.MainQueue.WaitUntilAllOperationsAreFinished();
			}
		}

		private void Initialize()
		{
			_mainThread = Thread.CurrentThread;

			AddAnimation = UITableViewRowAnimation.Automatic;
			DeleteAnimation = UITableViewRowAnimation.Automatic;
		}

		/// <summary>
		/// Occurs when a property of this instance changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when a new item gets selected in the list.
		/// </summary>
		public event EventHandler SelectionChanged;


	}
}

