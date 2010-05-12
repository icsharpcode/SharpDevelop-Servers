using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelReport.DataAccess
{
    public partial class Session
    {
        public Version AppVersion
        {
            get
            {
                return new Version(this.AppVersionMajor ?? 0, this.AppVersionMinor ?? 0, this.AppVersionBuild ?? 0, this.AppVersionRevision ?? 0);
            }
        }
    }
}
