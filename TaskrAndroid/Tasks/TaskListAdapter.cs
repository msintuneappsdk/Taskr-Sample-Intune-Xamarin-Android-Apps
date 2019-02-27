// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace TaskrAndroid
{
    /// <summary>
    /// An IListAdapter that updates a ListView to show Tasks.
    /// </summary>
    class TaskListAdapter : BaseAdapter, IListAdapter
    {

        private List<Task> list;
        Context context;

        public TaskListAdapter(Context context)
        {
            this.context = context;
            UpdateList();
        }

        /// <summary>
        /// Sets the list for the adapter to the task data, then updates the view to show it.
        /// </summary>
        private void UpdateList()
        {
            list = TaskManager.GetAllTasks();
            NotifyDataSetChanged();
        }

        // Fill in Count here, currently 0
        public override int Count
        {
            get
            {
                return list == null ? 0 : list.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return list?[position];
        }

        public override long GetItemId(int position)
        {
            return list == null ? -1 : list[position].ID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            // convertView may be null the first time this method is called
            if (view == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                if (inflater == null)
                {
                    return null;
                }
                view = inflater.Inflate(Resource.Layout.task_list_item, parent, false);
            }

            // if the list has not been set, return the view
            if (list == null)
            {
                return view;
            }

            // Get the fields to fill in
            TextView liDescription = view.FindViewById<TextView>(Resource.Id.task_list_item_description);

            // Fill them in
            Task task = list[position];
            liDescription.Text = task.Description;

            // Set the check button listener. Completing the task will remove it from the list.
            ImageButton completeButton = view.FindViewById<ImageButton>(Resource.Id.task_list_complete_button);
            completeButton.Click += (sender, e) => {
                TaskManager.CompleteTask(task.ID);
                UpdateList();
            };

            return view;
        }

    }
}