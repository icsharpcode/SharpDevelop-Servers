using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Import;

namespace CollectorServiceLibrary.Tests
{
    [TestFixture]
    public class ImportMessagesToSqlServerTestfixture
    {
        [Test]
        [ExpectedException(typeof(System.Runtime.Serialization.SerializationException))]
        public void FailWithSchemaException()
        {
            string filename = @"..\..\..\..\SampleData\_SerializationException_Test_6e5cec84-728d-4202-b957-bfa13e27d72e.xml.gz";
            ImportMessagesToSqlServer.ImportSingleMessage(filename);
        }
    }
}
