using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Shouldly;
using System;

namespace EFRepository.Generator.IntegrationTests
{
	public class ContextTests
	{
		[Fact]
		public void UserMethodsGenerated()
		{
			var now = DateTime.Now;

			var users = Enumerable.Range(1, 10)
			.Select(i => new User
			{
				Id = i,
				Name = $"User {i}",
				Address = $"{i} Fake St.",
				Phone = new string(i.ToString().ToCharArray()[0], 10),
				Created = now.AddHours(-i),
				IsDeleted = i % 2 == 0,
				Score = double.Parse($"{i}.{i}{i}{i}")
			}).Union(new[]
			{
				new User
				{
					Id = 0,
					Name = string.Empty,
					Address = null,
					Phone = " ",
					Created = now.AddDays(-5),
					IsDeleted = false,
					Score = 9.2
				}
			});

			var usersQueryable = users.AsQueryable();

			// Int/Long Functions
			usersQueryable.ById(1).FirstOrDefault()
				.ShouldNotBeNull()
				.Id.ShouldBe(1);

			usersQueryable.ByIdGreaterThan(5)
				.Count().ShouldBe(5);

			usersQueryable.ByIdGreaterThanOrEqual(5)
				.Count().ShouldBe(6);

			usersQueryable.ByIdLessThan(5)
				.Count().ShouldBe(5);

			usersQueryable.ByIdLessThanOrEqual(5)
				.Count().ShouldBe(6);

			// Boolean Functions
			usersQueryable.ByIsDeleted(false).FirstOrDefault()
				.ShouldNotBeNull()
				.IsDeleted.ShouldBeFalse();

			// Double Functions
			usersQueryable.ByScore(3.333).FirstOrDefault()
				.ShouldNotBeNull()
				.Score.ShouldBe(3.333);

			// DateTime Functions
			usersQueryable.ByCreatedIsAfter(now.AddHours(-5.5))
				.Count().ShouldBe(5);

			usersQueryable.ByCreatedOnDate(now.AddDays(-5))
				.Count().ShouldBe(1);

			usersQueryable.ByCreatedIsBefore(now.AddHours(-5.5))
				.Count().ShouldBe(6);

			usersQueryable.ByCreatedBetween(start: now.AddHours(-5.5), end: now.AddHours(-2.5))
				.Count().ShouldBe(3);

			usersQueryable.ByRegistrationDateOnDate(now.AddDays(-5))
				.Count().ShouldBe(0);

			// String functions
			usersQueryable.ByAddress("1 Fake St.")
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNotNull()
				.ByAddressStartsWith("1")
				.Count().ShouldBe(2);

			usersQueryable.ByAddressIsNull()
				.Count().ShouldBe(1);

			usersQueryable.ByNameIsNull()
				.Count().ShouldBe(0);

			usersQueryable.ByNameIsNullOrWhiteSpace()
				.Count().ShouldBe(1);

			usersQueryable.ByPhoneIsNullOrWhiteSpace()
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNullOrWhiteSpace()
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNotNullOrWhiteSpace()
				.Count().ShouldBe(10);

			usersQueryable.ByAddressStartsWith("1")
				.Count().ShouldBe(2);

			usersQueryable.ByAddressEndsWith("St.")
				.Count().ShouldBe(10);

			usersQueryable.ByAddressContains("Fake")
				.Count().ShouldBe(10);

			// Testing chained functions
			usersQueryable.ByAddress("1 Fake St.")
				.ByAddressIsNotNull()
				.ByNameIsNotNull()
				.ByPhoneContains("801")
				.ByScoreGreaterThan(1)
				.ByScoreLessThan(100)
				.Count().ShouldBe(0);

		}
	}
}