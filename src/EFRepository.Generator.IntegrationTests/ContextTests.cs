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
				.ShouldNotBeNull()
				.Count().ShouldBe(5);

			usersQueryable.ByCreatedOnDate(now.AddDays(-5))
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByCreatedIsBefore(now.AddHours(-5.5))
				.ShouldNotBeNull()
				.Count().ShouldBe(6);

			usersQueryable.ByCreatedBetween(start: now.AddHours(-5.5), end: now.AddHours(-2.5))
				.ShouldNotBeNull()
				.Count().ShouldBe(3);

			// String functions
			usersQueryable.ByAddress("1 Fake St.")
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNotNull()
				.ShouldNotBeNull()
				.Count().ShouldBe(10);

			usersQueryable.ByAddressIsNull()
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByNameIsNull()
				.ShouldNotBeNull()
				.Count().ShouldBe(0);

			usersQueryable.ByNameIsNullOrWhiteSpace()
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByPhoneIsNullOrWhiteSpace()
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNullOrWhiteSpace()
				.ShouldNotBeNull()
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNotNullOrWhiteSpace()
				.ShouldNotBeNull()
				.Count().ShouldBe(10);

			usersQueryable.ByAddressStartsWith("1")
				.ShouldNotBeNull()
				.Count().ShouldBe(2);

			usersQueryable.ByAddressEndsWith("St.")
				.ShouldNotBeNull()
				.Count().ShouldBe(10);

			usersQueryable.ByAddressContains("Fake")
				.ShouldNotBeNull()
				.Count().ShouldBe(10);
		}
	}
}