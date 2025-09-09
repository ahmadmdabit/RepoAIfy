using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RepoAIfyApp
{
    public class FileSystemNode : INotifyPropertyChanged
    {
        private bool? _isChecked = true;
        private bool _isExpanded;

        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<FileSystemNode> Children { get; set; }
        public FileSystemNode? Parent { get; set; }

        public FileSystemNode()
        {
            Children = new ObservableCollection<FileSystemNode>();
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var child in Children)
                {
                    child.SetIsChecked(_isChecked, true, false);
                }
            }

            if (updateParent && Parent != null)
            {
                Parent.VerifyCheckState();
            }

            OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
