using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GroupMeClient.Core.Services
{
    public class MessageBoxParams
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public Buttons MessageBoxButtons { get; set; }

        public Icon MessageBoxIcons { get; set; }

        public enum Buttons
        {
            Ok,
        }

        public enum Icon
        {
            None,
            Success,
            Error,
            Warning,
        }
    }
}
