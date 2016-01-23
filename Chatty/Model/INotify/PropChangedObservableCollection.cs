using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Chatty.Model.INotify
{
    /// <summary>
    /// This class allows to update the entire collection when an item in the collection has Notified a property change.
    /// Copyright and Created by: Ricardo Magalhães
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropChangeObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Add) {
                RegisterPropertyChanged(e.NewItems);
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove) {
                UnRegisterPropertyChanged(e.OldItems);
            }
            else if(e.Action == NotifyCollectionChangedAction.Replace) {
                UnRegisterPropertyChanged(e.OldItems);
                RegisterPropertyChanged(e.NewItems);
            }

            base.OnCollectionChanged(e);
        }

        protected override void ClearItems() {
            UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        private void RegisterPropertyChanged(IList items) {
            foreach(INotifyPropertyChanged item in items) {
                if(item != null) {
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        private void UnRegisterPropertyChanged(IList items) {
            foreach(INotifyPropertyChanged item in items) {
                if(item != null) {
                    item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}