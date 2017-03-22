﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Planner;
using System.Windows.Shapes;

namespace TaskPlanner
{
    /// <summary>
    /// Interaction logic for TaskTreeNodeControl.xaml
    /// </summary>
    public partial class TaskTreeNode : UserControl
    {
        public Task task;
        public List<TaskTreeNode> children;
        public bool isExpanded;
        public int depth;
        public string ID;

        private SolidColorBrush defaultExpanderBrush;

        public TaskTreeNode(Task task, int depth)
        {
            InitializeComponent();
            this.task = task;
            this.depth = depth;
            this.ID = task.ID;

            isExpanded = true;
            defaultExpanderBrush = (SolidColorBrush)expanderArrow.Foreground;

            children = new List<TaskTreeNode>();
            foreach (Task t in task.subtasks)
            { children.Add(new TaskTreeNode(t,depth+1)); }
            Refresh();
        }
        public void Refresh()
        {
            textBox.Text = task.title;
            checkBox.IsChecked = task.isCompleted;
            offsetGrid.Margin = new Thickness(depth * 30, 0, 0, 0);
            ExpanderRefresh();
        }
        public void Rename()
        {
            //No idea why just a simple .Focus() doesn't work.
            textBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                textBox.Focus();
            }));
        }

        public void ExpanderRefresh()
        {
            if (children.Count == 0)
            { expanderArrow.Foreground = null; }
            else
            { expanderArrow.Foreground = defaultExpanderBrush; }
            if (isExpanded)
            { expanderArrow.RenderTransform = new RotateTransform(45); }
            else
            { expanderArrow.RenderTransform = new RotateTransform(0); }
        }

        public void Select()
        {
            background.Fill = new SolidColorBrush(Colors.LightGray);
        }
        public void Deselect()
        {
            background.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            OnSelectionChanged();
        }
        private void grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSelectionChanged();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox.Focus();
            textBox.SelectAll();
            e.Handled = true;
        }
        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            OnTextUpdated();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Unfocusing was tricker than it looked
                var scope = FocusManager.GetFocusScope(textBox); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus
            }
        }

        private void expanderWrapper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExpanderRefresh();
            //Let the tasktree update its expand setting so it can create the necessary child nodes
            OnExpandUpdated(!isExpanded);
        }


        #region SelectionChanged
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskTreeNode));
        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        private void OnSelectionChanged()
        {
            RoutedEventArgs e = new RoutedEventArgs(TaskTreeNode.SelectionChangedEvent);
            RaiseEvent(e);
        }
        #endregion
        #region TextUpdated
        public delegate void TextUpdatedEventHandler(object sender, TextUpdatedEventArgs e);
        public static readonly RoutedEvent TextUpdatedEvent = EventManager.RegisterRoutedEvent(
            "TextUpdated", RoutingStrategy.Bubble, typeof(TextUpdatedEventHandler), typeof(TaskTreeNode));
        public event TextUpdatedEventHandler TextUpdated
        {
            add { AddHandler(TextUpdatedEvent, value); }
            remove { RemoveHandler(TextUpdatedEvent, value); }
        }

        private void OnTextUpdated()
        {
            //Only pass the event if it was changed
            if (task.title != textBox.Text)
            {
                TextUpdatedEventArgs e = new TextUpdatedEventArgs(TaskTreeNode.TextUpdatedEvent);

                e.task = task;
                e.newName = textBox.Text;
                RaiseEvent(e);
            }
        }
        #endregion
        #region CheckUpdated
        public delegate void CheckUpdatedEventHandler(object sender, CheckUpdatedEventArgs e);
        public static readonly RoutedEvent CheckUpdatedEvent = EventManager.RegisterRoutedEvent(
            "CheckUpdated", RoutingStrategy.Bubble, typeof(CheckUpdatedEventHandler), typeof(TaskTreeNode));
        public event CheckUpdatedEventHandler CheckUpdated
        {
            add { AddHandler(CheckUpdatedEvent, value); }
            remove { RemoveHandler(CheckUpdatedEvent, value); }
        }

        private void OnCheckUpdated()
        {
            //Only pass the event if it was changed
            if (task.isCompleted != checkBox.IsChecked)
            {
                CheckUpdatedEventArgs e = new CheckUpdatedEventArgs(TaskTreeNode.CheckUpdatedEvent);
                e.task = task;
                e.check = (bool)checkBox.IsChecked;
                RaiseEvent(e);
            }
        }
        #endregion
        #region ExpandUpdated
        public delegate void ExpandUpdatedEventHandler(object sender, ExpandUpdatedEventArgs e);
        public static readonly RoutedEvent ExpandUpdatedEvent = EventManager.RegisterRoutedEvent(
            "ExpandUpdated", RoutingStrategy.Bubble, typeof(ExpandUpdatedEventHandler), typeof(TaskTreeNode));
        public event ExpandUpdatedEventHandler ExpandUpdated
        {
            add { AddHandler(ExpandUpdatedEvent, value); }
            remove { RemoveHandler(ExpandUpdatedEvent, value); }
        }

        private void OnExpandUpdated(bool expanded)
        {
            //Only pass the event if it was changed
            if (isExpanded != expanded)
            {
                ExpandUpdatedEventArgs e = new ExpandUpdatedEventArgs(TaskTreeNode.ExpandUpdatedEvent);
                e.task = task;
                e.expanded = expanded;
                RaiseEvent(e);
            }
        }
        #endregion

    }
    public class TextUpdatedEventArgs : RoutedEventArgs
    {
        public Task task;
        public string newName;
        public TextUpdatedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
    public class CheckUpdatedEventArgs : RoutedEventArgs
    {
        public Task task;
        public bool check;
        public CheckUpdatedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
    public class ExpandUpdatedEventArgs : RoutedEventArgs
    {
        public Task task;
        public bool expanded;
        public ExpandUpdatedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
}
