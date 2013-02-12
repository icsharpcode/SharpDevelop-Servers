using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtefactsSite.ViewModels
{
    public class ArtefactViewModel
    {
        private string _fileName;
        private DateTime _internalCreationDate;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string CreationDate
        {
            get { return _internalCreationDate.ToString(); }
        }

        public DateTime InternalCreationDate
        {
            get { return _internalCreationDate; }
            set { _internalCreationDate = value; }
        }
    }
}