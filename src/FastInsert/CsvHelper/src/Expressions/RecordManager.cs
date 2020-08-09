// Copyright 2009-2020 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;

namespace CsvHelper.Expressions
{
	/// <summary>
	/// Manages record manipulation.
	/// </summary>
	public class RecordManager
	{
		private readonly RecordWriterFactory recordWriterFactory;

		/// <summary>
		/// Initializes a new instance using the given reader.
		/// </summary>
		/// <param name="reader"></param>
		public RecordManager()
		{
        }

		/// <summary>
		/// Initializes a new instance using the given writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public RecordManager(CsvWriter writer)
		{
			recordWriterFactory = ObjectResolver.Current.Resolve<RecordWriterFactory>(writer);
		}

		/// <summary>
		/// Writes the given record to the current writer row.
		/// </summary>
		/// <typeparam name="T">The type of the record.</typeparam>
		/// <param name="record">The record.</param>
		public void Write<T>(T record)
		{
			var recordWriter = recordWriterFactory.MakeRecordWriter(record);
			recordWriter.Write(record);
		}
	}
}
