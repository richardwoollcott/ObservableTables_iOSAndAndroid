using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

namespace ObservableTables.ViewModel
{
	public class TaskListViewModel : ViewModelBase
	{
		public ObservableCollection<TaskModel> TodoTasks { get; private set; }

		public TaskListViewModel()
		{
			TodoTasks = new ObservableCollection<TaskModel>()
			{
				new TaskModel {
					Name = "Make Lunch",
					Notes = ""
				},
				new TaskModel {
					Name = "Pack Lunch",
					Notes = "In the bag, make sure we don't squash anything. Remember to pack the orange juice too."
				},
				new TaskModel {
					Name = "Goto Work",
					Notes = "Walk if it's sunny"
				},
				new TaskModel {
					Name = "Eat Lunch",
					Notes = ""
				}
			};

			AddTaskCommand = new RelayCommand(AddTask);

		}

		public RelayCommand AddTaskCommand { get; set; }

		private void AddTask()
		{
			TodoTasks.Add(new TaskModel
				{
					Name = "New Task",
					Notes = ""
				});
		}
	}
}