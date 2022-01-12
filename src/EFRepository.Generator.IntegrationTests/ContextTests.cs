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
				.Count().ShouldBe(5);

			usersQueryable.ByCreatedOnDate(now.AddDays(-5))
				.Count().ShouldBe(1);

			usersQueryable.ByCreatedIsBefore(now.AddHours(-5.5))
				.Count().ShouldBe(6);

			usersQueryable.ByCreatedBetween(start: now.AddHours(-5.5), end: now.AddHours(-2.5))
				.Count().ShouldBe(3);

			// String functions
			usersQueryable.ByAddress("1 Fake St.")
				.Count().ShouldBe(1);

			usersQueryable.ByAddress("1 Fake St.")
				.Count().ShouldBe(1);

			usersQueryable.ByAddressIsNotNull()
				.ByAddressStartsWith("1")
				.Count().ShouldBe(10);

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
				.ByPhone("201-111-0221")
				.Count().ShouldBe(0);

		}
	}
}


/*


namespace EFRepository.Generator.IntegrationTests
{

	public static partial class UserExtensions
	{

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Id
		/// </summary>
		/// <param name="value">The int which should equal Id</param>
		public static IQueryable<User>? ById(this IQueryable<User>? query, int? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.Id == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name is null
		/// </summary>
		public static IQueryable<User>? ByNameIsNull(this IQueryable<User>? query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Name == null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name is not null
		/// </summary>
		public static IQueryable<User>? ByNameIsNotNull(this IQueryable<User>? query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Name != null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name is null
		/// </summary>
		public static IQueryable<User>? ByNameIsNullOrWhiteSpace(this IQueryable<User>? query)
		{
			if (query == null)
				return query;

			return query.Where(n => string.IsNullOrWhiteSpace(n.Name));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name is not null
		/// </summary>
		public static IQueryable<User>? ByNameIsNotNullOrWhiteSpace(this IQueryable<User>? query)
		{
			if (query == null)
				return query;

			return query.Where(n => !string.IsNullOrWhiteSpace(n.Name));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name
		/// </summary>
		/// <param name="value">The string which Name should be equal</param>
		public static IQueryable<User>? ByName(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Name == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name contains a value
		/// </summary>
		/// <param name="value">The string which Name should contain</param>
		public static IQueryable<User>? ByNameContains(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Name != null && n.Name.Contains(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name starts with a value
		/// </summary>
		/// <param name="value">The string which Name should start with</param>
		public static IQueryable<User>? ByNameStartsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Name != null && n.Name.StartsWith(value));
		}


		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Name ends with a value
		/// </summary>
		/// <param name="value">The string which Name should end with</param>
		public static IQueryable<User>? ByNameEndsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Name != null && n.Name.EndsWith(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone is null
		/// </summary>
		public static IQueryable<User>? ByPhoneIsNull(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Phone == null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone is not null
		/// </summary>
		public static IQueryable<User>? ByPhoneIsNotNull(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Phone != null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone is null
		/// </summary>
		public static IQueryable<User> ByPhoneIsNullOrWhiteSpace(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => string.IsNullOrWhiteSpace(n.Phone));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone is not null
		/// </summary>
		public static IQueryable<User> ByPhoneIsNotNullOrWhiteSpace(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => !string.IsNullOrWhiteSpace(n.Phone));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone
		/// </summary>
		/// <param name="value">The string which Phone should be equal</param>
		public static IQueryable<User> ByPhone(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Phone == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone contains a value
		/// </summary>
		/// <param name="value">The string which Phone should contain</param>
		public static IQueryable<User>? ByPhoneContains(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Phone != null && n.Phone.Contains(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone starts with a value
		/// </summary>
		/// <param name="value">The string which Phone should start with</param>
		public static IQueryable<User>? ByPhoneStartsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Phone != null && n.Phone.StartsWith(value));
		}


		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Phone ends with a value
		/// </summary>
		/// <param name="value">The string which Phone should end with</param>
		public static IQueryable<User>? ByPhoneEndsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Phone != null && n.Phone.EndsWith(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address is null
		/// </summary>
		public static IQueryable<User>? ByAddressIsNull(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Address == null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address is not null
		/// </summary>
		public static IQueryable<User>? ByAddressIsNotNull(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => n.Address != null);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address is null
		/// </summary>
		public static IQueryable<User> ByAddressIsNullOrWhiteSpace(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => string.IsNullOrWhiteSpace(n.Address));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address is not null
		/// </summary>
		public static IQueryable<User> ByAddressIsNotNullOrWhiteSpace(this IQueryable<User> query)
		{
			if (query == null)
				return query;

			return query.Where(n => !string.IsNullOrWhiteSpace(n.Address));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address
		/// </summary>
		/// <param name="value">The string which Address should be equal</param>
		public static IQueryable<User> ByAddress(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Address == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address contains a value
		/// </summary>
		/// <param name="value">The string which Address should contain</param>
		public static IQueryable<User>? ByAddressContains(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Address != null && n.Address.Contains(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address starts with a value
		/// </summary>
		/// <param name="value">The string which Address should start with</param>
		public static IQueryable<User>? ByAddressStartsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Address != null && n.Address.StartsWith(value));
		}


		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Address ends with a value
		/// </summary>
		/// <param name="value">The string which Address should end with</param>
		public static IQueryable<User>? ByAddressEndsWith(this IQueryable<User> query, string? value)
		{
			if (query == null)
				return query;

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.Address != null && n.Address.EndsWith(value));
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Created
		/// </summary>
		/// <param name="value">The DateTime which should equal Created</param>
		public static IQueryable<User> ByCreated(this IQueryable<User> query, DateTime? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.Created == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by whether or not the provided <see cref="DateTime" /> is after Created
		/// </summary>
		/// <param name="value">The <see cref="DateTime"/> that Created should be before</param>
		public static IQueryable<EFRepository.Generator.IntegrationTests.User>? ByCreatedIsBefore(this IQueryable<User> query, DateTime? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.Created < value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by whether or not the provided <see cref="DateTime" /> is after Created
		/// </summary>
		/// <param name="value">The <see cref="DateTime"/> that Created should be after</param>
		public static IQueryable<EFRepository.Generator.IntegrationTests.User>? ByCreatedIsAfter(this IQueryable<User> query, DateTime? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.Created > value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by whether or not Created is between the two provided values.
		/// </summary>
		/// <param name="start">The <see cref="DateTime"/> that should be before Created</param>
		/// <param name="end">The <see cref="DateTime"/> that should be after Created</param>
		public static IQueryable<EFRepository.Generator.IntegrationTests.User>? ByCreatedBetween(this IQueryable<User> query, DateTime? start, DateTime? end)
		{
			if (query == null)
				return query;

			if (start != null)
				query = query.Where(n => n.Created > start);

			if (end != null)
				query = query.Where(n => n.Created < end);

			return query;
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by whether or not Created is between the two provided values.
		/// </summary>
		/// <param name="value">The <see cref="DateTime"/> that should the same date as Created, excluding time</param>
		public static IQueryable<EFRepository.Generator.IntegrationTests.User>? ByCreatedOnDate(this IQueryable<User> query, DateTime? value)
		{
			if (query == null)
				return query;

			if (value != null)
				return query.Where(n => n.Created.Date == value.Value.Date);
			else
				return query;
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by IsDeleted
		/// </summary>
		/// <param name="value">The bool which should equal IsDeleted</param>
		public static IQueryable<User> ByIsDeleted(this IQueryable<User> query, bool? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.IsDeleted == value);
		}

		/// <summary>
		/// Filter the <see cref="IQueryable"/> of User by Score
		/// </summary>
		/// <param name="value">The double which should equal Score</param>
		public static IQueryable<User> ByScore(this IQueryable<User> query, double? value)
		{
			if (query == null)
				return query;

			if (value == null)
				return query;

			return query.Where(n => n.Score == value);
		}
	}
}
*/
