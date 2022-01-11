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
			});

			var usersQueryable = users.AsQueryable();

			var user = usersQueryable.ById(1).FirstOrDefault();

			user.ShouldNotBeNull();
			user.Id.ShouldBe(1);

			user = usersQueryable.ByIsDeleted(false).FirstOrDefault();

			user.ShouldNotBeNull();
			user.IsDeleted.ShouldBeFalse();

			user = usersQueryable.ByScore(3.333).FirstOrDefault();

			user.ShouldNotBeNull();
			user.Score.ShouldBe(3.333);

			var filteredUsers = usersQueryable.WhereCreatedIsAfter(now.AddHours(-5.5));

			filteredUsers.ShouldNotBeNull();
			filteredUsers.Count().ShouldBe(5);

			filteredUsers = usersQueryable.WhereCreatedIsBefore(now.AddHours(-5.5));

			filteredUsers.ShouldNotBeNull();
			filteredUsers.Count().ShouldBe(5);

			filteredUsers = usersQueryable.WhereCreatedIsBetween(start: now.AddHours(-5.5), end: now.AddHours(-2.5));

			filteredUsers.ShouldNotBeNull();
			filteredUsers.Count().ShouldBe(3);
		}
	}
}