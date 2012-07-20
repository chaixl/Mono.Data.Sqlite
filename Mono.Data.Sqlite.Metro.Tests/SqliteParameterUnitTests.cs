﻿// SqliteParameterUnitTests.cs - NUnit Test Cases for Mono.Data.Sqlite.SqliteParameter
//
// Author(s):	Thomas Zoechling <thomas.zoechling@gmx.at>


using System;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;

namespace MonoTests.Mono.Data.Sqlite
{
    [TestFixture]
    public class SqliteParameterUnitTests
    {
        readonly static string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        readonly static string _uri = Path.Combine(dbRootPath, "test.db");
        readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
        static SqliteConnection _conn = new SqliteConnection(_connectionString);

        public SqliteParameterUnitTests()
        {
        }

        [Test]
        [Category("NotWorking")]
        // fails randomly :)
        public void InsertRandomValuesWithParameter()
        {
            SqliteParameter textP = new SqliteParameter();
            textP.ParameterName = "textP";
            textP.SourceColumn = "t";

            SqliteParameter floatP = new SqliteParameter();
            floatP.ParameterName = "floatP";
            floatP.SourceColumn = "nu";

            SqliteParameter integerP = new SqliteParameter();
            integerP.ParameterName = "integerP";
            integerP.SourceColumn = "i";

            SqliteParameter blobP = new SqliteParameter();
            blobP.ParameterName = "blobP";
            blobP.SourceColumn = "b";

            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            for (int k = 0; k < random.Next(0, 100); k++)
            {
                builder.Append((char)random.Next(65536));
            }

            SqliteCommand createCommand = new SqliteCommand("CREATE TABLE t1(t  TEXT,  f FLOAT, i INTEGER, b TEXT);", _conn);
            SqliteCommand insertCmd = new SqliteCommand("DELETE FROM t1; INSERT INTO t1  (t, f, i, b ) VALUES(:textP,:floatP,:integerP,:blobP)", _conn);

            insertCmd.Parameters.Add(textP);
            insertCmd.Parameters.Add(floatP);
            insertCmd.Parameters.Add(blobP);
            insertCmd.Parameters.Add(integerP);

            string stringValue = builder.ToString();
            double doubleValue = Convert.ToDouble(random.Next(999));
            long intValue = random.Next(999);
            byte[] blobValue = System.Text.Encoding.UTF8.GetBytes("\u05D0\u05D1\u05D2" + builder.ToString());

            textP.Value = stringValue;
            floatP.Value = doubleValue;
            integerP.Value = intValue;
            blobP.Value = blobValue;

            SqliteCommand selectCmd = new SqliteCommand("SELECT * from t1", _conn);

            using (_conn)
            {
                _conn.Open();
                createCommand.ExecuteNonQuery();
                int res = insertCmd.ExecuteNonQuery();
                Assert.AreEqual(res, 1);

                using (IDataReader reader = selectCmd.ExecuteReader())
                {
                    Assert.AreEqual(reader.Read(), true);
                    Assert.AreEqual(reader["t"], stringValue);
                    Assert.AreEqual(reader["f"], doubleValue);
                    Assert.AreEqual(reader["i"], intValue);

                    object compareValue;
#if NET_2_0
					if (blobP.Value is byte[])
                        compareValue = System.Text.Encoding.UTF8.GetString(blobValue, 0, (blobValue.Length));
					else
#endif
                        compareValue = blobValue;
                    Assert.AreEqual(reader["b"], compareValue);
                    Assert.AreEqual(reader.Read(), false);
                }
            }
        }
    }
}