// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Android.App;
using Android.Content;
using Android.Print;
using Android.Webkit;
using Android.Widget;

namespace TaskrAndroid.Utils
{
    /// <summary>
    /// A helper for printing the current tasks.
    /// Will automatically be blocked by MAM if necessary.
    /// </summary>
    public class PrintHelper
    {
        private Activity activity;

        public PrintHelper(Activity activity)
        {
            this.activity = activity;
        }

        private class PrinterWebViewClient : WebViewClient
        {
            private PrintHelper printer;
            public PrinterWebViewClient(PrintHelper printer)
            {
                this.printer = printer;
            }

            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                return false;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                printer.CreateWebPrintJob(view);
            }
        }

        /// <summary>
        /// Prints the tasks formatted as an HTML document.
        /// </summary>
        /// <remarks>
        /// The only ways to print from an Android app are to: 
        ///     1) print a photo
        ///     2) print a website
        ///     3) manually draw the document in a PDF
        /// Making an HTML document to print is the easiest and most appropriate option for this app.
        /// </remarks>
        public void PrintTasks()
        {
            // Set up a WebView to print automatically
            WebView webView = new WebView(activity);
            webView.SetWebViewClient(new PrinterWebViewClient(this));

            // Set the content of the view to be the HTML document we want to print.
            webView.LoadData(TaskManager.CreateHTMLDocument(), "text/HTML", "UTF-8");
        }

        /// <summary>
        /// Creates the web print job.
        /// </summary>
        /// <param name="webView">The web view to print.</param>
        private void CreateWebPrintJob(WebView webView)
        {
            // Create the printing resources
            PrintManager printManager = (PrintManager)activity.GetSystemService(Context.PrintService);

            if (printManager != null)
            {
                // Create the print job with the name and adapter
                string jobName = activity.GetString(Resource.String.print_name);
                PrintDocumentAdapter printAdapter = webView.CreatePrintDocumentAdapter(jobName);

                printManager.Print(jobName, printAdapter, null);
            }
            else
            {
                Toast.MakeText(activity, Resource.String.print_failure, ToastLength.Long).Show();
            }
        }
    }
}