﻿using NBi.Core.ResultSet.Combination;
using NBi.Core.ResultSet.Resolver;
using NBi.Core.Scalar.Resolver;
using NBi.Core.Sequence.Resolver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBi.Core.Injection;
using NBi.Core.ResultSet;
using Moq;

namespace NBi.Testing.Core.ResultSet.Resolver
{
    public class IfUnavailableResultSetResolverTest
    {
        [Test]
        public void Execute_PrimaryFailing_SecondaryExecuted()
        {
            var primary = Mock.Of<IResultSetResolver>();
            Mock.Get(primary).Setup(x => x.Execute()).Throws(new ResultSetUnavailableException(null));
            var secondary = Mock.Of<IResultSetResolver>();
            var expectedRs = new NBi.Core.ResultSet.ResultSet();
            Mock.Get(secondary).Setup(x => x.Execute()).Returns(expectedRs);

            var args = new IfUnavailableResultSetResolverArgs(primary, secondary);
            var resolver = new IfUnavailableResultSetResolver(args);

            var rs = resolver.Execute();
            Mock.Get(primary).Verify(x => x.Execute(), Times.Once);
            Mock.Get(secondary).Verify(x => x.Execute(), Times.Once);
            Assert.That(rs, Is.EqualTo(expectedRs));
        }

        [Test]
        public void Execute_PrimarySuccessful_SecondaryNotExecuted()
        {
            var expectedRs = new NBi.Core.ResultSet.ResultSet();
            var primary = Mock.Of<IResultSetResolver>();
            Mock.Get(primary).Setup(x => x.Execute()).Returns(expectedRs);
            var secondary = Mock.Of<IResultSetResolver>();
            Mock.Get(secondary).Setup(x => x.Execute()).Throws(new ResultSetUnavailableException(null));

            var args = new IfUnavailableResultSetResolverArgs(primary, secondary);
            var resolver = new IfUnavailableResultSetResolver(args);

            var rs = resolver.Execute();
            Mock.Get(primary).Verify(x => x.Execute(), Times.Once);
            Mock.Get(secondary).Verify(x => x.Execute(), Times.Never);
            Assert.That(rs, Is.EqualTo(expectedRs));
        }

        [Test]
        public void Execute_PrimaryFailingSecondaryFailing_TertiaryNotExecuted()
        {
            var expectedRs = new NBi.Core.ResultSet.ResultSet();
            var primary = Mock.Of<IResultSetResolver>();
            Mock.Get(primary).Setup(x => x.Execute()).Throws(new ResultSetUnavailableException(null));
            var secondary = Mock.Of<IResultSetResolver>();
            Mock.Get(secondary).Setup(x => x.Execute()).Throws(new ResultSetUnavailableException(null));
            var tertiary = Mock.Of<IResultSetResolver>();
            Mock.Get(tertiary).Setup(x => x.Execute()).Returns(expectedRs);

            var secondaryArgs = new IfUnavailableResultSetResolverArgs(secondary, tertiary);
            var args = new IfUnavailableResultSetResolverArgs(primary, new IfUnavailableResultSetResolver(secondaryArgs));
            var resolver = new IfUnavailableResultSetResolver(args);

            var rs = resolver.Execute();
            Mock.Get(primary).Verify(x => x.Execute(), Times.Once);
            Mock.Get(secondary).Verify(x => x.Execute(), Times.Once);
            Mock.Get(tertiary).Verify(x => x.Execute(), Times.Once);
            Assert.That(rs, Is.EqualTo(expectedRs));
        }
    }
}