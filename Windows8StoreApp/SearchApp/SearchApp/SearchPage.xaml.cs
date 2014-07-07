﻿using SplunkSearch.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Splunk.Client;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
namespace SplunkSearch
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private CancellationTokenSource cancelSearchTokenSource;

        //private string searchTimeConstraint = "All Time";
        private string searchEarliestTime = null;
        private string searchLatestTime = null;

        private List<ResultData> allResults = new List<ResultData>();
        private int itemsPerPage = 100;
        private int totalPage = 0;
        private int currentShownPageIndex = -1;

        private List<object> comboBoxItems = new List<object>();
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public SearchPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            if (MainPage.SplunkService != null)
            {
                UserName1.Text = string.Format("User: ");
                UserName2.Text = string.Format(" {0}", loginUser);
                HostName1.Text = string.Format("Server:");
                HostName2.Text = string.Format(" {0}", MainPage.SplunkService.Server.Context.Host);
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        public static string loginUser { get; set; }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            cancelSearchTokenSource = new CancellationTokenSource();
            this.TimeSelectComboBox_DropDownClosed(null, null);

            string searchStr = SearchInput.Text.Trim();

            if (!searchStr.StartsWith("search ", StringComparison.OrdinalIgnoreCase))
            {
                searchStr = "search " + searchStr;
            }

            titleGrid.Visibility = Visibility.Collapsed;
            this.PageContentSearchInProgress();
            pagelink.Children.Clear();
            this.allResults = new List<ResultData>();
            this.totalPage = 0;
            this.currentShownPageIndex = -1;
            this.ShowResultPage(new List<ResultData>(), 0, 0);
            this.cancelSearchTokenSource = new CancellationTokenSource();

            try
            {
                if (TimeSelectComboBox.SelectedIndex == 1)
                {
                    this.DisplaySearchPreviewResult(searchStr);
                }
                else
                {
                    this.DisplaySearchResult(searchStr);
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog(ex.ToString(), "Error in Search");
                messageDialog.ShowAsync();

                titleGrid.Visibility = Visibility.Collapsed;
                this.PageContentReset();
            }
        }

        private async void DisplaySearchPreviewResult(string searchStr)
        {
            int resultCount = 0;
            ObservableCollection<ResultData> resultDatas = new ObservableCollection<ResultData>();
            resultListView.DataContext = new CollectionViewSource { Source = resultDatas };
            JobArgs args = new JobArgs();
            args.EarliestTime = this.searchEarliestTime;
            args.LatestTime = this.searchLatestTime;
            args.SearchMode = SearchMode.RealTime;
            Job realtimeJob = await MainPage.SplunkService.Jobs.CreateAsync(searchStr, args: args);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            do
            {
                using (SearchResultStream stream = await realtimeJob.GetSearchPreviewAsync())
                {
                    titleGrid.Visibility = Visibility.Visible;

                    try
                    {
                        foreach (SearchResult result in stream)
                        {
                            List<string> results = this.ParseResult(result);
                            resultDatas.Add(new ResultData(++resultCount, results[0], results[1]));

                            //TODO: need to do paging
                            if (resultCount > 2000 || this.cancelSearchTokenSource.Token.IsCancellationRequested) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //the stream has some broken fields
                        // Enumeration ended prematurely : System.IO.InvalidDataException: Read <fieldOrder> where </fieldOrder> was expected.   
                    }

                    await Task.Delay(1000);
                }

            } while (resultCount == 0 && watch.Elapsed.TotalSeconds <= 5 && !this.cancelSearchTokenSource.Token.IsCancellationRequested);
            //using (SearchResultStream stream = await realtimeJob.GetSearchPreviewAsync())
            //{
            //    this.DisplayResult(stream);
            //}

            this.PageContentReset();
            await realtimeJob.CancelAsync();
        }
      
        private async void DisplaySearchResult(string searchStr)
        {
            SearchExportArgs jobArgs = new SearchExportArgs();

            if (this.searchEarliestTime != null)
            {
                jobArgs.EarliestTime = this.searchEarliestTime;
            }

            if (this.searchLatestTime != null)
            {
                jobArgs.LatestTime = this.searchLatestTime;
            }

            int resultCount = 0;
            this.allResults.Clear();

            using (SearchResultStream resultStream = await MainPage.SplunkService.ExportSearchResultsAsync(searchStr, jobArgs))
            {
                titleGrid.Visibility = Visibility.Visible;

                //start a task to get all results
                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (SearchResult result in resultStream)
                        {
                            List<string> results = this.ParseResult(result);
                            allResults.Add(new ResultData(++resultCount, results[0], results[1]));

                            if (resultCount > this.totalPage * itemsPerPage)
                            {
                                this.totalPage++;
                            }

                            if (this.cancelSearchTokenSource.Token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //the stream has some broken fields
                        // Enumeration ended prematurely : System.IO.InvalidDataException: Read <fieldOrder> where </fieldOrder> was expected.   
                    }
                });


                bool showFirstPage = false;
                do
                {
                    if (this.allResults.Count > 0)
                    {
                        if (!showFirstPage && this.ShowResultPage(this.allResults, 0, this.itemsPerPage))
                        {
                            showFirstPage = true;
                        }

                        if (this.currentShownPageIndex < 0)
                        {
                            ShowPagingLink(0);
                        }
                    }

                    await Task.Delay(1000);
                } while (!(task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled));

                if (!showFirstPage)
                {
                    this.ShowResultPage(this.allResults, 0, this.itemsPerPage);
                }

                ShowPagingLink(0);

                this.PageContentReset();
            }
        }
        
    

        /// <summary>
        /// show paging links
        /// </summary>
        /// <param name="startPageIndex">0 based page index</param>
        /// <param name="totalPage"> total number of pages</param>
        /// <param name="totalLinkShown"></param>
        private void ShowPagingLink(int currentPageIndex)
        {
            if (this.totalPage <= 1)
            {
                return;
            }

            this.currentShownPageIndex = currentPageIndex;

            pagelink.Children.Clear();
            //show hyperlinks around the currentIndex

            if (currentPageIndex != 0)
            {
                HyperlinkButton hyperlink = new HyperlinkButton();
                hyperlink.Content = "<Prev";
                hyperlink.CommandParameter = (currentPageIndex).ToString();
                hyperlink.Click += new RoutedEventHandler(PageLinkClick);

                pagelink.Children.Add(hyperlink);

                hyperlink = new HyperlinkButton();
                hyperlink.Content = "1";
                hyperlink.Click += new RoutedEventHandler(PageLinkClick);
                pagelink.Children.Add(hyperlink);
            }
            else
            {
                TextBlock textblock = new TextBlock();
                textblock.Text = "1";
                pagelink.Children.Add(textblock);
            }

            if (currentPageIndex > 4)
            {
                TextBlock textblock = new TextBlock();
                textblock.Text = "...";
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                pagelink.Children.Add(textblock);
            }

            for (int i = currentPageIndex - 3; i <= currentPageIndex; i++)
            {
                if (i <= 0)
                {
                    continue;
                }

                if (i != currentPageIndex)
                {
                    HyperlinkButton hyperlink = new HyperlinkButton();
                    int content = i + 1;
                    hyperlink.Content = content;
                    hyperlink.Click += new RoutedEventHandler(PageLinkClick);
                    pagelink.Children.Add(hyperlink);
                }
                else
                {
                    TextBlock textblock = new TextBlock();
                    int content = i + 1;
                    textblock.Text = content.ToString();
                    pagelink.Children.Add(textblock);
                }
            }

            int showPageTill = 0; //1 based pageIndex
            if (currentPageIndex < 5)
            {
                showPageTill = totalPage > 10 ? 10 : totalPage;
            }
            else
            {
                showPageTill = currentPageIndex + 4 > totalPage ? totalPage : currentPageIndex + 4;
            }

            for (int i = currentPageIndex + 1; i < showPageTill; i++)
            {
                HyperlinkButton hyperlink = new HyperlinkButton();
                int content = i + 1;
                hyperlink.Content = content;
                hyperlink.Click += new RoutedEventHandler(PageLinkClick);
                pagelink.Children.Add(hyperlink);
            }

            if (showPageTill < totalPage)
            {
                TextBlock textblock = new TextBlock();
                textblock.Text = "...";
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                pagelink.Children.Add(textblock);
            }

            if (currentPageIndex + 1 < showPageTill)
            {
                HyperlinkButton hyperlink = new HyperlinkButton();
                hyperlink.Content = "Next>";
                hyperlink.CommandParameter = (currentPageIndex+2).ToString();
                hyperlink.Click += new RoutedEventHandler(PageLinkClick);
                pagelink.Children.Add(hyperlink);
            }
        }

        private void PageLinkClick(object sender, RoutedEventArgs e)
        {
            string str;
            if (((HyperlinkButton)sender).CommandParameter != null)
            {
                str = ((HyperlinkButton)sender).CommandParameter.ToString();
            }
            else
            {
                str = ((HyperlinkButton)sender).Content.ToString();
            }

            int page = int.Parse(str) - 1;
            ShowResultPage(this.allResults, page, this.itemsPerPage);
            ShowPagingLink(page);
        }

        private bool ShowResultPage(List<ResultData> allResults, int pageIndex, int itemsPerPage)
        {
            int currentPageMinValue = pageIndex * itemsPerPage;
            int currentPageMaxValue = (pageIndex + 1) * itemsPerPage;
            currentPageMaxValue = allResults.Count < currentPageMaxValue ? allResults.Count : currentPageMaxValue;

            ObservableCollection<ResultData> resultDatas = new ObservableCollection<ResultData>();
            resultListView.DataContext = new CollectionViewSource { Source = resultDatas };

            for (int i = currentPageMinValue; i < currentPageMaxValue; i++)
            {
                resultDatas.Add(allResults[i]);
            }

            if(pageIndex==0 && currentPageMaxValue>=itemsPerPage)
            {
                return true;
            }

            return false;
        }

        private void GetAllResults(SearchResultStream resultStream, ref List<ResultData> allResults)
        {
            int resultCount = 0;

            try
            {
                foreach (SearchResult result in resultStream)
                {
                    List<string> results = this.ParseResult(result);
                    allResults.Add(new ResultData(++resultCount, results[0], results[1]));
                }
            }
            catch (Exception ex)
            {
                //the stream has some broken fields
                // Enumeration ended prematurely : System.IO.InvalidDataException: Read <fieldOrder> where </fieldOrder> was expected.   
            }
        }

        private void SearchCancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelSearchTokenSource.Cancel();
            SearchCancel.Content = "Cancelling...";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private List<string> ParseResult(SearchResult searchResult)
        {
            List<string> results = new List<string>();
            string rawData = searchResult.SegmentedRaw;

            //DateTime time = DateTime.Parse(searchResult["_time"]);
            //string format = "yyyy/M/d hh:mm:ss.fff";
            //results.Add(string.Format("{0}-{1}", ++eventCount, time.ToString(format)));

            string time = searchResult.GetValue("_time");
            time = time.Replace("Pacific Summer Time", "PST");
            results.Add(string.Format("{0}", time));

            rawData = rawData.Trim();
            //remove <v xml:space="preserve" trunc="0">
            if (rawData.StartsWith("<v xml:space="))
            {
                rawData = rawData.Remove(0, 34);
            }

            //remove </v>
            if (rawData.EndsWith("</v>"))
            {
                rawData = rawData.Substring(0, rawData.Length - 4);
            }

            results.Add(rawData);

            return results;
        }

        private void PageContentReset()
        {
            SearchSubmit.Content = "Search";
            SearchCancel.Content = "Cancel";
            SearchCancel.Visibility = Visibility.Collapsed;
            searchInProgress.IsActive = false;
        }

        private void PageContentSearchInProgress()
        {
            SearchCancel.Content = "Cancel";
            SearchCancel.Visibility = Visibility.Visible;
            SearchSubmit.Content = "Searching";
            searchInProgress.IsActive = true;
        }

        private class ResultData
        {
            public string Time { get; set; }
            public string Event { get; set; }
            public int Index { get; set; }

            public ResultData(int index, string time, string theEvent)
            {
                this.Index = index;
                this.Time = time;
                this.Event = theEvent;
            }
        }

        private void ApplyRelativeTimeSearchClick()
        {
            int relativeTime = 0;
            try
            {
                if ((relativeTime = int.Parse(RelativeEarlistTimeValue.Text.Trim().TrimEnd(' '))) <= 0)
                {
                    throw new Exception("value must be positive value");
                }
            }
            catch
            {
                string msg = "Invalid input: value must be positive integer";
                Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog(msg, "Error in Input");
                messageDialog.ShowAsync();
            }

            string unit = "s";
            switch (RelativeEarlistTimeValueUnit.SelectedIndex)
            {
                case 1: unit = "m"; break;
                case 2: unit = "h"; break;
                case 3: unit = "d"; break;
            }

            this.searchEarliestTime = string.Format("rt-{0}{1}", relativeTime, unit);
            this.searchLatestTime = "rt";
        }

        private void ApplyDateTimeRangeClick()
        {
            DateTime start = new DateTime(EarlistDate.Date.Year, EarlistDate.Date.Month, EarlistDate.Date.Day).AddSeconds(EarlistTime.Time.TotalSeconds);
            DateTime end = new DateTime(LatestDate.Date.Year, LatestDate.Date.Month, LatestDate.Date.Day).AddSeconds(LatestTime.Time.TotalSeconds);
            this.searchEarliestTime = start.ToString("yyyy-MM-ddThh:mm:ss");
            this.searchLatestTime = end.ToString("yyyy-MM-ddThh:mm:ss");
            if (start >= end)
            {
                string msg = "Latest time must be greater than earlist time";
                Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog(msg, "Error in Input");
                messageDialog.ShowAsync();
            }
        }

        private void ApplyAdvancedTimeSelectionClick()
        {
            try
            {
                DateTime start = DateTime.Parse(customerEarlistInput.Text.Trim().TrimEnd(' '));
                DateTime end = DateTime.Parse(customerLatestInput.Text.Trim().TrimEnd(' '));
                this.searchEarliestTime = start.ToString("yyyy-MM-ddThh:mm:ss");
                this.searchLatestTime = end.ToString("yyyy-MM-ddThh:mm:ss");
                if (start >= end)
                {
                    throw new Exception("Latest time must be greater than earlist time");
                }
            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Error in Input");
                messageDialog.ShowAsync();
            }
        }

        private void TimeSelectComboBox_DropDownClosed(object sender, object e)
        {
            if (TimeSelectComboBox != null)
            {
                if (TimeSelectComboBox.SelectedIndex == 0)
                {
                    //"All Time";
                    this.searchLatestTime = null;
                    this.searchLatestTime = null;
                }

                if (TimeSelectComboBox.SelectedIndex == 1)
                {
                    //Relative time
                    this.ApplyRelativeTimeSearchClick();
                }

                if (TimeSelectComboBox.SelectedIndex == 2)
                {
                    //Date and Time range
                    this.ApplyDateTimeRangeClick();
                }
                else if (TimeSelectComboBox.SelectedIndex == 3)
                {
                    //Advanced
                    this.ApplyAdvancedTimeSelectionClick();
                }
            }
        }
    }
}
