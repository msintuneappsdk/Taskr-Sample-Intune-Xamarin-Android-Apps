// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.Content;
using Android.Print;
using Android.Webkit;
using Android.Widget;
using Microsoft.Intune.Mam.Client.Print;
using System;
using TaskrForms.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(PrinterUtility))]
namespace TaskrForms.Droid
{
    /// <summary>
    /// A utility for printing the current tasks.
    /// </summary>
    class PrinterUtility : IPrintUtility
    {
        /// <summary>
        /// Prints the tasks formatted as an HTML document.
        /// </summary>
        /// <remarks>
        /// Example of MAM policy - allow printing.
        /// Will be automatically blocked by MAM if necessary.
        /// </remarks>
        /// <param name="doc">The formatted HTML string representation of the current tasks.</param>
        public void Print(string doc)
        {
            try
            {
                // Set up a WebView to print automatically
                WebView webView = new WebView(Xamarin.Forms.Forms.Context);
                webView.SetWebViewClient(new PrinterWebViewClient(this));

                // Set the content of the view to be the HTML document we want to print.
                webView.LoadData(doc, "text/HTML", "UTF-8");
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show();
            }
        }

        private class PrinterWebViewClient : WebViewClient
        {
            private PrinterUtility printer;
            public PrinterWebViewClient(PrinterUtility printer)
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
        /// Creates the web print job.
        /// </summary>
        /// <param name="webView">The web view to print.</param>
        private void CreateWebPrintJob(WebView webView)
        {
            // Create the printing resources
            PrintManager printManager = (PrintManager)Xamarin.Forms.Forms.Context.GetSystemService(Context.PrintService);

            if (printManager == null)
            {
                Toast.MakeText(Application.Context, Resource.String.print_failure, ToastLength.Long).Show();
                return;
            }

            try
            {
                // Create the print job with the name and adapter
                PrintDocumentAdapter printAdapter = webView.CreatePrintDocumentAdapter(Application.Context.GetString(Resource.String.print_name));
                MAMPrintManagement.Print(printManager, Application.Context.GetString(Resource.String.print_name), printAdapter, null);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show();
            }
        }
    }
}