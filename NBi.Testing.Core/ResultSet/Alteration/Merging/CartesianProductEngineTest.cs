﻿using NBi.Core.ResultSet;
using NBi.Core.ResultSet.Alteration;
using NBi.Core.ResultSet.Alteration.Merging;
using NBi.Core.ResultSet.Resolver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rs = NBi.Core.ResultSet;

namespace NBi.Testing.Core.ResultSet.Alteration.Merging
{
    public class CartesianProductEngineTest
    {
        private (Rs.ResultSet firstRs, IResultSetService service) Initialize(int count)
        {
            var dataTable = new DataTable() { TableName = "MyTable" };
            dataTable.Columns.Add(new DataColumn("Id"));
            dataTable.Columns.Add(new DataColumn("Numeric value", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Boolean value", typeof(bool)));
            for (int i = 0; i < 20; i++)
                dataTable.LoadDataRow(new object[] { "Alpha", i, true }, false);
            dataTable.AcceptChanges();
            var rs1 = new Rs.ResultSet();
            rs1.Load(dataTable);

            IResultSetResolver resolver = null;
            if (count > 0)
            {
                var list = new List<object[]>();
                for (int i = 0; i < count; i++)
                    list.Add(new object[] { new DateTime(2020, 1, i + 1), i });
                var args = new ObjectsResultSetResolverArgs(list.ToArray());
                resolver = new ObjectsResultSetResolver(args);
            }
            else
            {
                var args = new EmptyResultSetResolverArgs(new[] { new ColumnNameIdentifier("one"), new ColumnNameIdentifier("two") });
                resolver = new EmptyResultSetResolver(args);
            }
            return (rs1, new ResultSetService(resolver.Execute, new List<Alter>()));
        }

        [Test()]
        public void Execute_TwentyRowsAndOneRow_TwentyRows()
        {
            var (firstRs, secondRs) = Initialize(1);

            var args = new CartesianProductArgs(secondRs);
            var merge = new CartesianProductEngine(args);
            merge.Execute(firstRs);

            Assert.That(firstRs.Rows.Count, Is.EqualTo(20));
            Assert.That(firstRs.Columns.Count, Is.EqualTo(5));
        }

        [Test()]
        public void Execute_TwentyRowsAndFiveRow_HundredRows()
        {
            var (firstRs, secondRs) = Initialize(5);

            var args = new CartesianProductArgs(secondRs);
            var merge = new CartesianProductEngine(args);
            merge.Execute(firstRs);

            Assert.That(firstRs.Rows.Count, Is.EqualTo(100));
            Assert.That(firstRs.Columns.Count, Is.EqualTo(5));
        }

        [Test()]
        public void Execute_TwentyRowsAndZeroRow_ZeroRows()
        {
            var (firstRs, secondRs) = Initialize(0);

            var args = new CartesianProductArgs(secondRs);
            var merge = new CartesianProductEngine(args);
            merge.Execute(firstRs);

            Assert.That(firstRs.Rows.Count, Is.EqualTo(0));
            Assert.That(firstRs.Columns.Count, Is.EqualTo(5));
        }
    }
}
