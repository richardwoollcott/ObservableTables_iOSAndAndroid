              using Android.App;
using Android.Widget;
using Android.OS;
using ObservableTables.ViewModel;
using Android.Views;
using GalaSoft.MvvmLight.Helpers;

namespace ObservableTables.Droid
{
	[Activity (Label = "Tasks", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		private ListView taskList;

		public ListView TaskList
		{
			get
			{
				return taskList
					?? (taskList = FindViewById<ListView>(Resource.Id.tasksListView));
			}
		}

		public TaskListViewModel Vm
		{
			get
			{
				return App.Locator.TaskList;
			}
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			var toolbar = FindViewById<Toolbar> (Resource.Id.tasksToolbar);
			//Toolbar will now take on default Action Bar characteristics
			SetActionBar (toolbar);
			//You can now use and reference the ActionBar
			ActionBar.Title = "Hello from Toolbar";

			TaskList.Adapter = Vm.TodoTasks.GetAdapter(GetTaskAdapter);
		}

		private View GetTaskAdapter(int position, TaskModel taskModel, View convertView)
		{
			// Not reusing views here
			convertView = LayoutInflater.Inflate(Resource.Layout.TaskTemplate, null);

			var title = convertView.FindViewById<TextView>(Resource.Id.NameTextView);
			title.Text = taskModel.Name;

			var desc = convertView.FindViewById<TextView>(Resource.Id.NotesTextView);
			desc.Text = taskModel.Notes;

			return convertView;
		}
	}
}


