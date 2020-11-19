using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;

namespace DeleteNetworkList
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<keyvalue> list = new ObservableCollection<keyvalue>();
        ICollectionView CollectionView;

        public MainWindow()
        {
            InitializeComponent();
        }

        public class keyvalue
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Name { get; set; }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        void Search()
        {
            list.Clear();
            RegistryKey rParentKey = null;
            try
            {
                string baseKeyName = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles\";

                // すべてのサブ・キー名を取得する
                rParentKey = localKey.OpenSubKey(baseKeyName);

                string[] arySubKeyNames = rParentKey.GetSubKeyNames();

                foreach (string subKeyName in arySubKeyNames)
                {
                    //Console.WriteLine(subKeyName);
                    var value = localKey.OpenSubKey(baseKeyName + subKeyName).GetValue("ProfileName").ToString();
                    var key = baseKeyName;
                    keyvalue item = new keyvalue { Key = key, Value = value, Name = subKeyName };
                    list.Add(item);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally
            {
                rParentKey.Close();
            }
        }

        RegistryKey localKey = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NetworkList.DataContext = list;
            
            if (Environment.Is64BitOperatingSystem)
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            CollectionView = CollectionViewSource.GetDefaultView(list);
            CollectionView.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkList.SelectedItems.Count <= 0) return;
            if (!dialog()) return;

            RegistryKey regKey = null;
            try
            {                
                foreach (keyvalue item in NetworkList.SelectedItems)
                {
                    regKey = localKey.OpenSubKey(item.Key, true);
                    if (regKey != null)
                    {
                        regKey.DeleteSubKeyTree(item.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (regKey != null)
                {
                    regKey.Close();
                    Search();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            localKey.Close();
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            //ソート順の変更
            CollectionView = CollectionViewSource.GetDefaultView(list);
            try
            {
                if (CollectionView.SortDescriptions[0].Direction == ListSortDirection.Descending)
                {
                    CollectionView.SortDescriptions.Clear();
                    CollectionView.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
                }
                else
                {
                    CollectionView.SortDescriptions.Clear();
                    CollectionView.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Descending));
                }
            }
            catch
            {
                CollectionView.SortDescriptions.Clear();
                CollectionView.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
            }
        }

        bool dialog()
        {
            MessageBoxResult result = MessageBox.Show("レジストリの変更を伴う操作です。本当に実行しますか？\r\nThe operation, which involves changing the registry.\r\nAre you sure you want to do it ? ",
            "警告/Warning",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

            if (result == MessageBoxResult.Yes)
            {
                return true;
            }
            else if (result == MessageBoxResult.No)
            {
                return false;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

    }
}
