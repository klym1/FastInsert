// Copyright 2009-2020 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CsvHelper.Expressions
{
	/// <summary>
	/// Manages expression creation.
	/// </summary>
	public class ExpressionManager
	{
		private readonly CsvWriter writer;

		/// <summary>
		/// Initializes a new instance using the given writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public ExpressionManager(CsvWriter writer)
		{
			this.writer = writer;
		}


        /// <summary>
		/// Creates a member expression for the given member on the record.
		/// This will recursively traverse the mapping to find the member
		/// and create a safe member accessor for each level as it goes.
		/// </summary>
		/// <param name="recordExpression">The current member expression.</param>
		/// <param name="mapping">The mapping to look for the member to map on.</param>
		/// <param name="memberMap">The member map to look for on the mapping.</param>
		/// <returns>An Expression to access the given member.</returns>
		public virtual Expression CreateGetMemberExpression(Expression recordExpression, ClassMap mapping, MemberMap memberMap)
		{
			if (mapping.MemberMaps.Any(mm => mm == memberMap))
			{
				// The member is on this level.
				if (memberMap.Data.Member is PropertyInfo)
				{
					return Expression.Property(recordExpression, (PropertyInfo)memberMap.Data.Member);
				}

				if (memberMap.Data.Member is FieldInfo)
				{
					return Expression.Field(recordExpression, (FieldInfo)memberMap.Data.Member);
				}
			}

			// The member isn't on this level of the mapping.
			// We need to search down through the reference maps.
			foreach (var refMap in mapping.ReferenceMaps)
			{
				var wrapped = refMap.Data.Member.GetMemberExpression(recordExpression);
				var memberExpression = CreateGetMemberExpression(wrapped, refMap.Data.Mapping, memberMap);
				if (memberExpression == null)
				{
					continue;
				}

				if (refMap.Data.Member.MemberType().GetTypeInfo().IsValueType)
				{
					return memberExpression;
				}

				var nullCheckExpression = Expression.Equal(wrapped, Expression.Constant(null));

				var isValueType = memberMap.Data.Member.MemberType().GetTypeInfo().IsValueType;
				var isGenericType = isValueType && memberMap.Data.Member.MemberType().GetTypeInfo().IsGenericType;
				Type memberType;
				if (isValueType && !isGenericType && !writer.Context.WriterConfiguration.UseNewObjectForNullReferenceMembers)
				{
					memberType = typeof(Nullable<>).MakeGenericType(memberMap.Data.Member.MemberType());
					memberExpression = Expression.Convert(memberExpression, memberType);
				}
				else
				{
					memberType = memberMap.Data.Member.MemberType();
				}

				var defaultValueExpression = isValueType && !isGenericType
					? (Expression)Expression.New(memberType)
					: Expression.Constant(null, memberType);
				var conditionExpression = Expression.Condition(nullCheckExpression, defaultValueExpression, memberExpression);
				return conditionExpression;
			}

			return null;
		}

		/// <summary>
		/// Creates an instance of the given type using <see cref="ReflectionHelper.CreateInstance"/> (in turn using the ObjectResolver), then assigns
		/// the given member assignments to that instance.
		/// </summary>
		/// <param name="recordType">The type of the record we're creating.</param>
		/// <param name="assignments">The member assignments that will be assigned to the created instance.</param>
		/// <returns>A <see cref="BlockExpression"/> representing the instance creation and assignments.</returns>
		public virtual BlockExpression CreateInstanceAndAssignMembers(Type recordType, List<MemberAssignment> assignments)
		{
			var expressions = new List<Expression>();
			var createInstanceMethod = typeof(ReflectionHelper).GetMethod(nameof(ReflectionHelper.CreateInstance), new Type[] { typeof(Type), typeof(object[]) });
			var instanceExpression = Expression.Convert(Expression.Call(createInstanceMethod, Expression.Constant(recordType), Expression.Constant(new object[0])), recordType);
			var variableExpression = Expression.Variable(instanceExpression.Type, "instance");
			expressions.Add(Expression.Assign(variableExpression, instanceExpression));
			expressions.AddRange(assignments.Select(b => Expression.Assign(Expression.MakeMemberAccess(variableExpression, b.Member), b.Expression)));
			expressions.Add(variableExpression);
			var variables = new ParameterExpression[] { variableExpression };
			var blockExpression = Expression.Block(variables, expressions);

			return blockExpression;
		}
    }
}
